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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class CongThucPhoiRepository : BaseRepository<CongThucPhoi>, ICongThucPhoiRepository
    {
        public CongThucPhoiRepository(AppDbContext db) : base(db)
        {
        }

        public async Task<(int total, IReadOnlyList<CongThucPhoi> data)> SearchPagedAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<CongThucPhoi> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => x.MaCongThuc.Contains(search) || (x.TenCongThuc ?? "").Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && CheckValidPropertyPath.IsValidPropertyPath<CongThucPhoi>(sortBy))
            {
                var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                q = q.OrderBy(cfg, $"{sortBy} {dir}");              // nhận camelCase/PascalCase/nested
            }
            else
            {
                q = q.OrderByDescending(x => x.NgayTao);    // fallback an toàn
            }

            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);

            return (total, data);
        }
        public Task<bool> ExistsByCodeAsync(string MaCongThuc, CancellationToken ct = default)
            => _set.AnyAsync(x => x.MaCongThuc == MaCongThuc, ct);
        public async Task<int> UpdateCongThucPTDto(UpdateCongThucPTDto dto, CancellationToken ct = default)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            // Phân biệt null vs empty:
            var shouldTouchTPHH = dto.ListTPHH != null;
            var shouldTouchQuang = dto.ListQuang != null;

            // Dedup theo ID, ưu tiên phần tử cuối (như bạn đã làm)
            List<CongThuc_TPHHItem> newTPHH = shouldTouchTPHH
                ? dto.ListTPHH!
                    .Where(i => i.ID_TPHH > 0)
                    .GroupBy(i => i.ID_TPHH)
                    .Select(g => g.Last())
                    .ToList()
                : new List<CongThuc_TPHHItem>();

            List<CongThuc_QuangItem> newQuang = shouldTouchQuang
                ? dto.ListQuang!
                    .Where(i => i.ID_Quang > 0)
                    .GroupBy(i => i.ID_Quang)
                    .Select(g => g.Last())
                    .ToList()
                : new List<CongThuc_QuangItem>();

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            // ---------- Sync TPHH ----------
            if (shouldTouchTPHH)
            {
                var setTPHH = _db.Set<CongThucPhoi_TPHH>();
                // Lấy hiện trạng (tracked để cập nhật)
                var currentTPHH = await setTPHH
                    .Where(x => x.ID_CongThucPhoi == dto.ID)
                    .ToListAsync(ct);

                var currentMap = currentTPHH.ToDictionary(x => x.ID_TPHH);
                var incomingIds = new HashSet<int>(newTPHH.Select(x => x.ID_TPHH));

                // XÓA: cái nào đang có mà không còn trong DTO
                var toDelete = currentTPHH.Where(x => !incomingIds.Contains(x.ID_TPHH)).ToList();
                if (toDelete.Count > 0) setTPHH.RemoveRange(toDelete);

                // THÊM/CẬP NHẬT
                foreach (var item in newTPHH)
                {
                    if (currentMap.TryGetValue(item.ID_TPHH, out var entity))
                    {
                        // Cập nhật nếu khác
                        if (entity.Min_PhanTram != item.Min_PhanTram ||
                            entity.Max_PhanTram != item.Max_PhanTram)
                        {
                            entity.Min_PhanTram = item.Min_PhanTram;
                            entity.Max_PhanTram = item.Max_PhanTram;
                        }
                    }
                    else
                    {
                        // Thêm mới
                        setTPHH.Add(new CongThucPhoi_TPHH
                        {
                            ID_CongThucPhoi = dto.ID,
                            ID_TPHH = item.ID_TPHH,
                            Min_PhanTram = item.Min_PhanTram,
                            Max_PhanTram = item.Max_PhanTram
                        });
                    }
                }
            }

            // ---------- Sync QUẶNG ----------
            if (shouldTouchQuang)
            {
                var setQuang = _db.Set<CongThucPhoi_Quang>();
                var currentQuang = await setQuang
                    .Where(x => x.ID_CongThucPhoi == dto.ID)
                    .ToListAsync(ct);

                var currentMap = currentQuang.ToDictionary(x => x.ID_Quang);
                var incomingIds = new HashSet<int>(newQuang.Select(x => x.ID_Quang));

                // XÓA
                var toDelete = currentQuang.Where(x => !incomingIds.Contains(x.ID_Quang)).ToList();
                if (toDelete.Count > 0) setQuang.RemoveRange(toDelete);

                // THÊM/CẬP NHẬT
                foreach (var item in newQuang)
                {
                    if (currentMap.TryGetValue(item.ID_Quang, out var entity))
                    {
                        if (entity.TiLePhoi != item.TiLePhoi)
                        {
                            entity.TiLePhoi = item.TiLePhoi;
                        }
                    }
                    else
                    {
                        setQuang.Add(new CongThucPhoi_Quang
                        {
                            ID_CongThucPhoi = dto.ID,
                            ID_Quang = item.ID_Quang,
                            TiLePhoi = item.TiLePhoi
                        });
                    }
                }
            }

            var affected = await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return affected;
        }

        public async Task<int> UpsertCongThucPTAsync(UpsertCongThucPTDto dto, CancellationToken ct = default)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var shouldTouchTPHH = dto.ListTPHH != null;   // null = không đụng tới
            var shouldTouchQuang = dto.ListQuang != null; // []  = xoá sạch

            // Dedup theo ID, lấy phần tử cuối
            var newTPHH = shouldTouchTPHH
                ? dto.ListTPHH!
                    .Where(i => i.ID_TPHH > 0)
                    .GroupBy(i => i.ID_TPHH).Select(g => g.Last()).ToList()
                : new List<CongThuc_TPHHItem>();

            var newQuang = shouldTouchQuang
                ? dto.ListQuang!
                    .Where(i => i.ID_Quang > 0)
                    .GroupBy(i => i.ID_Quang).Select(g => g.Last()).ToList()
                : new List<CongThuc_QuangItem>();

            var isCreate = dto.ID <= 0;

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            if (isCreate)
            {
                // ============== CREATE (2 SaveChanges trong 1 transaction) ==============
                var parent = new CongThucPhoi
                {
                    MaCongThuc = dto.MaCongThuc ?? "",
                    TenCongThuc = dto.TenCongThuc ?? "",
                    TongPhanTram = dto.TongPhanTram,
                    GhiChu = dto.GhiChu,
                    NgayTao = DateTime.UtcNow,
                    ID_NguoiTao = dto.ID_NguoiTao
                };

                _db.Add(parent);
                await _db.SaveChangesAsync(ct);                 // LẦN 1: lấy parent.ID (IDENTITY)

                // Chèn bảng phụ với ID_CongThucPhoi = parent.ID
                if (shouldTouchTPHH && newTPHH.Count > 0)
                {
                    var rows = newTPHH.Select(t => new CongThucPhoi_TPHH
                    {
                        ID_CongThucPhoi = parent.ID,
                        ID_TPHH = t.ID_TPHH,
                        Min_PhanTram = t.Min_PhanTram,
                        Max_PhanTram = t.Max_PhanTram
                    });
                    _db.AddRange(rows);
                }

                if (shouldTouchQuang && newQuang.Count > 0)
                {
                    var rows = newQuang.Select(q => new CongThucPhoi_Quang
                    {
                        ID_CongThucPhoi = parent.ID,
                        ID_Quang = q.ID_Quang,
                        TiLePhoi = q.TiLePhoi
                    });
                    _db.AddRange(rows);
                }

                await _db.SaveChangesAsync(ct);                // LẦN 2: chèn bảng phụ
                await tx.CommitAsync(ct);
                return parent.ID;
            }
            else
            {
                // ============== UPDATE (1 SaveChanges) ==============
                var parent = await _db.Set<CongThucPhoi>()
                                      .FirstOrDefaultAsync(x => x.ID == dto.ID, ct)
                             ?? throw new KeyNotFoundException($"Không tìm thấy công thức ID={dto.ID}");

                // Cập nhật các cột cho phép
                if (dto.MaCongThuc is not null) parent.MaCongThuc = dto.MaCongThuc;
                if (dto.TenCongThuc is not null) parent.TenCongThuc = dto.TenCongThuc;
                if (dto.TongPhanTram.HasValue) parent.TongPhanTram = dto.TongPhanTram;
                if (dto.GhiChu is not null) parent.GhiChu = dto.GhiChu;
                parent.NgaySua = DateTime.UtcNow;
                parent.ID_NguoiSua = dto.ID_NguoiSua;

                // ---------- Sync TPHH ----------
                if (shouldTouchTPHH)
                {
                    var set = _db.Set<CongThucPhoi_TPHH>();
                    var cur = await set.Where(x => x.ID_CongThucPhoi == dto.ID).ToListAsync(ct);

                    var curMap = cur.ToDictionary(x => x.ID_TPHH);
                    var incomingIds = new HashSet<int>(newTPHH.Select(x => x.ID_TPHH));

                    // XÓA cái không còn trong payload (nếu payload rỗng => xoá sạch)
                    var del = cur.Where(x => !incomingIds.Contains(x.ID_TPHH)).ToList();
                    if (del.Count > 0) set.RemoveRange(del);

                    // THÊM / CẬP NHẬT
                    foreach (var item in newTPHH)
                    {
                        if (curMap.TryGetValue(item.ID_TPHH, out var e))
                        {
                            if (e.Min_PhanTram != item.Min_PhanTram || e.Max_PhanTram != item.Max_PhanTram)
                            {
                                e.Min_PhanTram = item.Min_PhanTram;
                                e.Max_PhanTram = item.Max_PhanTram;
                            }
                        }
                        else
                        {
                            set.Add(new CongThucPhoi_TPHH
                            {
                                ID_CongThucPhoi = dto.ID,
                                ID_TPHH = item.ID_TPHH,
                                Min_PhanTram = item.Min_PhanTram,
                                Max_PhanTram = item.Max_PhanTram
                            });
                        }
                    }
                }

                // ---------- Sync QUẶNG ----------
                if (shouldTouchQuang)
                {
                    var set = _db.Set<CongThucPhoi_Quang>();
                    var cur = await set.Where(x => x.ID_CongThucPhoi == dto.ID).ToListAsync(ct);

                    var curMap = cur.ToDictionary(x => x.ID_Quang);
                    var incomingIds = new HashSet<int>(newQuang.Select(x => x.ID_Quang));

                    var del = cur.Where(x => !incomingIds.Contains(x.ID_Quang)).ToList();
                    if (del.Count > 0) set.RemoveRange(del);

                    foreach (var item in newQuang)
                    {
                        if (curMap.TryGetValue(item.ID_Quang, out var e))
                        {
                            if (e.TiLePhoi != item.TiLePhoi)
                                e.TiLePhoi = item.TiLePhoi;
                        }
                        else
                        {
                            set.Add(new CongThucPhoi_Quang
                            {
                                ID_CongThucPhoi = dto.ID,
                                ID_Quang = item.ID_Quang,
                                TiLePhoi = item.TiLePhoi
                            });
                        }
                    }
                }

                await _db.SaveChangesAsync(ct);   // 1 lần
                await tx.CommitAsync(ct);
                return parent.ID;
            }
        }

        

        public async Task<CongThucPhoiDetailRespone?> GetCongThucPhoiDetailAsync(int id, CancellationToken ct = default)
        {
            // 1) Thông tin công thức
            var congThuc = await _db.Set<CongThucPhoi>().AsNoTracking()
                .Where(x => x.ID == id)
                .Select(x => new CongThucPhoiResponse(
                    x.ID,
                    x.MaCongThuc,
                    x.TenCongThuc,
                    x.TongPhanTram,
                    x.GhiChu,
                    x.NgayTao,
                    x.ID_NguoiTao,
                    x.NgaySua,
                    x.ID_NguoiSua,
                    x.IsDeleted,
                    x.ID_QuangNeo
                ))
                .FirstOrDefaultAsync(ct);

            if (congThuc is null) return null;

            // 2) TPHH của công thức (kèm min/max)
            var tphhOfCongThuc = await (
                from m in _db.Set<CongThucPhoi_TPHH>().AsNoTracking()
                join tp in _db.Set<TP_HoaHoc>().AsNoTracking() on m.ID_TPHH equals tp.ID
                where m.ID_CongThucPhoi == id
                orderby tp.Ma_TPHH
                select new TPHHOfCongThucResponse(
                    tp.ID,
                    tp.Ma_TPHH,
                    tp.Ten_TPHH,
                    m.Min_PhanTram,
                    m.Max_PhanTram
                )
            ).ToListAsync(ct);

            // 3) Danh sách quặng của công thức
            var quangs = await (
                from m in _db.Set<CongThucPhoi_Quang>().AsNoTracking()
                join q in _db.Set<Quang>().AsNoTracking() on m.ID_Quang equals q.ID
                where m.ID_CongThucPhoi == id
                orderby q.MaQuang
                select new { q.ID, q.MaQuang, q.TenQuang }
            ).ToListAsync(ct);

            var quangIds = quangs.Select(x => x.ID).Distinct().ToList();

            // 4) TPHH theo từng quặng (1 query, rồi group)
            var tphhMapByQuang = new Dictionary<int, List<TPHHOfQuangReponse>>();
            if (quangIds.Count > 0)
            {
                var tphhFlat = await (
                    from rel in _db.Set<Quang_TPHH>().AsNoTracking()
                    join tp in _db.Set<TP_HoaHoc>().AsNoTracking() on rel.ID_TPHH equals tp.ID
                    where quangIds.Contains(rel.ID_Quang)
                    select new
                    {
                        rel.ID_Quang,
                        tp.ID,
                        tp.Ma_TPHH,
                        tp.Ten_TPHH,
                        rel.PhanTram
                    }
                ).ToListAsync(ct);

                tphhMapByQuang = tphhFlat
                    .GroupBy(x => x.ID_Quang)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(z => new TPHHOfQuangReponse(z.ID, z.Ma_TPHH, z.Ten_TPHH,z.PhanTram)).ToList()
                    );
            }

            // 5) Ráp danh sách quặng theo model mới
            var quangResponses = quangs
                .Select(x => new CongThucQuangResponse(
                    x.ID,
                    x.MaQuang,
                    x.TenQuang,
                    tphhMapByQuang.TryGetValue(x.ID, out var list) ? list : new List<TPHHOfQuangReponse>()
                ))
                .ToList();

            return new CongThucPhoiDetailRespone(congThuc, tphhOfCongThuc, quangResponses);
        }

        public async Task<UpsertAndConfirmResult> UpsertAndConfirmAsync(
        UpsertAndConfirmDto dto, CancellationToken ct = default)
        {
            // ---- Validate cơ bản
            if (string.IsNullOrWhiteSpace(dto.CongThucPhoi.MaCongThuc)) throw new ArgumentException("MaCongThuc required");
            if (string.IsNullOrWhiteSpace(dto.CongThucPhoi.TenCongThuc)) throw new ArgumentException("TenCongThuc required");
            if (dto.CongThucPhoi.QuangInputs is null || dto.CongThucPhoi.QuangInputs.Count == 0) throw new InvalidOperationException("Cần ít nhất 1 quặng input.");
            if (dto.CongThucPhoi.QuangInputs.GroupBy(x => x.ID_Quang).Any(g => g.Count() > 1)) throw new InvalidOperationException("Trùng quặng input.");
            if (string.IsNullOrWhiteSpace(dto.Quang.MaQuang)) throw new ArgumentException("MaQuang required");
            if (string.IsNullOrWhiteSpace(dto.Quang.TenQuang)) throw new ArgumentException("TenQuang required");
            if (dto.KetQuaTPHHtItems is null || dto.KetQuaTPHHtItems.Count == 0) throw new InvalidOperationException("Thiếu thành phần TPHH đầu ra.");

            // Pre-check: neo (nếu có)
            if (dto.CongThucPhoi.ID_QuangNeo is int neoId)
            {
                var neoOk = await _db.Set<Quang>().AnyAsync(q => q.ID == neoId && !q.IsDeleted, ct);
                if (!neoOk) throw new InvalidOperationException("Quặng neo không tồn tại.");
            }

            // Pre-check: quặng input tồn tại
            var oreIds = dto.CongThucPhoi.QuangInputs.Select(x => x.ID_Quang).Distinct().ToList();
            var oreCount = await _db.Set<Quang>().CountAsync(q => oreIds.Contains(q.ID) && !q.IsDeleted, ct);
            if (oreCount != oreIds.Count) throw new InvalidOperationException("Có quặng input không tồn tại/đã xoá.");

            // Pre-check: TPHH (nếu có min/max hoặc final components)
            var chemIds = (dto.CongThucPhoi.RangBuocTPHHs?.Select(x => x.ID_TPHH) ?? Enumerable.Empty<int>())
                          .Concat(dto.KetQuaTPHHtItems.Select(x => x.ID_TPHH))
                          .Distinct().ToList();
            if (chemIds.Count > 0)
            {
                var chemOk = await _db.Set<TP_HoaHoc>().CountAsync(h => chemIds.Contains(h.ID), ct);
                if (chemOk != chemIds.Count) throw new InvalidOperationException("Có ID_TPHH không hợp lệ.");
            }

            // Unique: mã công thức (nếu tạo mới/đổi mã), mã quặng output
            if (dto.CongThucPhoi.ID is null)
            {
                var dupF = await _db.CongThucPhoi.AnyAsync(x => x.MaCongThuc == dto.CongThucPhoi.MaCongThuc, ct);
                if (dupF) throw new InvalidOperationException("Mã công thức đã tồn tại.");
            }
            else
            {
                var dupF = await _db.CongThucPhoi.AnyAsync(x => x.MaCongThuc == dto.CongThucPhoi.MaCongThuc && x.ID != dto.CongThucPhoi.ID, ct);
                if (dupF) throw new InvalidOperationException("Mã công thức đã tồn tại ở bản ghi khác.");
            }

            var dupOre = await _db.Quang.AnyAsync(q => !q.IsDeleted && q.MaQuang == dto.Quang.MaQuang, ct);
            if (dupOre) throw new InvalidOperationException("Mã quặng đầu ra đã tồn tại.");

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            // ---- Upsert công thức
            CongThucPhoi f;
            if (dto.CongThucPhoi.ID is null)
            {
                f = new CongThucPhoi
                {
                    MaCongThuc = dto.CongThucPhoi.MaCongThuc.Trim(),
                    TenCongThuc = dto.CongThucPhoi.TenCongThuc.Trim(),
                    GhiChu = dto.CongThucPhoi.GhiChu,
                    ID_QuangNeo = dto.CongThucPhoi.ID_QuangNeo,
                    NgayTao = DateTime.UtcNow,
                    TongPhanTram = dto.CongThucPhoi.TongPhanTram,
                };
                _db.CongThucPhoi.Add(f);
                await _db.SaveChangesAsync(ct); // f.ID
            }
            else
            {
                f = await _db.CongThucPhoi.FirstOrDefaultAsync(x => x.ID == dto.CongThucPhoi.ID, ct)
                    ?? throw new KeyNotFoundException("Công thức không tồn tại.");

                f.MaCongThuc = dto.CongThucPhoi.MaCongThuc.Trim();
                f.TenCongThuc = dto.CongThucPhoi.TenCongThuc.Trim();
                f.GhiChu = dto.CongThucPhoi.GhiChu;
                f.ID_QuangNeo = dto.CongThucPhoi.ID_QuangNeo;
                await _db.SaveChangesAsync(ct);

                // clear inputs/constraints cũ
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM CongThucPhoi_Quang WHERE ID_CongThucPhoi = {f.ID}", ct);
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM CongThucPhoi_TPHH WHERE ID_CongThucPhoi = {f.ID}", ct);
            }

            // insert inputs
            _db.Set<CongThucPhoi_Quang>().AddRange(
                dto.CongThucPhoi.QuangInputs.Select(x => new CongThucPhoi_Quang
                {
                    ID_CongThucPhoi = f.ID,
                    ID_Quang = x.ID_Quang,
                    TiLePhoi = x.TiLePhoi
                })
            );

            // (tuỳ) insert ràng buộc min/max
            if (dto.CongThucPhoi.RangBuocTPHHs is { Count: > 0 })
            {
                _db.Set<CongThucPhoi_TPHH>().AddRange(
                    dto.CongThucPhoi.RangBuocTPHHs.Select(c => new CongThucPhoi_TPHH
                    {
                        ID_CongThucPhoi = f.ID,
                        ID_TPHH = c.ID_TPHH,
                        Min_PhanTram = c.Min_PhanTram,
                        Max_PhanTram = c.Max_PhanTram
                    })
                );
            }
            await _db.SaveChangesAsync(ct);

            // ---- Tạo quặng thành phẩm
            var ore = new Quang
            {
                MaQuang = dto.Quang.MaQuang.Trim(),
                TenQuang = dto.Quang.TenQuang.Trim(),
                Gia = dto.Quang.Gia,
                GhiChu = dto.Quang.GhiChu,
                LoaiQuang = (int)LoaiQuangEnum.Tron, // enum của bạn
                ID_CongThucPhoi = f.ID,
                NgayTao = DateTime.UtcNow,
                MatKhiNung = dto.Quang.MatKhiNung
            };
            _db.Quang.Add(ore);
            await _db.SaveChangesAsync(ct); // ore.ID

            // Thành phần hoá học do FE đã tính
            _db.Set<Quang_TPHH>().AddRange(
                dto.KetQuaTPHHtItems.Select(c => new Quang_TPHH
                {
                    ID_Quang = ore.ID,
                    ID_TPHH = c.ID_TPHH,
                    PhanTram = Math.Round(c.PhanTram, 2)
                })
            );
            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return new UpsertAndConfirmResult(f.ID, ore.ID);
        }

        public async Task<CongThucEditVm?> GetForEditAsync(int formulaId, CancellationToken ct = default)
        {
            var header = await _db.CongThucPhoi
                .Where(x => x.ID == formulaId)
                .Select(x => new { x.ID, x.ID_QuangNeo, x.MaCongThuc, x.TenCongThuc, x.GhiChu })
                .FirstOrDefaultAsync(ct);
            if (header is null) return null;

            var inputs = await _db.Set<CongThucPhoi_Quang>()
                .Where(i => i.ID_CongThucPhoi == formulaId)
                .Join(_db.Set<Quang>().Where(q => !q.IsDeleted),
                      i => i.ID_Quang, q => q.ID,
                      (i, q) => new FormulaInputVm(q.ID, q.MaQuang, q.TenQuang, q.Gia, i.TiLePhoi))
                .ToListAsync(ct);

            return new CongThucEditVm(header.ID, header.ID_QuangNeo, header.MaCongThuc, header.TenCongThuc, header.GhiChu, inputs);
        }

        

        public async Task<NeoDashboardVm?> GetByNeoAsync(int quangNeoId, CancellationToken ct = default)
        {
            // thông tin neo (nếu có)
            var neo = await _db.Set<Quang>()
                .Where(q => q.ID == quangNeoId && !q.IsDeleted)
                .Select(q => new { q.ID, q.MaQuang, q.TenQuang })
                .FirstOrDefaultAsync(ct);
            if (neo is null) return null;

            // các công thức gắn neo
            var formulas = await _db.CongThucPhoi
                .Where(f => f.ID_QuangNeo == quangNeoId)
                .Select(f => new { f.ID, f.MaCongThuc, f.TenCongThuc })
                .ToListAsync(ct);

            // thống kê quặng thành phẩm đã sản xuất theo từng công thức
            var produced = await _db.Quang
                .Where(q => !q.IsDeleted && q.ID_CongThucPhoi != null
                            && formulas.Select(f => f.ID).Contains(q.ID_CongThucPhoi.Value))
                .GroupBy(q => q.ID_CongThucPhoi!.Value)
                .Select(g => new { FormulaId = g.Key, Count = g.Count(), LastAt = g.Max(x => x.NgayTao) })
                .ToListAsync(ct);
            var producedMap = produced.ToDictionary(x => x.FormulaId, x => x);

            var result = new NeoDashboardVm(
                neo.ID, neo.MaQuang, neo.TenQuang,
                formulas.Select(f => new FormulaSummaryVm(
                    f.ID, f.MaCongThuc, f.TenCongThuc,
                    producedMap.TryGetValue(f.ID, out var s) ? s.Count : 0,
                    producedMap.TryGetValue(f.ID, out s) ? s.LastAt : (DateTimeOffset?)null
                )).ToList()
            );

            return result;
        }
    }
}
