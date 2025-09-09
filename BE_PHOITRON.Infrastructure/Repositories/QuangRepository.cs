using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.Enums;
using BE_PHOITRON.Application.ResponsesModel;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.DataEntities;
using BE_PHOITRON.Infrastructure.Persistence;
using BE_PHOITRON.Infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    internal class QuangRepository : BaseRepository<Quang>, IQuangRepository
    {
        public QuangRepository(AppDbContext db) : base(db)
        {
        }

        public async Task<(int total, IReadOnlyList<Quang> data)> SearchPagedAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<Quang> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => x.MaQuang.Contains(search) || (x.TenQuang ?? "").Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && CheckValidPropertyPath.IsValidPropertyPath<Quang>(sortBy))
            {
                var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                q = q.OrderBy(cfg, $"{sortBy} {dir}");              // nhận camelCase/PascalCase/nested
            }
            else
            {
                q = q.OrderByDescending(x => x.NgayTao);     // fallback an toàn
            }

            var data = await q.Skip(page  * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);

            return (total, data);
        }

        public Task<bool> ExistsByCodeAsync(string maQuang, CancellationToken ct = default)
            => _set.AnyAsync(x => x.MaQuang == maQuang, ct);

        public Task<Quang?> GetWithGiaHienHanhAsync(int id, DateTime? at, CancellationToken ct = default)
        {
            //var time = at ?? DateTime.UtcNow;
            //return _set
            //    .Include(x => x.GiaQuangs.Where(g => g.HieuLucTu <= time && (g.HieuLucDen == null || g.HieuLucDen >= time)))
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(x => x.ID == id, ct);
            throw new NotImplementedException();
        }

        public async Task<int> UpdateTPHH(Quang_TPHHUpdateDto dto, CancellationToken ct = default)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            var items = (dto.Items ?? new List<Quang_TPHHItem>())
                        .Where(i => i.ID_TPHH > 0)
                        .GroupBy(i => i.ID_TPHH)           // phòng trùng ID_TPHH trong input
                        .Select(g => g.Last())              // lấy bản cuối cùng
                        .ToList();

            // (Tuỳ chọn) Validate tổng ~ 100%
            var total = items.Sum(x => x.PhanTram);
            if (total < 0) throw new InvalidOperationException("Tổng PhanTram < 0");
            // ví dụ: yêu cầu ~100% với sai số 0.01
            // if (Math.Abs(total - 100m) > 0.01m) throw new InvalidOperationException("Tổng PhanTram phải bằng 100%");

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var current = await _db.Set<Quang_TPHH>()
                .Where(x => x.ID_Quang == dto.ID_Quang)
                .ToListAsync(ct);

            var currentMap = current.ToDictionary(x => x.ID_TPHH);

            // Upsert
            foreach (var it in items)
            {
                if (currentMap.TryGetValue(it.ID_TPHH, out var exist))
                {
                    if (exist.PhanTram != it.PhanTram)
                        exist.PhanTram = it.PhanTram; // UPDATE
                }
                else
                {
                    _db.Add(new Quang_TPHH    // INSERT
                    {
                        ID_Quang = dto.ID_Quang,
                        ID_TPHH = it.ID_TPHH,
                        PhanTram = it.PhanTram
                    });
                }
            }

            // Delete những TPHH không còn
            var keepIds = items.Select(i => i.ID_TPHH).ToHashSet();
            var toRemove = current.Where(x => !keepIds.Contains(x.ID_TPHH)).ToList();
            if (toRemove.Count > 0) _db.RemoveRange(toRemove);

            var affected = await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return affected;
        }

        public async Task<QuangDetailResponse> GetDetailQuang(int id, CancellationToken ct = default)
        {
            var detail = await _db.Set<Quang>().AsNoTracking()
            .Where(q => q.ID == id)
            .Select(q => new QuangDetailResponse(
                new QuangResponse(
                    q.ID, q.MaQuang, q.TenQuang, q.Gia, q.GhiChu,
                    q.NgayTao, q.ID_NguoiTao, q.NgaySua, q.ID_NguoiSua, q.IsDeleted, q.MatKhiNung, q.LoaiQuang, q.ID_CongThucPhoi
                ),
                (from qt in _db.Set<Quang_TPHH>().AsNoTracking()
                 join tp in _db.Set<TP_HoaHoc>().AsNoTracking() on qt.ID_TPHH equals tp.ID
                 where qt.ID_Quang == q.ID
                 select new TPHHOfQuangReponse(tp.ID, tp.Ma_TPHH, tp.Ten_TPHH, qt.PhanTram)).ToList()
            ))
            .FirstOrDefaultAsync(ct);

            return detail;
        }

        public async Task<int> UpsertAsync(UpsertQuangMuaDto dto, CancellationToken ct = default)
        {
            // --- Validate cơ bản ---
            if (string.IsNullOrWhiteSpace(dto.Quang.MaQuang))
                throw new ArgumentException("MaQuang is required.");
            if (string.IsNullOrWhiteSpace(dto.Quang.TenQuang))
                throw new ArgumentException("TenQuang is required.");
            if (dto.ThanhPhan is null || dto.ThanhPhan.Count == 0)
                throw new InvalidOperationException("Cần ít nhất 1 thành phần hoá học.");

            // Không cho trùng ChemicalId
            var dupChem = dto.ThanhPhan.GroupBy(x => x.ID_TPHH)
                                       .FirstOrDefault(g => g.Count() > 1);
            if (dupChem != null) throw new InvalidOperationException("Trùng thành phần hoá học trong payload.");

            // Tuỳ chọn: ràng buộc mỗi % trong [0..100]
            if (dto.ThanhPhan.Any(x => x.PhanTram < 0 || x.PhanTram > 100))
                throw new InvalidOperationException("Phần trăm phải nằm trong khoảng 0..100.");

            // Kiểm tra Chemical tồn tại
            var chemIds = dto.ThanhPhan.Select(x => x.ID_TPHH).ToList();
            var existChem = await _db.Set<TP_HoaHoc>().CountAsync(x => chemIds.Contains(x.ID), ct);
            if (existChem != chemIds.Count)
                throw new InvalidOperationException("Có thành phần hoá học không tồn tại.");

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            if (dto.ID is null)
            {
                // CREATE
                var existsCode = await _db.Quang.AnyAsync(
                    x => !x.IsDeleted && x.MaQuang == dto.Quang.MaQuang, ct);
                if (existsCode)
                    throw new InvalidOperationException("Mã quặng đã tồn tại.");

                var header = new Quang
                {
                    MaQuang = dto.Quang.MaQuang.Trim(),
                    TenQuang = dto.Quang.TenQuang.Trim(),
                    Gia = dto.Quang.Gia,
                    GhiChu = dto.Quang.GhiChu,
                    NgayTao = DateTime.UtcNow,
                    IsDeleted = false,
                    LoaiQuang = (int)LoaiQuangEnum.Mua,
                    MatKhiNung = dto.Quang.MatKhiNung
                };

                _db.Quang.Add(header);
                await _db.SaveChangesAsync(ct); // có header.Id

                var childs = dto.ThanhPhan.Select(tp => new Quang_TPHH
                {
                    ID_Quang = header.ID,
                    ID_TPHH = tp.ID_TPHH,
                    PhanTram = Math.Round(tp.PhanTram, 2)
                });
                _db.Set<Quang_TPHH>().AddRange(childs);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return header.ID;
            }
            else
            {
                // UPDATE
                var header = await _db.Quang
                    .FirstOrDefaultAsync(x => x.ID == dto.ID && !x.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Quặng không tồn tại.");

                var existsOther = await _db.Quang.AnyAsync(
                    x => !x.IsDeleted && x.MaQuang == dto.Quang.MaQuang && x.ID != header.ID, ct);
                if (existsOther)
                    throw new InvalidOperationException("Mã quặng đã tồn tại ở bản ghi khác.");

                header.MaQuang = dto.Quang.MaQuang.Trim();
                header.TenQuang = dto.Quang.TenQuang.Trim();
                header.Gia = dto.Quang.Gia;
                header.GhiChu = dto.Quang.GhiChu;
                header.NgaySua = DateTime.UtcNow;
                header.MatKhiNung = dto.Quang.MatKhiNung;

                await _db.SaveChangesAsync(ct);

                // Thay thế thành phần: xoá cũ -> chèn mới
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM Quang_TPHH WHERE ID_Quang = {header.ID}", ct);

                var newRows = dto.ThanhPhan.Select(tp => new Quang_TPHH
                {
                    ID_Quang = header.ID,
                    ID_TPHH = tp.ID_TPHH,
                    PhanTram = Math.Round(tp.PhanTram, 2)
                });
                _db.Set<Quang_TPHH>().AddRange(newRows);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return header.ID;
            }
        }

        public async Task<List<QuangDetailResponse>> getOreChemistryBatch(List<int> id_Quangs, CancellationToken ct = default)
        {
            if (id_Quangs is null || id_Quangs.Count == 0)
                return new List<QuangDetailResponse>();

            // Map thứ tự để trả về đúng thứ tự client gửi lên
            var order = id_Quangs.Select((id, idx) => new { id, idx })
                              .ToDictionary(x => x.id, x => x.idx);

            // 1) Lấy thông tin quặng
            var ores = await _db.Set<Quang>()
                .AsNoTracking()
                .Where(o => id_Quangs.Contains(o.ID) && !o.IsDeleted)
                .Select(o => new QuangResponse(
                    o.ID,
                    o.MaQuang,
                    o.TenQuang,
                    o.Gia,
                    o.GhiChu,
                    o.NgayTao,
                    o.ID_NguoiTao,
                    o.NgaySua,
                    o.ID_NguoiSua,
                    o.IsDeleted,
                    o.MatKhiNung,
                    o.LoaiQuang,
                    o.ID_CongThucPhoi
                ))
                .ToListAsync(ct);

            if (ores.Count == 0) return new List<QuangDetailResponse>();

            var oreIdsFound = ores.Select(o => o.ID).ToList();

            // 2) Lấy các cell Quang_TPHH + join tên/mã TPHH
            var cells = await _db.Set<Quang_TPHH>()
                .AsNoTracking()
                .Where(x => oreIdsFound.Contains(x.ID_Quang))
                .Join(_db.Set<TP_HoaHoc>().AsNoTracking(),
                      qt => qt.ID_TPHH,
                      hh => hh.ID,
                      (qt, hh) => new
                      {
                          qt.ID_Quang,
                          Chem = new TPHHOfQuangReponse(
                              hh.ID,
                              hh.Ma_TPHH,
                              hh.Ten_TPHH,
                              qt.PhanTram        // đổi sang decimal? nếu cột có thể NULL
                          )
                      })
                .ToListAsync(ct);

            // 3) Gom theo quặng
            var chemByOre = cells
                .GroupBy(x => x.ID_Quang)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Chem).ToList());

            // 4) Build kết quả, giữ thứ tự oreIds
            var result = ores
                .Select(o => new QuangDetailResponse(
                    o,
                    chemByOre.TryGetValue(o.ID, out var list) ? list : new List<TPHHOfQuangReponse>()
                ))
                .OrderBy(r => order.TryGetValue(r.Quang.ID, out var idx) ? idx : int.MaxValue)
                .ToList();

            return result;
        }

        public async Task<IReadOnlyList<QuangItemResponse>> GetByListIdsAsync(List<int> IDs, CancellationToken ct = default)
        {
            if (IDs is null || IDs.Count == 0)
                return new List<QuangItemResponse>();
            var result = _db.Quang
                .Where(x => IDs.Contains(x.ID) && !x.IsDeleted)
                .Select(o => new QuangItemResponse(
                    o.ID,
                    o.MaQuang,
                    o.TenQuang,
                    o.Gia,
                    o.LoaiQuang,
                    o.IsDeleted
                ))
                .ToList();

            return result;
        }
    }
}
