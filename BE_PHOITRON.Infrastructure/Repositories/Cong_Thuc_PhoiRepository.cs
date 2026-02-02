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
        private readonly IQuangRepository _quangRepo;

        public Cong_Thuc_PhoiRepository(AppDbContext db, IQuangRepository quangRepo) : base(db)
        {
            _quangRepo = quangRepo;
        }

        public async Task<bool> ExistsByCodeAsync(string maCongThuc, CancellationToken ct = default)
            => await _set.AnyAsync(x => x.Ma_Cong_Thuc == maCongThuc && !x.Da_Xoa, ct);

        public async Task<Cong_Thuc_Phoi?> GetByQuangDauRaAsync(int idQuangDauRa, CancellationToken ct = default)
        {
            // Ưu tiên lấy công thức active (Trang_Thai = 1), nếu không có thì lấy mới nhất
            var activeFormula = await _set.AsNoTracking()
                .Where(x => x.ID_Quang_DauRa == idQuangDauRa && !x.Da_Xoa && x.Trang_Thai == 1)
                .OrderByDescending(x => x.Hieu_Luc_Tu)
                .FirstOrDefaultAsync(ct);
            
            if (activeFormula != null)
                return activeFormula;
            
            // Nếu không có công thức active, lấy công thức mới nhất
            return await _set.AsNoTracking()
                .Where(x => x.ID_Quang_DauRa == idQuangDauRa && !x.Da_Xoa)
                .OrderByDescending(x => x.Hieu_Luc_Tu)
                .FirstOrDefaultAsync(ct);
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

        /// <summary>
        /// Xóa công thức phối (delegate đến DeleteCongThucPhoiWithRelatedDataAsync để đảm bảo tính nhất quán)
        /// </summary>
        public async Task<bool> DeleteCongThucPhoiAsync(int id, CancellationToken ct = default)
        {
            return await DeleteCongThucPhoiWithRelatedDataAsync(id, ct);
        }

        public async Task<bool> DeleteCongThucPhoiWithRelatedDataAsync(int id, CancellationToken ct = default)
        {
            // Kiểm tra xem đã có transaction chưa
            var hasExistingTransaction = _db.Database.CurrentTransaction != null;
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
            
            if (!hasExistingTransaction)
            {
                transaction = await _db.Database.BeginTransactionAsync(ct);
            }
            
            try
            {
                var congThucPhoi = await _set.FirstOrDefaultAsync(x => x.ID == id, ct);
                if (congThucPhoi == null) return false;

                var outputQuangId = congThucPhoi.ID_Quang_DauRa;

                // 0. Kiểm tra quặng đầu ra có đang được dùng ở công thức phối khác không
                // Check trong bảng CTP_ChiTiet_Quang xem ID_Quang_DauRa có nằm trong list ID_Quang_DauVao của công thức phối nào không
                if (outputQuangId > 0)
                {
                    // Lấy tất cả các công thức phối có sử dụng quặng đầu ra này làm quặng đầu vào
                    var usedInOtherFormulas = await _db.Set<CTP_ChiTiet_Quang>()
                        .Where(ctq => ctq.ID_Quang_DauVao == outputQuangId  // Quặng đầu ra được dùng như đầu vào
                                   && ctq.ID_Cong_Thuc_Phoi != id            // Loại trừ công thức đang xóa
                                   && !ctq.Da_Xoa)                            // Chỉ lấy record chưa xóa
                        .Join(_db.Set<Cong_Thuc_Phoi>()
                                .Where(ctp => !ctp.Da_Xoa),                   // Chỉ lấy công thức chưa xóa
                            ctq => ctq.ID_Cong_Thuc_Phoi,
                            ctp => ctp.ID,
                            (ctq, ctp) => new { FormulaId = ctp.ID, FormulaCode = ctp.Ma_Cong_Thuc })
                        .Distinct()
                        .ToListAsync(ct);
                    
                    if (usedInOtherFormulas.Any())
                    {
                        var formulaCodes = string.Join(", ", usedInOtherFormulas.Select(x => x.FormulaCode ?? $"ID:{x.FormulaId}"));
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync(ct);
                        }
                        throw new InvalidOperationException($"Không thể xóa công thức phối. Quặng đầu ra (ID: {outputQuangId}) đang được sử dụng trong công thức phối khác: {formulaCodes}");
                    }
                }

                // 1. Xóa CTP_ChiTiet_Quang_TPHH
                var chiTietQuangIds = await _db.Set<CTP_ChiTiet_Quang>()
                    .Where(x => x.ID_Cong_Thuc_Phoi == id)
                    .Select(x => x.ID)
                    .ToListAsync(ct);
                
                if (chiTietQuangIds.Any())
                {
                    var chiTietQuangTphh = await _db.Set<CTP_ChiTiet_Quang_TPHH>()
                        .Where(x => chiTietQuangIds.Contains(x.ID_CTP_ChiTiet_Quang))
                        .ToListAsync(ct);
                    if (chiTietQuangTphh.Any())
                    {
                        _db.Set<CTP_ChiTiet_Quang_TPHH>().RemoveRange(chiTietQuangTphh);
                    }
                }

                // 2. Xóa CTP_ChiTiet_Quang
                var chiTietQuang = await _db.Set<CTP_ChiTiet_Quang>()
                    .Where(x => x.ID_Cong_Thuc_Phoi == id)
                    .ToListAsync(ct);
                if (chiTietQuang.Any())
                {
                    _db.Set<CTP_ChiTiet_Quang>().RemoveRange(chiTietQuang);
                    await _db.SaveChangesAsync(ct);
                }

                // 3. Xóa CTP_RangBuoc_TPHH
                var rangBuocTphh = await _db.Set<CTP_RangBuoc_TPHH>()
                    .Where(x => x.ID_Cong_Thuc_Phoi == id)
                    .ToListAsync(ct);
                if (rangBuocTphh.Any())
                {
                    _db.Set<CTP_RangBuoc_TPHH>().RemoveRange(rangBuocTphh);
                }

                // 4. Xóa CTP_BangChiPhi
                var bangChiPhi = await _db.Set<CTP_BangChiPhi>()
                    .Where(x => x.ID_CongThucPhoi == id)
                    .ToListAsync(ct);
                if (bangChiPhi.Any())
                {
                    _db.Set<CTP_BangChiPhi>().RemoveRange(bangChiPhi);
                }

                // 5. Xóa PA_LuaChon_CongThuc
                var luaChonCongThuc = await _db.Set<PA_LuaChon_CongThuc>()
                    .Where(x => x.ID_Cong_Thuc_Phoi == id)
                    .ToListAsync(ct);
                if (luaChonCongThuc.Any())
                {
                    _db.Set<PA_LuaChon_CongThuc>().RemoveRange(luaChonCongThuc);
                }

                // 6. Xóa Cong_Thuc_Phoi (dùng entity đã load ban đầu để tránh tracking conflict)
                _set.Remove(congThucPhoi);

                // 7. Xóa Quang với ID = ID_Quang_DauRa (xóa trực tiếp để tránh vòng lặp)
                // Không check công thức phối vì đây là quặng đầu ra của công thức đang xóa
                if (outputQuangId > 0)
                {
                    var outputQuang = await _db.Set<Quang>().FirstOrDefaultAsync(x => x.ID == outputQuangId, ct);
                    if (outputQuang != null)
                    {
                        // Xóa Quang_TP_PhanTich
                        var quangTPPhanTich = await _db.Set<Quang_TP_PhanTich>()
                            .Where(x => x.ID_Quang == outputQuangId)
                            .ToListAsync(ct);
                        if (quangTPPhanTich.Any())
                        {
                            _db.Set<Quang_TP_PhanTich>().RemoveRange(quangTPPhanTich);
                        }

                        // Xóa Quang_Gia_LichSu
                        var quangGiaLichSu = await _db.Set<Quang_Gia_LichSu>()
                            .Where(x => x.ID_Quang == outputQuangId)
                            .ToListAsync(ct);
                        if (quangGiaLichSu.Any())
                        {
                            _db.Set<Quang_Gia_LichSu>().RemoveRange(quangGiaLichSu);
                        }

                        // Xóa Quang
                        _db.Set<Quang>().Remove(outputQuang);
                    }
                }

                await _db.SaveChangesAsync(ct);
                
                // Chỉ commit nếu đây là transaction do hàm này tạo
                if (transaction != null)
                {
                    await transaction.CommitAsync(ct);
                }

                return true;
            }
            catch
            {
                // Chỉ rollback nếu đây là transaction do hàm này tạo
                if (transaction != null)
                {
                    await transaction.RollbackAsync(ct);
                }
                throw;
            }
            finally
            {
                // Dispose transaction nếu đây là transaction do hàm này tạo
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }
    }
}
