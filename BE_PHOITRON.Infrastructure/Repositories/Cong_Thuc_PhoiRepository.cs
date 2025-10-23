using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class Cong_Thuc_PhoiRepository : BaseRepository<Cong_Thuc_Phoi>, ICong_Thuc_PhoiRepository
    {
        public Cong_Thuc_PhoiRepository(AppDbContext db) : base(db) { }

        public async Task<bool> ExistsByCodeAsync(string maCongThuc, CancellationToken ct = default)
            => await _set.AnyAsync(x => x.Ma_Cong_Thuc == maCongThuc && !x.Da_Xoa, ct);

        public async Task<IReadOnlyList<Cong_Thuc_Phoi>> GetByQuangDauRaAsync(int idQuangDauRa, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.ID_Quang_DauRa == idQuangDauRa && !x.Da_Xoa)
                .OrderByDescending(x => x.Hieu_Luc_Tu)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Cong_Thuc_Phoi>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.Trang_Thai == 1 && !x.Da_Xoa) // Trang_Thai = 1 means active
                .ToListAsync(ct);
        }

        public async Task<bool> HasOverlappingPeriodAsync(int idQuangDauRa, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _set.Where(x => x.ID_Quang_DauRa == idQuangDauRa);
            
            if (excludeId.HasValue)
                query = query.Where(x => x.ID != excludeId.Value);

            // Check for overlapping periods
            return await query.AnyAsync(x => 
                (hieuLucDen == null && x.Hieu_Luc_Den == null) || // Both are open-ended
                (hieuLucDen == null && x.Hieu_Luc_Den > hieuLucTu) || // New is open-ended, existing ends after new starts
                (x.Hieu_Luc_Den == null && hieuLucDen > x.Hieu_Luc_Tu) || // Existing is open-ended, new ends after existing starts
                (hieuLucDen.HasValue && x.Hieu_Luc_Den.HasValue && 
                 hieuLucTu < x.Hieu_Luc_Den && hieuLucDen > x.Hieu_Luc_Tu), ct); // Both have end dates and overlap
        }

        protected override IQueryable<Cong_Thuc_Phoi> ApplySearchFilter(IQueryable<Cong_Thuc_Phoi> query, string search)
        {
            return query.Where(x => x.Ma_Cong_Thuc.Contains(search) || 
                                  (x.Ten_Cong_Thuc != null && x.Ten_Cong_Thuc.Contains(search)) ||
                                  (x.Ghi_Chu != null && x.Ghi_Chu.Contains(search)));
        }

        protected override IQueryable<Cong_Thuc_Phoi> ApplySorting(IQueryable<Cong_Thuc_Phoi> query, string sortBy, string? sortDir)
        {
            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            
            return sortBy.ToLower() switch
            {
                "ma_cong_thuc" => isDesc ? query.OrderByDescending(x => x.Ma_Cong_Thuc) : query.OrderBy(x => x.Ma_Cong_Thuc),
                "ten_cong_thuc" => isDesc ? query.OrderByDescending(x => x.Ten_Cong_Thuc) : query.OrderBy(x => x.Ten_Cong_Thuc),
                "hieu_luc_tu" => isDesc ? query.OrderByDescending(x => x.Hieu_Luc_Tu) : query.OrderBy(x => x.Hieu_Luc_Tu),
                "trang_thai" => isDesc ? query.OrderByDescending(x => x.Trang_Thai) : query.OrderBy(x => x.Trang_Thai),
                _ => query.OrderByDescending(x => x.Hieu_Luc_Tu)
            };
        }

        public override async Task<(int total, IReadOnlyList<Cong_Thuc_Phoi> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<Cong_Thuc_Phoi> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => (x.Ma_Cong_Thuc ?? "").Contains(search) ||
                                 (x.Ten_Cong_Thuc ?? "").Contains(search) ||
                                 (x.Ghi_Chu ?? "").Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && Infrastructure.Shared.CheckValidPropertyPath.IsValidPropertyPath<Cong_Thuc_Phoi>(sortBy))
            {
                var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                q = q.OrderBy(cfg, $"{sortBy} {dir}");
            }
            else
            {
                q = q.OrderByDescending(x => x.Hieu_Luc_Tu);
            }

            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);

            return (total, data);
        }

        public async Task<bool> DeleteCongThucPhoiAsync(int id, CancellationToken ct = default)
        {
            using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // 1. Get the Cong_Thuc_Phoi entity
                var congThucPhoi = await _set.FirstOrDefaultAsync(x => x.ID == id && !x.Da_Xoa, ct);
                if (congThucPhoi == null)
                    return false;

                // 2. Check if output ore is being used in other formulas as input ore
                var outputQuangId = congThucPhoi.ID_Quang_DauRa;
                if (outputQuangId > 0)
                {
                    var isUsedInOtherFormulas = await _db.Set<CTP_ChiTiet_Quang>()
                        .AnyAsync(x => x.ID_Quang_DauVao == outputQuangId && 
                                      x.ID_Cong_Thuc_Phoi != id && 
                                      !x.Da_Xoa, ct);
                    
                    if (isUsedInOtherFormulas)
                    {
                        // Rollback transaction and throw exception with specific message
                        await transaction.RollbackAsync(ct);
                        throw new InvalidOperationException($"Không thể xóa công thức phối. Quặng đầu ra (ID: {outputQuangId}) đang được sử dụng trong công thức phối khác.");
                    }
                }

                // 3. Delete CTP_ChiTiet_Quang (Formula Detail Ores)
                var chiTietQuang = await _db.Set<CTP_ChiTiet_Quang>()
                    .Where(x => x.ID_Cong_Thuc_Phoi == id && !x.Da_Xoa)
                    .ToListAsync(ct);
                if (chiTietQuang.Any())
                {
                    _db.Set<CTP_ChiTiet_Quang>().RemoveRange(chiTietQuang);
                }

                // 4. Delete CTP_ChiTiet_Quang_TPHH (Formula Detail Ore Chemical Components)
                var chiTietQuangTphh = await _db.Set<CTP_ChiTiet_Quang_TPHH>()
                    .Where(x => chiTietQuang.Select(c => c.ID).Contains(x.ID_CTP_ChiTiet_Quang) && !x.Da_Xoa)
                    .ToListAsync(ct);
                if (chiTietQuangTphh.Any())
                {
                    _db.Set<CTP_ChiTiet_Quang_TPHH>().RemoveRange(chiTietQuangTphh);
                }

                // 5. Delete CTP_RangBuoc_TPHH (Formula Chemical Component Constraints)
                var rangBuocTphh = await _db.Set<CTP_RangBuoc_TPHH>()
                    .Where(x => x.ID_Cong_Thuc_Phoi == id && !x.Da_Xoa)
                    .ToListAsync(ct);
                if (rangBuocTphh.Any())
                {
                    _db.Set<CTP_RangBuoc_TPHH>().RemoveRange(rangBuocTphh);
                }

                // 6. Delete PA_LuaChon_CongThuc (Plan Formula Selections)
                var luaChonCongThuc = await _db.Set<PA_LuaChon_CongThuc>()
                    .Where(x => x.ID_Cong_Thuc_Phoi == id && !x.Da_Xoa)
                    .ToListAsync(ct);
                if (luaChonCongThuc.Any())
                {
                    _db.Set<PA_LuaChon_CongThuc>().RemoveRange(luaChonCongThuc);
                }

                // 7. Delete Quang_TP_PhanTich (Ore Chemical Analysis) for output ore only
                if (outputQuangId > 0)
                {
                    var quangTpPhanTich = await _db.Set<Quang_TP_PhanTich>()
                        .Where(x => x.ID_Quang == outputQuangId && !x.Da_Xoa)
                        .ToListAsync(ct);
                    if (quangTpPhanTich.Any())
                    {
                        _db.Set<Quang_TP_PhanTich>().RemoveRange(quangTpPhanTich);
                    }
                }

                // 8. Delete Quang_Gia_LichSu (Ore Price History) for output ore only
                if (outputQuangId > 0)
                {
                    var quangGiaLichSu = await _db.Set<Quang_Gia_LichSu>()
                        .Where(x => x.ID_Quang == outputQuangId && !x.Da_Xoa)
                        .ToListAsync(ct);
                    if (quangGiaLichSu.Any())
                    {
                        _db.Set<Quang_Gia_LichSu>().RemoveRange(quangGiaLichSu);
                    }
                }

                // 9. Delete Quang (Output Ore) only
                if (outputQuangId > 0)
                {
                    var outputQuang = await _db.Set<Quang>()
                        .FirstOrDefaultAsync(x => x.ID == outputQuangId && !x.Da_Xoa, ct);
                    if (outputQuang != null)
                    {
                        _db.Set<Quang>().Remove(outputQuang);
                    }
                }

                // 10. Finally delete the Cong_Thuc_Phoi itself
                _db.Set<Cong_Thuc_Phoi>().Remove(congThucPhoi);

                // Save all changes
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}
