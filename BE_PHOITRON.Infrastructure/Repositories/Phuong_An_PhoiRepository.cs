using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class Phuong_An_PhoiRepository : BaseRepository<Phuong_An_Phoi>, IPhuong_An_PhoiRepository
    {
        public Phuong_An_PhoiRepository(AppDbContext db) : base(db) { }

        private static string ToSlug(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var s = input.Trim().ToLowerInvariant();
            // Remove diacritics
            var normalized = s.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            s = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
            // Keep letters, numbers, dash; replace spaces with '-'
            var chars = s.Select(ch => char.IsLetterOrDigit(ch) ? ch : (char)(ch == ' ' ? '-' : '-')).ToArray();
            var slug = new string(chars);
            // Collapse multiple dashes
            while (slug.Contains("--")) slug = slug.Replace("--", "-");
            return slug.Trim('-');
        }

        private static string BuildCode(int maxLen, params string[] parts)
        {
            var joined = string.Join('-', parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(ToSlug));
            if (joined.Length <= maxLen) return joined;
            // Try to shrink by trimming middle parts first
            // Fallback: keep start and end, append checksum
            var checksum = Math.Abs(joined.GetHashCode()).ToString();
            var suffix = "-" + checksum.Substring(0, Math.Min(6, checksum.Length));
            var keep = Math.Max(0, maxLen - suffix.Length);
            joined = joined.Substring(0, keep).Trim('-') + suffix;
            // Ensure final length
            if (joined.Length > maxLen) joined = joined.Substring(0, maxLen).Trim('-');
            return joined;
        }

        public async Task<IReadOnlyList<Phuong_An_Phoi>> GetByQuangDichAsync(int idQuangDich, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.ID_Quang_Dich == idQuangDich && !x.Da_Xoa)
                .OrderByDescending(x => x.Ngay_Tinh_Toan)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Phuong_An_Phoi>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.Trang_Thai == 1) // Trang_Thai = 1 means active
                .OrderByDescending(x => x.Ngay_Tinh_Toan)
                .ToListAsync(ct);
        }

        public async Task<bool> ValidateNoCircularDependencyAsync(int idPhuongAn, CancellationToken ct = default)
        {
            // This will be implemented with recursive CTE to check for circular dependencies
            // For now, return true (no circular dependency)
            // TODO: Implement recursive CTE logic
            return await Task.FromResult(true);
        }

        protected override IQueryable<Phuong_An_Phoi> ApplySearchFilter(IQueryable<Phuong_An_Phoi> query, string search)
        {
            return query.Where(x => x.Ten_Phuong_An.Contains(search) ||
                                  (x.Ghi_Chu != null && x.Ghi_Chu.Contains(search)));
        }

        protected override IQueryable<Phuong_An_Phoi> ApplySorting(IQueryable<Phuong_An_Phoi> query, string sortBy, string? sortDir)
        {
            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            
            return sortBy.ToLower() switch
            {
                "ten_phuong_an" => isDesc ? query.OrderByDescending(x => x.Ten_Phuong_An) : query.OrderBy(x => x.Ten_Phuong_An),
                "ngay_tinh_toan" => isDesc ? query.OrderByDescending(x => x.Ngay_Tinh_Toan) : query.OrderBy(x => x.Ngay_Tinh_Toan),
                "trang_thai" => isDesc ? query.OrderByDescending(x => x.Trang_Thai) : query.OrderBy(x => x.Trang_Thai),
                "phien_ban" => isDesc ? query.OrderByDescending(x => x.Phien_Ban) : query.OrderBy(x => x.Phien_Ban),
                _ => query.OrderByDescending(x => x.Ngay_Tinh_Toan)
            };
        }

        public override async Task<(int total, IReadOnlyList<Phuong_An_Phoi> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<Phuong_An_Phoi> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => (x.Ten_Phuong_An ?? "").Contains(search) ||
                                 (x.Ghi_Chu ?? "").Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && Infrastructure.Shared.CheckValidPropertyPath.IsValidPropertyPath<Phuong_An_Phoi>(sortBy))
            {
                var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                q = q.OrderBy(cfg, $"{sortBy} {dir}");
            }
            else
            {
                q = q.OrderByDescending(x => x.Ngay_Tinh_Toan);
            }

            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);

            return (total, data);
        }

        public async Task<int> MixAsync(MixQuangRequestDto dto, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                Cong_Thuc_Phoi congThuc;
                Quang quang;

                var isUpdate = dto.CongThucPhoi.ID.HasValue && dto.CongThucPhoi.ID.Value > 0;

                if (isUpdate)
                {
                    // UPDATE FLOW
                    congThuc = await _db.Set<Cong_Thuc_Phoi>().FirstOrDefaultAsync(x => x.ID == dto.CongThucPhoi.ID!.Value && !x.Da_Xoa, ct)
                        ?? throw new InvalidOperationException($"Không tìm thấy công thức phối ID {dto.CongThucPhoi.ID}");

                    // Update công thức
                    congThuc.Ma_Cong_Thuc = dto.CongThucPhoi.Ma_Cong_Thuc ?? congThuc.Ma_Cong_Thuc;
                    congThuc.Ten_Cong_Thuc = dto.CongThucPhoi.Ten_Cong_Thuc ?? congThuc.Ten_Cong_Thuc;
                    congThuc.Ghi_Chu = dto.CongThucPhoi.Ghi_Chu ?? congThuc.Ghi_Chu;
                    if (dto.CongThucPhoi.Ngay_Tao.HasValue)
                        congThuc.Hieu_Luc_Tu = dto.CongThucPhoi.Ngay_Tao.Value;

                    // Update quặng đầu ra (chỉ cập nhật thông tin cơ bản, KHÔNG động đến Quang_TP_PhanTich)
                    quang = await _db.Set<Quang>().FirstOrDefaultAsync(x => x.ID == congThuc.ID_Quang_DauRa && !x.Da_Xoa, ct)
                        ?? throw new InvalidOperationException($"Không tìm thấy quặng đầu ra ID {congThuc.ID_Quang_DauRa}");
                    if (dto.QuangThanhPham != null)
                    {
                        quang.Ma_Quang = dto.QuangThanhPham.Ma_Quang;
                        quang.Ten_Quang = dto.QuangThanhPham.Ten_Quang;
                        quang.Loai_Quang = dto.QuangThanhPham.Loai_Quang;
                        if (dto.CongThucPhoi.Ngay_Tao.HasValue)
                            quang.Ngay_Tao = dto.CongThucPhoi.Ngay_Tao.Value;
                        quang.Ngay_Sua = DateTimeOffset.Now;
                        _db.Set<Quang>().Update(quang);

                        // Upsert thành phần hóa học quặng đầu ra vào Quang_TP_PhanTich nếu FE gửi vào
                        if (dto.QuangThanhPham.ThanhPhanHoaHoc?.Any() == true)
                        {
                            var oldTp = await _db.Set<Quang_TP_PhanTich>()
                                .Where(x => x.ID_Quang == quang.ID && !x.Da_Xoa)
                                .ToListAsync(ct);
                            if (oldTp.Count > 0)
                                _db.Set<Quang_TP_PhanTich>().RemoveRange(oldTp);

                            var now = DateTimeOffset.Now;
                            var newTp = dto.QuangThanhPham.ThanhPhanHoaHoc.Select(x => new Quang_TP_PhanTich
                            {
                                ID_Quang = quang.ID,
                                ID_TPHH = x.ID_TPHH,
                                Gia_Tri_PhanTram = x.Gia_Tri_PhanTram,
                                ThuTuTPHH = x.ThuTuTPHH,
                                Hieu_Luc_Tu = dto.CongThucPhoi.Ngay_Tao ?? now,
                                Hieu_Luc_Den = null,
                                Da_Xoa = false
                            }).ToList();
                            await _db.Set<Quang_TP_PhanTich>().AddRangeAsync(newTp, ct);
                        }
                    }

                    // Upsert CTP_ChiTiet_Quang (KHÔNG xóa để giữ ổn định ID cho bản ghi con)
                    var existingCtqs = await _db.Set<CTP_ChiTiet_Quang>()
                        .Where(x => x.ID_Cong_Thuc_Phoi == congThuc.ID)
                        .ToListAsync(ct);

                    var byOreId = existingCtqs.ToDictionary(x => x.ID_Quang_DauVao, x => x);
                    var inputOreIds = (dto.ChiTietQuang ?? new List<CTP_ChiTiet_QuangDto>()).Select(x => x.ID_Quang).ToHashSet();

                    var toAdd = new List<CTP_ChiTiet_Quang>();
                    var upserted = new List<CTP_ChiTiet_Quang>();

                    foreach (var input in dto.ChiTietQuang ?? Enumerable.Empty<CTP_ChiTiet_QuangDto>())
                    {
                        if (byOreId.TryGetValue(input.ID_Quang, out var ctq))
                        {
                            // Update existing ore - recalculate Path and Level
                            ctq.Ti_Le_Phan_Tram = input.Ti_Le_PhanTram;
                            // Map fields unconditionally; nulls are acceptable
                            ctq.Khau_Hao = input.Khau_Hao;
                            ctq.Ti_Le_KhaoHao = input.Ti_Le_KhaoHao;
                            ctq.KL_VaoLo = input.KL_VaoLo;
                            ctq.Ti_Le_HoiQuang = input.Ti_Le_HoiQuang;
                            ctq.KL_Nhan = input.KL_Nhan;
                            ctq.Da_Xoa = false;
                            
                            // Recalculate Path and Level for updated ore
                            // No Path/Level logic needed anymore
                            
                            _db.Set<CTP_ChiTiet_Quang>().Update(ctq);
                            upserted.Add(ctq);
                        }
                        else
                        {
                            // Insert mới - no Path/Level logic anymore
                            
                            var entity = new CTP_ChiTiet_Quang
                        {
                            ID_Cong_Thuc_Phoi = congThuc.ID,
                                ID_Quang_DauVao = input.ID_Quang,
                                Ti_Le_Phan_Tram = input.Ti_Le_PhanTram,
                            // Map fields unconditionally; nulls are acceptable
                            Khau_Hao = input.Khau_Hao,
                            Ti_Le_KhaoHao = input.Ti_Le_KhaoHao,
                            KL_VaoLo = input.KL_VaoLo,
                            Ti_Le_HoiQuang = input.Ti_Le_HoiQuang,
                            KL_Nhan = input.KL_Nhan,
                            Da_Xoa = false
                            };
                            toAdd.Add(entity);
                            upserted.Add(entity);
                        }
                    }

                    if (toAdd.Count > 0)
                    {
                        await _db.Set<CTP_ChiTiet_Quang>().AddRangeAsync(toAdd, ct);
                        // Lưu để có ID cho bản ghi mới trước khi xử lý con
                        await _db.SaveChangesAsync(ct);
                    }

                    // Soft delete các bản ghi không còn trong input
                    foreach (var ctq in existingCtqs)
                    {
                        if (!inputOreIds.Contains(ctq.ID_Quang_DauVao))
                        {
                            ctq.Da_Xoa = true;
                            _db.Set<CTP_ChiTiet_Quang>().Update(ctq);

                            // Đồng bộ con: soft delete tất cả con của ctq này
                            var childs = await _db.Set<CTP_ChiTiet_Quang_TPHH>()
                                .Where(x => x.ID_CTP_ChiTiet_Quang == ctq.ID && !x.Da_Xoa)
                                    .ToListAsync(ct);
                            foreach (var ch in childs)
                            {
                                ch.Da_Xoa = true;
                            }
                            if (childs.Count > 0)
                                _db.Set<CTP_ChiTiet_Quang_TPHH>().UpdateRange(childs);
                        }
                    }

                    // Upsert con: CTP_ChiTiet_Quang_TPHH theo từng ctq đã upsert
                    foreach (var input in dto.ChiTietQuang ?? Enumerable.Empty<CTP_ChiTiet_QuangDto>())
                    {
                        if (input.TP_HoaHocs?.Any() != true) continue;

                        var ctqEntity = upserted.First(x => x.ID_Quang_DauVao == input.ID_Quang);
                        var ctqId = ctqEntity.ID; // với bản ghi mới đã được save ở trên

                        var existingTps = await _db.Set<CTP_ChiTiet_Quang_TPHH>()
                            .Where(x => x.ID_CTP_ChiTiet_Quang == ctqId)
                            .ToListAsync(ct);
                        var byChem = existingTps.ToDictionary(x => x.ID_TPHH, x => x);
                        var inputChemIds = input.TP_HoaHocs.Select(tp => tp.Id).ToHashSet();

                        var tpToAdd = new List<CTP_ChiTiet_Quang_TPHH>();
                        foreach (var tp in input.TP_HoaHocs)
                        {
                            if (byChem.TryGetValue(tp.Id, out var child))
                            {
                                child.Gia_Tri_PhanTram = tp.PhanTram ?? 0;
                                child.Da_Xoa = false;
                                _db.Set<CTP_ChiTiet_Quang_TPHH>().Update(child);
                            }
                            else
                            {
                                tpToAdd.Add(new CTP_ChiTiet_Quang_TPHH
                                {
                                    ID_CTP_ChiTiet_Quang = ctqId,
                                    ID_TPHH = tp.Id,
                                    Gia_Tri_PhanTram = tp.PhanTram ?? 0,
                                    Da_Xoa = false
                                });
                            }
                        }

                        if (tpToAdd.Count > 0)
                            await _db.Set<CTP_ChiTiet_Quang_TPHH>().AddRangeAsync(tpToAdd, ct);

                        // Soft delete các bản ghi con không còn trong input
                        foreach (var child in existingTps)
                        {
                            if (!inputChemIds.Contains(child.ID_TPHH))
                            {
                                child.Da_Xoa = true;
                            }
                        }
                        var toUpdateDeleted = existingTps.Where(x => x.Da_Xoa).ToList();
                        if (toUpdateDeleted.Count > 0)
                            _db.Set<CTP_ChiTiet_Quang_TPHH>().UpdateRange(toUpdateDeleted);
                    }

                    // Replace constraints
                    var rbOld = await _db.Set<CTP_RangBuoc_TPHH>().Where(x => x.ID_Cong_Thuc_Phoi == congThuc.ID).ToListAsync(ct);
                    _db.Set<CTP_RangBuoc_TPHH>().RemoveRange(rbOld);
                    if (dto.RangBuocTPHH?.Any() == true)
                    {
                        var rbs = dto.RangBuocTPHH.Select(x => new CTP_RangBuoc_TPHH
                        {
                            ID_Cong_Thuc_Phoi = congThuc.ID,
                            ID_TPHH = x.ID_TPHH,
                            Min_PhanTram = x.Min_PhanTram,
                            Max_PhanTram = x.Max_PhanTram,
                            Da_Xoa = false
                        }).ToList();
                        await _db.Set<CTP_RangBuoc_TPHH>().AddRangeAsync(rbs, ct);
                    }

                    // Ensure plan link exists and update milestone
                    if (dto.CongThucPhoi.ID_Phuong_An > 0)
                    {
                        var existingLink = await _db.Set<PA_LuaChon_CongThuc>().FirstOrDefaultAsync(x => x.ID_Phuong_An == dto.CongThucPhoi.ID_Phuong_An && x.ID_Cong_Thuc_Phoi == congThuc.ID && !x.Da_Xoa, ct);
                        if (existingLink == null)
                        {
                            // Get next ThuTuPhoi for this plan
                            var nextThuTuPhoi = await _db.Set<PA_LuaChon_CongThuc>()
                                .Where(x => x.ID_Phuong_An == dto.CongThucPhoi.ID_Phuong_An && !x.Da_Xoa)
                                .MaxAsync(x => (int?)x.ThuTuPhoi, ct) ?? 0;
                            nextThuTuPhoi++;

                            var link = new PA_LuaChon_CongThuc
                            {
                                ID_Phuong_An = dto.CongThucPhoi.ID_Phuong_An,
                                ID_Cong_Thuc_Phoi = congThuc.ID,
                                ID_Quang_DauRa = congThuc.ID_Quang_DauRa,
                                Milestone = dto.Milestone,
                                ThuTuPhoi = nextThuTuPhoi,
                                Da_Xoa = false
                            };
                            await _db.Set<PA_LuaChon_CongThuc>().AddAsync(link, ct);
                        }
                        else
                        {
                            // Update milestone for existing link
                            existingLink.Milestone = dto.Milestone;
                            _db.Set<PA_LuaChon_CongThuc>().Update(existingLink);
                        }
                    }
                }
                else
                {
                    // CREATE FLOW (existing implementation)
                    quang = new Quang
                    {
                        Ma_Quang = dto.QuangThanhPham.Ma_Quang,
                        Ten_Quang = dto.QuangThanhPham.Ten_Quang,
                        Loai_Quang = dto.QuangThanhPham.Loai_Quang,
                        Dang_Hoat_Dong = true,
                        Da_Xoa = false,
                        Ghi_Chu = null,
                        Ngay_Tao = dto.CongThucPhoi.Ngay_Tao ?? DateTimeOffset.Now,
                        Nguoi_Tao = null,
                    };
                    await _db.Set<Quang>().AddAsync(quang, ct);
                    await _db.SaveChangesAsync(ct);

                    if (dto.QuangThanhPham.ThanhPhanHoaHoc?.Count > 0)
                    {
                        var tps = dto.QuangThanhPham.ThanhPhanHoaHoc.Select(x => new Quang_TP_PhanTich
                        {
                            ID_Quang = quang.ID,
                            ID_TPHH = x.ID_TPHH,
                            Gia_Tri_PhanTram = x.Gia_Tri_PhanTram,
                            ThuTuTPHH = x.ThuTuTPHH,
                            Hieu_Luc_Tu = DateTimeOffset.Now,
                            Da_Xoa = false
                        }).ToList();
                        await _db.Set<Quang_TP_PhanTich>().AddRangeAsync(tps, ct);
                    }

                    congThuc = new Cong_Thuc_Phoi
                    {
                        Ma_Cong_Thuc = dto.CongThucPhoi.Ma_Cong_Thuc ?? string.Empty,
                        Ten_Cong_Thuc = dto.CongThucPhoi.Ten_Cong_Thuc ?? string.Empty,
                        Ghi_Chu = dto.CongThucPhoi.Ghi_Chu,
                        ID_Quang_DauRa = quang.ID,
                        Hieu_Luc_Tu = dto.CongThucPhoi.Ngay_Tao ?? DateTimeOffset.Now,
                        Da_Xoa = false
                    };
                    await _db.Set<Cong_Thuc_Phoi>().AddAsync(congThuc, ct);
                    await _db.SaveChangesAsync(ct);

                    if (dto.ChiTietQuang?.Count > 0)
                    {
                        var inputs = new List<CTP_ChiTiet_Quang>();
                        
                        // Create input ores without Path/Level logic
                        foreach (var input in dto.ChiTietQuang)
                        {
                            inputs.Add(new CTP_ChiTiet_Quang
                        {
                            ID_Cong_Thuc_Phoi = congThuc.ID,
                                ID_Quang_DauVao = input.ID_Quang,
                                Ti_Le_Phan_Tram = input.Ti_Le_PhanTram,
                            // Fields newly mapped in entity; default to null when creating from mix
                            Khau_Hao = null,
                            Ti_Le_KhaoHao = null,
                            KL_VaoLo = null,
                            Ti_Le_HoiQuang = null,
                            KL_Nhan = null,
                            Da_Xoa = false
                            });
                        }
                        
                        await _db.Set<CTP_ChiTiet_Quang>().AddRangeAsync(inputs, ct);
                        // Cần lưu ngay để EF sinh ID cho từng CTP_ChiTiet_Quang trước khi tạo bản ghi con TPHH
                        await _db.SaveChangesAsync(ct);

                        // Lưu dữ liệu đã chỉnh sửa vào CTP_ChiTiet_Quang_TPHH
                        foreach (var input in dto.ChiTietQuang)
                        {
                            if (input.TP_HoaHocs?.Any() == true)
                            {
                                // Tìm ID của CTP_ChiTiet_Quang vừa tạo
                                var ctqId = inputs.First(x => x.ID_Quang_DauVao == input.ID_Quang).ID;
                                
                                // Xóa dữ liệu cũ (nếu có)
                                var oldTps = await _db.Set<CTP_ChiTiet_Quang_TPHH>()
                                    .Where(x => x.ID_CTP_ChiTiet_Quang == ctqId && !x.Da_Xoa)
                                    .ToListAsync(ct);
                                _db.Set<CTP_ChiTiet_Quang_TPHH>().RemoveRange(oldTps);

                                // Thêm dữ liệu mới đã chỉnh sửa
                                var newTps = input.TP_HoaHocs.Select(tp => new CTP_ChiTiet_Quang_TPHH
                                {
                                    ID_CTP_ChiTiet_Quang = ctqId,
                                    ID_TPHH = tp.Id,
                                    Gia_Tri_PhanTram = tp.PhanTram ?? 0,
                                    Da_Xoa = false
                                }).ToList();
                                await _db.Set<CTP_ChiTiet_Quang_TPHH>().AddRangeAsync(newTps, ct);
                            }
                        }
                    }

                    if (dto.RangBuocTPHH?.Count > 0)
                    {
                        var rbs = dto.RangBuocTPHH.Select(x => new CTP_RangBuoc_TPHH
                        {
                            ID_Cong_Thuc_Phoi = congThuc.ID,
                            ID_TPHH = x.ID_TPHH,
                            Min_PhanTram = x.Min_PhanTram,
                            Max_PhanTram = x.Max_PhanTram,
                            Da_Xoa = false
                        }).ToList();
                        await _db.Set<CTP_RangBuoc_TPHH>().AddRangeAsync(rbs, ct);
                    }

                    // Link formula to plan
                    if (dto.CongThucPhoi.ID_Phuong_An > 0)
                    {
                        // Get next ThuTuPhoi for this plan
                        var nextThuTuPhoi = await _db.Set<PA_LuaChon_CongThuc>()
                            .Where(x => x.ID_Phuong_An == dto.CongThucPhoi.ID_Phuong_An && !x.Da_Xoa)
                            .MaxAsync(x => (int?)x.ThuTuPhoi, ct) ?? 0;
                        nextThuTuPhoi++;

                        var link = new PA_LuaChon_CongThuc
                        {
                            ID_Phuong_An = dto.CongThucPhoi.ID_Phuong_An,
                            ID_Cong_Thuc_Phoi = congThuc.ID,
                            ID_Quang_DauRa = congThuc.ID_Quang_DauRa,
                            Milestone = dto.Milestone,
                            ThuTuPhoi = nextThuTuPhoi,
                            Da_Xoa = false
                        };
                        await _db.Set<PA_LuaChon_CongThuc>().AddAsync(link, ct);
                    }
                }

                await _db.SaveChangesAsync(ct);

                // Save output ore price to Quang_Gia_LichSu if provided
                if (dto.QuangThanhPham.Gia != null)
                {
                    // Dùng đúng thời điểm do FE chọn cho tỷ giá/giá
                    var eff = dto.QuangThanhPham.Gia.Ngay_Chon_TyGia;
                    await SaveOutputOrePriceAsync(quang.ID, dto.QuangThanhPham.Gia, eff, ct);
                }

                // After upsert details, create price/consumption snapshots per ore
                // Guard in case database schema of PA_Snapshot_Gia chưa được cập nhật
                try
                {
                    await CreateOrReplaceSnapshotsAsync(congThuc, dto, ct);
                }
                catch (Microsoft.Data.SqlClient.SqlException)
                {
                    // Bỏ qua snapshot nếu schema chưa có đủ cột để không chặn luồng mix
                }
                await tx.CommitAsync(ct);
                return isUpdate ? congThuc.ID_Quang_DauRa : quang.ID;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private async Task CreateOrReplaceSnapshotsAsync(Cong_Thuc_Phoi congThuc, MixQuangRequestDto dto, CancellationToken ct)
        {
            var ngayTinh = dto.CongThucPhoi.Ngay_Tao ?? DateTimeOffset.Now;
            int? idPa = dto.CongThucPhoi.ID_Phuong_An > 0 ? dto.CongThucPhoi.ID_Phuong_An : null;

            // Resolve MKN id if needed
            int? mknId = null;
            if (dto.Milestone.HasValue)
            {
                var code = await _db.Set<TP_HoaHoc>().AsNoTracking().Where(x => !x.Da_Xoa && x.Ma_TPHH == "MKN").Select(x => (int?)x.ID).FirstOrDefaultAsync(ct);
                mknId = code;
            }

            foreach (var input in dto.ChiTietQuang ?? Enumerable.Empty<CTP_ChiTiet_QuangDto>())
            {
                // Find current price at snapshot time
                var price = await _db.Set<Quang_Gia_LichSu>().AsNoTracking()
                    .Where(x => x.ID_Quang == input.ID_Quang && !x.Da_Xoa && x.Hieu_Luc_Tu <= ngayTinh)
                    .OrderByDescending(x => x.Hieu_Luc_Tu)
                    .FirstOrDefaultAsync(ct);

                var donGiaUsd = price?.Don_Gia_USD_1Tan ?? 0m;
                var tyGia = price?.Ty_Gia_USD_VND;
                var donGiaVnd = price?.Don_Gia_VND_1Tan;

                var ratio = input.Ti_Le_PhanTram;
                decimal mkn = 0m;
                if (dto.Milestone.HasValue && mknId.HasValue && input.TP_HoaHocs?.Any() == true)
                {
                    var found = input.TP_HoaHocs.FirstOrDefault(x => x.Id == mknId.Value);
                    if (found != null && found.PhanTram.HasValue) mkn = (decimal)found.PhanTram.Value;
                }
                var tieuHao = dto.Milestone.HasValue && mknId.HasValue ? ratio * (1 - (mkn / 100m)) : ratio;

                decimal? chiPhiTheoTiLeVnd = null;
                if (donGiaVnd.HasValue)
                {
                    chiPhiTheoTiLeVnd = donGiaVnd.Value * (ratio / 100m);
                }

                // Deactivate old active snapshot of the same logical key
                var oldActive = await _db.Set<PA_Snapshot_Gia>()
                    .Where(x => x.ID_Cong_Thuc_Phoi == congThuc.ID && x.ID_Quang == input.ID_Quang && x.ID_Phuong_An == idPa && x.Is_Active)
                    .ToListAsync(ct);
                if (oldActive.Count > 0)
                {
                    foreach (var oa in oldActive) oa.Is_Active = false;
                    _db.Set<PA_Snapshot_Gia>().UpdateRange(oldActive);
                }

                int version = 1;

                var snap = new PA_Snapshot_Gia
                {
                    // Các cột mới có thể chưa tồn tại ở DB cũ, đã bọc try/catch ở caller
                    ID_Phuong_An = idPa,
                    ID_Cong_Thuc_Phoi = congThuc.ID,
                    ID_Quang = input.ID_Quang,
                    Ti_Le_Phan_Tram = ratio,
                    He_So_Hao_Hut_DauVao = null,
                    Tieu_Hao_PhanTram = tieuHao,
                    Don_Gia_1Tan = donGiaUsd,
                    Tien_Te = price?.Tien_Te ?? "USD",
                    Ty_Gia_USD_VND = tyGia,
                    Don_Gia_VND_1Tan = donGiaVnd,
                    Chi_Phi_Theo_Ti_Le_VND = chiPhiTheoTiLeVnd,
                    Nguon_Gia_ID = price?.ID,
                    Version_No = version,
                    Is_Active = true,
                    Price_Override_By_User_ID = null,
                    Scope = 0,
                    Ghi_Chu = null,
                    Created_At = ngayTinh,
                    Created_By_User_ID = null,
                    Effective_At = ngayTinh
                };
                await _db.Set<PA_Snapshot_Gia>().AddAsync(snap, ct);
            }
        }

        public async Task<CongThucPhoiDetailResponse?> GetCongThucPhoiDetailAsync(int congThucPhoiId, CancellationToken ct = default)
        {
            var congThuc = await _db.Set<Cong_Thuc_Phoi>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == congThucPhoiId && !x.Da_Xoa, ct);

            if (congThuc == null) return null;

            // Get QuangDauRa
            var quangDauRa = await _db.Set<Quang>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == congThuc.ID_Quang_DauRa && !x.Da_Xoa, ct);

            if (quangDauRa == null) return null;

            // Get ChiTietQuang
            var chiTietQuang = await (from ctq in _db.Set<CTP_ChiTiet_Quang>().AsNoTracking()
                                    join q in _db.Set<Quang>().AsNoTracking() on ctq.ID_Quang_DauVao equals q.ID
                                    where ctq.ID_Cong_Thuc_Phoi == congThucPhoiId && !ctq.Da_Xoa && !q.Da_Xoa
                                    select new CTP_ChiTiet_QuangResponse(
                                        ctq.ID,
                                        ctq.ID_Cong_Thuc_Phoi,
                                        ctq.ID_Quang_DauVao,
                                        ctq.Ti_Le_Phan_Tram,
                                        ctq.Khau_Hao,
                                        ctq.Thu_Tu,
                                        ctq.Ghi_Chu,
                                        congThuc.Ma_Cong_Thuc,
                                        congThuc.Ten_Cong_Thuc,
                                        q.Ma_Quang,
                                        q.Ten_Quang
                                    )).ToListAsync(ct);

            // Get RangBuocTPHH
            var rangBuocTPHH = await (from rb in _db.Set<CTP_RangBuoc_TPHH>().AsNoTracking()
                                    join tphh in _db.Set<TP_HoaHoc>().AsNoTracking() on rb.ID_TPHH equals tphh.ID
                                    where rb.ID_Cong_Thuc_Phoi == congThucPhoiId && !rb.Da_Xoa && !tphh.Da_Xoa
                                    select new CTP_RangBuoc_TPHHResponse(
                                        rb.ID,
                                        rb.ID_Cong_Thuc_Phoi,
                                        rb.ID_TPHH,
                                        rb.Min_PhanTram,
                                        rb.Max_PhanTram,
                                        rb.Rang_Buoc_Cung,
                                        rb.Uu_Tien,
                                        rb.Ghi_Chu,
                                        congThuc.Ma_Cong_Thuc,
                                        congThuc.Ten_Cong_Thuc,
                                        tphh.Ma_TPHH,
                                        tphh.Ten_TPHH
                                    )).ToListAsync(ct);

            return new CongThucPhoiDetailResponse(
                congThuc.ID,
                congThuc.Ma_Cong_Thuc,
                congThuc.Ten_Cong_Thuc,
                congThuc.Ghi_Chu,
                0, // ID_Phuong_An - không có trong entity, để 0
                congThuc.ID_Quang_DauRa,
                0, // Tong_Ti_Le_Phoi - không có trong entity, để 0
                new QuangMinimal(quangDauRa.ID, quangDauRa.Ten_Quang ?? string.Empty),
                chiTietQuang,
                rangBuocTPHH
            );
        }

        public async Task<CongThucPhoiDetailMinimal?> GetDetailMinimalAsync(int congThucPhoiId, CancellationToken ct = default)
        {
            var cong = await _db.Set<Cong_Thuc_Phoi>().AsNoTracking().FirstOrDefaultAsync(x => x.ID == congThucPhoiId && !x.Da_Xoa, ct);
            if (cong is null) return null;

            var quangOut = await _db.Set<Quang>().AsNoTracking().FirstOrDefaultAsync(x => x.ID == cong.ID_Quang_DauRa && !x.Da_Xoa, ct);
            if (quangOut is null) return null;

            // Output ore chemistry
            var outChems = await _db.Set<Quang_TP_PhanTich>().AsNoTracking()
                .Where(x => x.ID_Quang == quangOut.ID && !x.Da_Xoa)
                .Join(_db.Set<TP_HoaHoc>().AsNoTracking(), a => a.ID_TPHH, b => b.ID, (a, b) => new { a, b })
                .OrderBy(x => x.a.ThuTuTPHH)
                .ThenBy(x => x.b.Ma_TPHH)
                .Select(x => new TPHHItem(x.b.ID, x.b.Ma_TPHH, x.b.Ten_TPHH, x.a.Gia_Tri_PhanTram, x.a.ThuTuTPHH))
                .ToListAsync(ct);

            // Input ores & their chemistry - sorted by dependency (deepest first)
            var ctqs = await _db.Set<CTP_ChiTiet_Quang>().AsNoTracking()
                .Where(x => x.ID_Cong_Thuc_Phoi == cong.ID && !x.Da_Xoa)
                .Join(_db.Set<Quang>().AsNoTracking(), a => a.ID_Quang_DauVao, q => q.ID, (a, q) => new { a, q })
                .OrderBy(x => x.a.ID_Quang_DauVao) // Simple ordering by ore ID
                .ToListAsync(ct);

            var inputOreIds = ctqs.Select(x => x.q.ID).Distinct().ToList();
            
            // Ưu tiên lấy dữ liệu đã chỉnh sửa từ CTP_ChiTiet_Quang_TPHH
            var chemDict = new Dictionary<int, List<TPHHValue>>();
            foreach (var ctq in ctqs)
            {
                var editedChems = await _db.Set<CTP_ChiTiet_Quang_TPHH>().AsNoTracking()
                    .Where(x => x.ID_CTP_ChiTiet_Quang == ctq.a.ID && !x.Da_Xoa)
                    .Join(_db.Set<TP_HoaHoc>().AsNoTracking(), a => a.ID_TPHH, b => b.ID, (a, b) => new { a, b })
                    .OrderBy(x => x.b.Ma_TPHH)
                    .Select(x => new TPHHValue(x.a.ID_TPHH, x.a.Gia_Tri_PhanTram, null))
                    .ToListAsync(ct);
                
                if (editedChems.Any())
                {
                    chemDict[ctq.q.ID] = editedChems;
                }
                else
                {
                    // Fallback về dữ liệu gốc từ Quang_TP_PhanTich
                    var originalChems = await _db.Set<Quang_TP_PhanTich>().AsNoTracking()
                        .Where(x => x.ID_Quang == ctq.q.ID && !x.Da_Xoa)
                        .OrderBy(x => x.ThuTuTPHH)
                        .ThenBy(x => x.ID_TPHH)
                        .Select(x => new TPHHValue(x.ID_TPHH, x.Gia_Tri_PhanTram, x.ThuTuTPHH))
                        .ToListAsync(ct);
                    chemDict[ctq.q.ID] = originalChems;
                }
            }

            // Get current prices for input ores
            var now = DateTimeOffset.Now;
            var prices = await _db.Set<Quang_Gia_LichSu>().AsNoTracking()
                .Where(p => !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                .GroupBy(p => p.ID_Quang)
                .Select(g => g.OrderByDescending(x => x.Hieu_Luc_Tu).First())
                .ToListAsync(ct);
            var priceMap = prices.ToDictionary(p => p.ID_Quang, p => p);

            var chiTiet = ctqs.Select(x =>
            {
                var chems = chemDict.TryGetValue(x.q.ID, out var list) ? list : new List<TPHHValue>();
                // Tính MKN từ TP_HoaHocs nếu có ID_TPHH = 20, nếu không có thì fallback về 0
                var mknFromChems = chems.FirstOrDefault(c => c.Id == 20)?.PhanTram ?? 0;
                
                // Get current price for this ore
                priceMap.TryGetValue(x.q.ID, out var price);
                
                return new ChiTietQuangChem(
                    x.q.ID,
                    x.q.Ten_Quang ?? string.Empty,
                    x.a.Ti_Le_Phan_Tram,
                    chems,
                    x.q.Loai_Quang,
                    price?.Don_Gia_USD_1Tan,
                    price?.Ty_Gia_USD_VND,
                    price?.Don_Gia_VND_1Tan,
                    // map milestone-specific fields
                    x.a.Khau_Hao,
                    x.a.Ti_Le_KhaoHao,
                    x.a.KL_VaoLo,
                    x.a.Ti_Le_HoiQuang,
                    x.a.KL_Nhan
                );
            }).ToList();

            // Sort inputs: mixed ores (Loai_Quang = 1) first, then others
            chiTiet = chiTiet
                .OrderBy(c => c.Loai_Quang == 1 ? 0 : 1)
                .ThenBy(c => c.ID_Quang)
                .ToList();

            // Constraints from cong-thuc
            var rbs = await _db.Set<CTP_RangBuoc_TPHH>().AsNoTracking()
                .Where(x => x.ID_Cong_Thuc_Phoi == cong.ID && !x.Da_Xoa)
                .Join(_db.Set<TP_HoaHoc>().AsNoTracking(), a => a.ID_TPHH, b => b.ID,
                    (a, b) => new RangBuocTPHHItem(b.ID, b.Ma_TPHH, b.Ten_TPHH, a.Min_PhanTram, a.Max_PhanTram))
                .ToListAsync(ct);

            // Get milestone for this specific formula from PA_LuaChon_CongThuc
            var milestone = await _db.Set<PA_LuaChon_CongThuc>().AsNoTracking()
                .Where(x => x.ID_Cong_Thuc_Phoi == cong.ID && !x.Da_Xoa)
                .Select(x => x.Milestone)
                .FirstOrDefaultAsync(ct);

            return new CongThucPhoiDetailMinimal(
                new CongThucInfo(cong.ID, cong.Ma_Cong_Thuc, cong.Ten_Cong_Thuc, cong.Ghi_Chu),
                new QuangChem(quangOut.ID, quangOut.Ma_Quang, quangOut.Ten_Quang ?? string.Empty, outChems),
                chiTiet,
                rbs,
                milestone
            );
        }

        public async Task<PhuongAnWithFormulasResponse?> GetFormulasByPlanAsync(int idPhuongAn, CancellationToken ct = default)
        {
            var plan = await _db.Set<Phuong_An_Phoi>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == idPhuongAn && !x.Da_Xoa, ct);
            if (plan is null) return null;

            // Get milestone from PA_LuaChon_CongThuc (assuming all formulas in a plan have the same milestone)
            var milestone = await _db.Set<PA_LuaChon_CongThuc>()
                .AsNoTracking()
                .Where(x => x.ID_Phuong_An == idPhuongAn && !x.Da_Xoa)
                .Select(x => x.Milestone)
                .FirstOrDefaultAsync(ct);

            var q = from pa in _db.Set<PA_LuaChon_CongThuc>().AsNoTracking()
                    join cong in _db.Set<Cong_Thuc_Phoi>().AsNoTracking() on pa.ID_Cong_Thuc_Phoi equals cong.ID
                    where pa.ID_Phuong_An == idPhuongAn && !pa.Da_Xoa && !cong.Da_Xoa
                    orderby cong.Hieu_Luc_Tu
                    select new
                    {
                        cong.ID,
                        cong.Ma_Cong_Thuc,
                        cong.Ten_Cong_Thuc,
                        cong.ID_Quang_DauRa,
                        pa.Milestone
                    };

            var formulas = await q.ToListAsync(ct);

            var quangIds = formulas.Select(f => f.ID_Quang_DauRa).Distinct().ToList();
            var quangMap = await _db.Set<Quang>()
                .AsNoTracking()
                .Where(qr => quangIds.Contains(qr.ID))
                .Select(qr => new { qr.ID, qr.Ten_Quang })
                .ToDictionaryAsync(x => x.ID, x => x.Ten_Quang ?? string.Empty, ct);

            var list = formulas.Select(f => new CongThucPhoiSummary(
                f.ID,
                f.Ma_Cong_Thuc,
                f.Ten_Cong_Thuc,
                f.ID_Quang_DauRa,
                quangMap.TryGetValue(f.ID_Quang_DauRa, out var tenQuang) ? tenQuang : string.Empty,
                f.Milestone
            )).ToList();

            return new PhuongAnWithFormulasResponse(
                plan.ID,
                plan.Ten_Phuong_An,
                plan.Ngay_Tinh_Toan,
                milestone,
                list);
        }

        public async Task<PhuongAnWithMilestonesResponse?> GetFormulasByPlanWithDetailsAsync(int idPhuongAn, CancellationToken ct = default)
        {
            // Get all formulas for this plan
            var plan = await _db.Set<Phuong_An_Phoi>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == idPhuongAn && !x.Da_Xoa, ct);
            if (plan is null) return null;

            var formulas = await _db.Set<PA_LuaChon_CongThuc>()
                .AsNoTracking()
                .Where(x => x.ID_Phuong_An == idPhuongAn && !x.Da_Xoa)
                .Join(_db.Set<Cong_Thuc_Phoi>().AsNoTracking(), 
                    pa => pa.ID_Cong_Thuc_Phoi, 
                    cong => cong.ID, 
                    (pa, cong) => new { pa, cong })
                .Where(x => !x.cong.Da_Xoa)
                .Select(x => new { 
                    x.cong.ID, 
                    x.pa.Milestone,
                    ThuTuPhoi = x.pa.ThuTuPhoi ?? 0
                })
                .OrderBy(x => x.ThuTuPhoi)
                .ToListAsync(ct);

            if (!formulas.Any()) 
            {
                // Get quặng kết quả ngay cả khi không có formulas
                var emptyQuangKetQua = await _db.PA_Quang_KQ
                    .AsNoTracking()
                    .Where(x => x.ID_PhuongAn == idPhuongAn)
                    .Join(_db.Quang.AsNoTracking(), 
                        pa => pa.ID_Quang, 
                        q => q.ID, 
                        (pa, q) => new QuangKetQuaInfo(
                            q.ID, 
                            q.Loai_Quang,
                            q.Ma_Quang ?? "",
                            q.Ten_Quang ?? ""
                        ))
                    .ToListAsync(ct);

                return new PhuongAnWithMilestonesResponse(
                    plan.ID,
                    plan.Ten_Phuong_An,
                    plan.Ngay_Tinh_Toan,
                    new List<CongThucPhoiDetailMinimal>(),
                    emptyQuangKetQua);
            }

            var formulaIds = formulas.Select(f => f.ID).ToList();
            var milestoneMap = formulas.ToDictionary(f => f.ID, f => f.Milestone);

            // Get all formula details
            var details = new List<CongThucPhoiDetailMinimal>();
            foreach (var formulaId in formulaIds)
            {
                var detail = await GetDetailMinimalAsync(formulaId, ct);
                if (detail != null)
                {
                    var milestoneFromMap = milestoneMap.GetValueOrDefault(formulaId);
                    var detailWithCorrectMilestone = new CongThucPhoiDetailMinimal(
                        detail.CongThuc,
                        detail.QuangDauRa,
                        detail.ChiTietQuang,
                        detail.RangBuocTPHH,
                        milestoneFromMap
                    );
                    details.Add(detailWithCorrectMilestone);
                }
            }

            // Sort by ThuTuPhoi (thứ tự phối trong plan)
            var sortedDetails = details
                .OrderBy(d => GetThuTuPhoiFromDetail(d, formulas))
                .ToList();

            // Get quặng kết quả (gang và xỉ) của phương án này
            var quangKetQuaList = await _db.PA_Quang_KQ
                .AsNoTracking()
                .Where(x => x.ID_PhuongAn == idPhuongAn)
                .Join(_db.Quang.AsNoTracking(), 
                    pa => pa.ID_Quang, 
                    q => q.ID, 
                    (pa, q) => new QuangKetQuaInfo(
                        q.ID, 
                        q.Loai_Quang,
                        q.Ma_Quang ?? "",
                        q.Ten_Quang ?? ""
                    ))
                .ToListAsync(ct);

            return new PhuongAnWithMilestonesResponse(
                plan.ID,
                plan.Ten_Phuong_An,
                plan.Ngay_Tinh_Toan,
                sortedDetails,
                quangKetQuaList);
        }




        private int GetThuTuPhoiFromDetail(CongThucPhoiDetailMinimal detail, IEnumerable<dynamic> allFormulas)
        {
            // Tìm ThuTuPhoi từ allFormulas dựa trên ID công thức
            foreach (var formula in allFormulas)
            {
                if (formula.ID == detail.CongThuc.Id)
                {
                    return formula.ThuTuPhoi;
                }
            }
            return 0;
        }

        // ============================================================
        // CLONE OPERATIONS (Plan & Milestones)
        // ============================================================

        private async Task<(int newCongThucId, int newOutOreId)> CloneFormulaCoreAsync(
            int sourceCongThucId,
            int targetPlanId,
            bool resetRatiosToZero,
            bool copySnapshots,
            bool copyDates,
            DateTimeOffset? cloneDate,
            int? sourcePlanIdForSnapshots,
            Dictionary<int,int>? inputOreRemap,
            CancellationToken ct)
        {
            // Load source formula and output ore
            var cong = await _db.Set<Cong_Thuc_Phoi>().FirstAsync(x => x.ID == sourceCongThucId && !x.Da_Xoa, ct);
            var quangOut = await _db.Set<Quang>().FirstAsync(x => x.ID == cong.ID_Quang_DauRa && !x.Da_Xoa, ct);

            // 1) Clone output ore (Quang)
            var planName = await _db.Set<Phuong_An_Phoi>().Where(x => x.ID == targetPlanId).Select(x => x.Ten_Phuong_An).FirstAsync(ct);
            var dateStr = (cloneDate ?? DateTimeOffset.Now).ToString("yyyyMMdd");
            var newOut = new Quang
            {
                Ma_Quang = BuildCode(100, quangOut.Ten_Quang ?? string.Empty, planName, dateStr, "copy"),
                Ten_Quang = quangOut.Ten_Quang,
                Loai_Quang = quangOut.Loai_Quang,
                Dang_Hoat_Dong = true,
                Da_Xoa = false,
                Ghi_Chu = quangOut.Ghi_Chu,
                Ngay_Tao = copyDates ? quangOut.Ngay_Tao : DateTimeOffset.Now,
                Ngay_Sua = DateTimeOffset.Now
            };
            await _db.Set<Quang>().AddAsync(newOut, ct);
            await _db.SaveChangesAsync(ct);

            // 1b) Clone Quang_TP_PhanTich for the new output ore
            var srcComps = await _db.Set<Quang_TP_PhanTich>().AsNoTracking()
                .Where(x => x.ID_Quang == quangOut.ID && !x.Da_Xoa)
                .ToListAsync(ct);
            if (srcComps.Count > 0)
            {
                var clonedComps = srcComps.Select(s => new Quang_TP_PhanTich
                {
                    ID_Quang = newOut.ID,
                    ID_TPHH = s.ID_TPHH,
                    Gia_Tri_PhanTram = s.Gia_Tri_PhanTram,
                    Hieu_Luc_Tu = copyDates ? s.Hieu_Luc_Tu : DateTimeOffset.Now,
                    Hieu_Luc_Den = copyDates ? s.Hieu_Luc_Den : null,
                    Nguon_Du_Lieu = s.Nguon_Du_Lieu,
                    Ghi_Chu = s.Ghi_Chu,
                    ThuTuTPHH = s.ThuTuTPHH,
                    Da_Xoa = false
                }).ToList();
                await _db.Set<Quang_TP_PhanTich>().AddRangeAsync(clonedComps, ct);
                await _db.SaveChangesAsync(ct);
            }

            // 2) Clone Cong_Thuc_Phoi
            var newCong = new Cong_Thuc_Phoi
            {
                Ma_Cong_Thuc = BuildCode(100, cong.Ten_Cong_Thuc ?? string.Empty, planName, dateStr, "copy"),
                Ten_Cong_Thuc = cong.Ten_Cong_Thuc,
                Ghi_Chu = cong.Ghi_Chu,
                ID_Quang_DauRa = newOut.ID,
                Hieu_Luc_Tu = copyDates ? cong.Hieu_Luc_Tu : DateTimeOffset.Now,
                Da_Xoa = false
            };
            await _db.Set<Cong_Thuc_Phoi>().AddAsync(newCong, ct);
            await _db.SaveChangesAsync(ct);

            // 3) Clone inputs CTP_ChiTiet_Quang
            var inputs = await _db.Set<CTP_ChiTiet_Quang>()
                .Where(x => x.ID_Cong_Thuc_Phoi == cong.ID && !x.Da_Xoa)
                .OrderBy(x => x.Thu_Tu)
                .ToListAsync(ct);
            var newInputs = new List<CTP_ChiTiet_Quang>();
            
            foreach (var inp in inputs)
            {
                var mappedOreId = (inputOreRemap != null && inputOreRemap.TryGetValue(inp.ID_Quang_DauVao, out var mapped)) ? mapped : inp.ID_Quang_DauVao;
                
                // No Path/Level logic needed anymore
                
                newInputs.Add(new CTP_ChiTiet_Quang
                {
                    ID_Cong_Thuc_Phoi = newCong.ID,
                    ID_Quang_DauVao = mappedOreId,
                    Ti_Le_Phan_Tram = resetRatiosToZero ? 0 : inp.Ti_Le_Phan_Tram,
                    Khau_Hao = inp.Khau_Hao,
                    Thu_Tu = inp.Thu_Tu,
                    Ghi_Chu = inp.Ghi_Chu,
                    Da_Xoa = false
                });
            }
            await _db.Set<CTP_ChiTiet_Quang>().AddRangeAsync(newInputs, ct);
            await _db.SaveChangesAsync(ct);

            // 4) Clone child TPHH per input
            for (int i = 0; i < inputs.Count; i++)
            {
                var inp = inputs[i];
                var newCtq = newInputs[i];
                var childs = await _db.Set<CTP_ChiTiet_Quang_TPHH>()
                    .Where(x => x.ID_CTP_ChiTiet_Quang == inp.ID && !x.Da_Xoa)
                    .ToListAsync(ct);
                if (childs.Count == 0) continue;
                var newChilds = childs.Select(ch => new CTP_ChiTiet_Quang_TPHH
                {
                    ID_CTP_ChiTiet_Quang = newCtq.ID,
                    ID_TPHH = ch.ID_TPHH,
                    Gia_Tri_PhanTram = ch.Gia_Tri_PhanTram,
                    Da_Xoa = false
                }).ToList();
                await _db.Set<CTP_ChiTiet_Quang_TPHH>().AddRangeAsync(newChilds, ct);
            }

            // 5) Clone constraints
            var rbs = await _db.Set<CTP_RangBuoc_TPHH>().Where(x => x.ID_Cong_Thuc_Phoi == cong.ID && !x.Da_Xoa).ToListAsync(ct);
            if (rbs.Count > 0)
            {
                var newRbs = rbs.Select(r => new CTP_RangBuoc_TPHH
                {
                    ID_Cong_Thuc_Phoi = newCong.ID,
                    ID_TPHH = r.ID_TPHH,
                    Min_PhanTram = r.Min_PhanTram,
                    Max_PhanTram = r.Max_PhanTram,
                    Da_Xoa = false
                }).ToList();
                await _db.Set<CTP_RangBuoc_TPHH>().AddRangeAsync(newRbs, ct);
            }

            // 6) Link to target plan
            var srcLink = await _db.Set<PA_LuaChon_CongThuc>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID_Cong_Thuc_Phoi == cong.ID && !x.Da_Xoa, ct);
            
            // Get next ThuTuPhoi for target plan
            var nextThuTuPhoi = await _db.Set<PA_LuaChon_CongThuc>()
                .Where(x => x.ID_Phuong_An == targetPlanId && !x.Da_Xoa)
                .MaxAsync(x => (int?)x.ThuTuPhoi, ct) ?? 0;
            nextThuTuPhoi++;

            var newLink = new PA_LuaChon_CongThuc
            {
                ID_Phuong_An = targetPlanId,
                ID_Cong_Thuc_Phoi = newCong.ID,
                ID_Quang_DauRa = newOut.ID,
                Milestone = srcLink?.Milestone,
                ThuTuPhoi = nextThuTuPhoi,
                Da_Xoa = false
            };
            await _db.Set<PA_LuaChon_CongThuc>().AddAsync(newLink, ct);

            // 7) Clone snapshots optional
            if (copySnapshots && sourcePlanIdForSnapshots.HasValue)
            {
                var srcSnaps = await _db.Set<PA_Snapshot_Gia>().AsNoTracking()
                    .Where(x => x.ID_Cong_Thuc_Phoi == cong.ID && x.ID_Phuong_An == sourcePlanIdForSnapshots.Value && x.Is_Active)
                    .ToListAsync(ct);
                foreach (var s in srcSnaps)
                {
                    var snap = new PA_Snapshot_Gia
                    {
                        ID_Phuong_An = targetPlanId,
                        ID_Cong_Thuc_Phoi = newCong.ID,
                        ID_Quang = s.ID_Quang,
                        Ti_Le_Phan_Tram = resetRatiosToZero ? 0 : s.Ti_Le_Phan_Tram,
                        He_So_Hao_Hut_DauVao = s.He_So_Hao_Hut_DauVao,
                        Tieu_Hao_PhanTram = s.Tieu_Hao_PhanTram,
                        Don_Gia_1Tan = s.Don_Gia_1Tan,
                        Tien_Te = s.Tien_Te,
                        Ty_Gia_USD_VND = s.Ty_Gia_USD_VND,
                        Don_Gia_VND_1Tan = s.Don_Gia_VND_1Tan,
                        Chi_Phi_Theo_Ti_Le_VND = s.Chi_Phi_Theo_Ti_Le_VND,
                        Nguon_Gia_ID = s.Nguon_Gia_ID,
                        Version_No = 1,
                        Is_Active = true,
                        Created_At = copyDates ? s.Created_At : DateTimeOffset.Now,
                        Effective_At = copyDates ? s.Effective_At : DateTimeOffset.Now
                    };
                    await _db.Set<PA_Snapshot_Gia>().AddAsync(snap, ct);
                }
            }

            await _db.SaveChangesAsync(ct);
            return (newCong.ID, newOut.ID);
        }

        private async Task<(int newFormulaId, int newOutOreId)> CloneFormulaRecursiveAsync(
            int sourceFormulaId,
            int sourcePlanId,
            int targetPlanId,
            bool resetRatiosToZero,
            bool copySnapshots,
            bool copyDates,
            DateTimeOffset? cloneDate,
            Dictionary<int,int> formulaMap,
            Dictionary<int,int> outOreMap,
            Dictionary<int,int> outOreToFormulaMap,
            CancellationToken ct)
        {
            // If cloned already, return
            if (formulaMap.TryGetValue(sourceFormulaId, out var existingNewFormulaId))
            {
                var existingOut = outOreMap.FirstOrDefault(kv => outOreToFormulaMap.TryGetValue(kv.Key, out var fId) && fId == sourceFormulaId).Value;
                return (existingNewFormulaId, existingOut);
            }

            // Find dependencies: inputs that are output of other formulas in same plan
            var inputs = await _db.Set<CTP_ChiTiet_Quang>().AsNoTracking()
                .Where(x => x.ID_Cong_Thuc_Phoi == sourceFormulaId && !x.Da_Xoa)
                .Select(x => x.ID_Quang_DauVao)
                .Distinct()
                .ToListAsync(ct);

            var deps = inputs
                .Where(oreId => outOreToFormulaMap.ContainsKey(oreId))
                .Select(oreId => outOreToFormulaMap[oreId])
                .Distinct()
                .ToList();

            // Clone dependencies first
            foreach (var depFormulaId in deps)
            {
                await CloneFormulaRecursiveAsync(depFormulaId, sourcePlanId, targetPlanId, resetRatiosToZero, copySnapshots, copyDates, cloneDate, formulaMap, outOreMap, outOreToFormulaMap, ct);
            }

            // Prepare remap dictionary from old input ore ids to newly cloned ore ids
            var remap = new Dictionary<int,int>();
            foreach (var oreId in inputs)
            {
                if (outOreMap.TryGetValue(oreId, out var newOre)) remap[oreId] = newOre;
            }

            // Clone this formula using remapped inputs
            var result = await CloneFormulaCoreAsync(sourceFormulaId, targetPlanId, resetRatiosToZero, copySnapshots, copyDates, cloneDate, sourcePlanId, remap, ct);
            formulaMap[sourceFormulaId] = result.newCongThucId;

            // Map old output ore -> new output ore
            var oldOut = await _db.Set<Cong_Thuc_Phoi>().AsNoTracking().Where(x => x.ID == sourceFormulaId).Select(x => x.ID_Quang_DauRa).FirstAsync(ct);
            outOreMap[oldOut] = result.newOutOreId;

            return result;
        }

        public async Task<int> ClonePlanAsync(ClonePlanRequestDto dto, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var source = await _db.Set<Phuong_An_Phoi>().FirstOrDefaultAsync(x => x.ID == dto.SourcePlanId && !x.Da_Xoa, ct)
                    ?? throw new InvalidOperationException($"Không tìm thấy Phương Án nguồn ID {dto.SourcePlanId}");

                // Create target plan
                var target = new Phuong_An_Phoi
                {
                    Ten_Phuong_An = dto.NewPlanName,
                    ID_Quang_Dich = source.ID_Quang_Dich,
                    Ngay_Tinh_Toan = dto.CopyDates ? source.Ngay_Tinh_Toan : DateTimeOffset.Now,
                    Phien_Ban = source.Phien_Ban,
                    Trang_Thai = dto.CopyStatuses ? source.Trang_Thai : (byte)0,
                    Muc_Tieu = source.Muc_Tieu,
                    Ghi_Chu = $"Cloned from Plan {source.ID}",
                    Da_Xoa = false
                };
                await _db.Set<Phuong_An_Phoi>().AddAsync(target, ct);
                await _db.SaveChangesAsync(ct);

                // Clone process parameter configurations (PA_ProcessParamValue) from source plan to target plan
                var srcParamConfigs = await _db.PA_ProcessParamValue
                    .AsNoTracking()
                    .Where(x => x.ID_Phuong_An == source.ID)
                    .ToListAsync(ct);

                if (srcParamConfigs.Count > 0)
                {
                    var clonedParamConfigs = srcParamConfigs.Select(x => new PA_ProcessParamValue
                    {
                        ID_Phuong_An = target.ID,
                        ID_ProcessParam = x.ID_ProcessParam,
                        ThuTuParam = x.ThuTuParam,
                        GiaTri = x.GiaTri,
                        Ngay_Tao = DateTime.Now,
                        Nguoi_Tao = x.Nguoi_Tao
                    }).ToList();

                    await _db.PA_ProcessParamValue.AddRangeAsync(clonedParamConfigs, ct);
                    await _db.SaveChangesAsync(ct);
                }

                // Clone quặng kết quả (gang và xỉ) từ phương án gốc
                var quangKetQuaGoc = await _db.PA_Quang_KQ
                    .Where(x => x.ID_PhuongAn == source.ID)
                    .ToListAsync(ct);

                foreach (var mapping in quangKetQuaGoc)
                {
                    await CloneQuangKetQuaAsync(mapping.ID_Quang, target.ID, source.ID_Quang_Dich, ct);
                }

                // Clone cấu hình thống kê (PA_ThongKe_Result) từ phương án gốc
                var srcStatisticalConfigs = await _db.PA_ThongKe_Result
                    .AsNoTracking()
                    .Where(x => x.ID_PhuongAn == source.ID)
                    .ToListAsync(ct);

                if (srcStatisticalConfigs.Count > 0)
                {
                    var clonedStatisticalConfigs = srcStatisticalConfigs.Select(x => new PA_ThongKe_Result
                    {
                        ID_PhuongAn = target.ID,
                        ID_ThongKe_Function = x.ID_ThongKe_Function,
                        ThuTu = x.ThuTu,
                        GiaTri = 0m, // Không copy giá trị tính toán, chỉ copy cấu hình
                        Ngay_Tinh = DateTime.Now,
                        Nguoi_Tinh = null
                    }).ToList();

                    await _db.PA_ThongKe_Result.AddRangeAsync(clonedStatisticalConfigs, ct);
                    await _db.SaveChangesAsync(ct);
                }

                // Get all formula links
                var links = await _db.Set<PA_LuaChon_CongThuc>().AsNoTracking()
                    .Where(x => x.ID_Phuong_An == source.ID && !x.Da_Xoa)
                    .ToListAsync(ct);
                // Build map oldOutOre -> sourceFormula
                var outOreToFormulaMap = links.ToDictionary(l => _db.Set<Cong_Thuc_Phoi>().AsNoTracking().Where(x => x.ID == l.ID_Cong_Thuc_Phoi).Select(x => x.ID_Quang_DauRa).First(), l => l.ID_Cong_Thuc_Phoi);
                var outOreMap = new Dictionary<int,int>();
                var formulaMap = new Dictionary<int,int>();
                foreach (var link in links)
                {
                    await CloneFormulaRecursiveAsync(link.ID_Cong_Thuc_Phoi, source.ID, target.ID, dto.ResetRatiosToZero, dto.CopySnapshots, dto.CopyDates, dto.CloneDate, formulaMap, outOreMap, outOreToFormulaMap, ct);
                }

                await tx.CommitAsync(ct);
                return target.ID;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<int> CloneMilestonesAsync(CloneMilestonesRequestDto dto, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var source = await _db.Set<Phuong_An_Phoi>().FirstOrDefaultAsync(x => x.ID == dto.SourcePlanId && !x.Da_Xoa, ct)
                    ?? throw new InvalidOperationException($"Không tìm thấy Phương Án nguồn ID {dto.SourcePlanId}");

                // Ensure target plan
                Phuong_An_Phoi target;
                if (dto.TargetPlanId.HasValue && dto.TargetPlanId.Value > 0)
                {
                    target = await _db.Set<Phuong_An_Phoi>().FirstOrDefaultAsync(x => x.ID == dto.TargetPlanId && !x.Da_Xoa, ct)
                        ?? throw new InvalidOperationException($"Không tìm thấy Phương Án đích ID {dto.TargetPlanId}");
                }
                else
                {
                    target = new Phuong_An_Phoi
                    {
                        Ten_Phuong_An = $"Clone of {source.Ten_Phuong_An}",
                        ID_Quang_Dich = source.ID_Quang_Dich,
                        Ngay_Tinh_Toan = DateTimeOffset.Now,
                        Phien_Ban = source.Phien_Ban,
                        Trang_Thai = 0,
                        Muc_Tieu = source.Muc_Tieu,
                        Ghi_Chu = $"Cloned from Plan {source.ID}",
                        Da_Xoa = false
                    };
                    await _db.Set<Phuong_An_Phoi>().AddAsync(target, ct);
                    await _db.SaveChangesAsync(ct);
                }

                // Pick links to clone
                var allLinks = await _db.Set<PA_LuaChon_CongThuc>().AsNoTracking()
                    .Where(x => x.ID_Phuong_An == source.ID && !x.Da_Xoa)
                    .ToListAsync(ct);

                IEnumerable<PA_LuaChon_CongThuc> pickedLinks = allLinks;
                if (dto.CloneItems?.Any() == true)
                {
                    var byFormula = new HashSet<int>(dto.CloneItems.SelectMany(i => i.FormulaIds ?? Array.Empty<int>()));
                    var byMilestone = new HashSet<int>(dto.CloneItems.Where(i => i.Milestone.HasValue).Select(i => i.Milestone!.Value));
                    pickedLinks = allLinks.Where(l => byFormula.Contains(l.ID_Cong_Thuc_Phoi) || (l.Milestone.HasValue && byMilestone.Contains(l.Milestone.Value)));
                }

                var outOreToFormulaMap2 = allLinks.ToDictionary(l => _db.Set<Cong_Thuc_Phoi>().AsNoTracking().Where(x => x.ID == l.ID_Cong_Thuc_Phoi).Select(x => x.ID_Quang_DauRa).First(), l => l.ID_Cong_Thuc_Phoi);
                var outOreMap = new Dictionary<int,int>();
                var formulaMap = new Dictionary<int,int>();
                foreach (var link in pickedLinks)
                {
                    await CloneFormulaRecursiveAsync(link.ID_Cong_Thuc_Phoi, source.ID, target.ID, dto.ResetRatiosToZero, dto.CopySnapshots, dto.CopyDates, dto.CloneDate, formulaMap, outOreMap, outOreToFormulaMap2, ct);
                }

                await tx.CommitAsync(ct);
                return target.ID;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<bool> DeletePlanAsync(int id, CancellationToken ct = default)
        {
            // Delegate to the comprehensive deletion to ensure all related data is removed consistently
            return await DeletePlanWithRelatedDataAsync(id, ct);
        }

        /// <summary>
        /// Clone quặng (gang hoặc xỉ) thành quặng kết quả cho phương án
        /// </summary>
        /// <param name="idQuangNguon">ID quặng nguồn (gang đích hoặc quặng kết quả hiện tại)</param>
        /// <param name="idPhuongAn">ID phương án đích</param>
        /// <param name="idGangDich">ID gang đích của phương án (để set cho gang kết quả)</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>ID quặng kết quả mới được tạo</returns>
        public async Task<int> CloneQuangKetQuaAsync(int idQuangNguon, int idPhuongAn, int? idGangDich = null, CancellationToken ct = default)
        {
            // Lấy thông tin phương án để tạo tên
            var phuongAn = await _db.Phuong_An_Phoi.FindAsync(idPhuongAn);
            if (phuongAn == null) throw new ArgumentException($"Phương án {idPhuongAn} không tồn tại");

            // Lấy thông tin quặng nguồn
            var quangNguon = await _db.Quang.FindAsync(idQuangNguon);
            if (quangNguon == null) throw new ArgumentException($"Quặng nguồn {idQuangNguon} không tồn tại");

            // Tạo tên và mã cho quặng kết quả
            var tenQuangKetQua = $"{phuongAn.Ten_Phuong_An} quặng kết quả";
            var maQuangKetQua = ToSlug(tenQuangKetQua);

            // Tạo quặng kết quả mới
            var quangKetQua = new Quang
            {
                Ma_Quang = maQuangKetQua,
                Ten_Quang = tenQuangKetQua,
                Loai_Quang = quangNguon.Loai_Quang, // Giữ nguyên loại (Gang=2, Xỉ=4)
                Dang_Hoat_Dong = true,
                Da_Xoa = false,
                Ghi_Chu = $"Quặng kết quả cho phương án {phuongAn.Ten_Phuong_An}",
                Ngay_Tao = DateTime.Now,
                ID_Quang_Gang = quangNguon.Loai_Quang == 2 ? idGangDich : quangNguon.ID_Quang_Gang // Gang kết quả link với gang đích, Xỉ giữ nguyên
            };

            _db.Quang.Add(quangKetQua);
            await _db.SaveChangesAsync(ct);

            // Clone toàn bộ thành phần hóa học từ quặng nguồn
            var thanhPhanNguon = await _db.Quang_TP_PhanTich
                .Where(x => x.ID_Quang == idQuangNguon)
                .ToListAsync(ct);

            var thanhPhanMoi = thanhPhanNguon.Select(x => new Quang_TP_PhanTich
            {
                ID_Quang = quangKetQua.ID,
                ID_TPHH = x.ID_TPHH,
                Gia_Tri_PhanTram = 0,
                ThuTuTPHH = x.ThuTuTPHH,
                CalcFormula = x.CalcFormula,
                IsCalculated = x.IsCalculated,
                // Không clone khối lượng, để tính lại theo công thức
            }).ToList();

            _db.Quang_TP_PhanTich.AddRange(thanhPhanMoi);

            // Map vào PA_Quang_KQ
            var paQuangKq = new PA_Quang_KQ
            {
                ID_PhuongAn = idPhuongAn,
                ID_Quang = quangKetQua.ID,
                LoaiQuang = quangKetQua.Loai_Quang // 2 = Gang, 4 = Xỉ
            };

            _db.PA_Quang_KQ.Add(paQuangKq);
            await _db.SaveChangesAsync(ct);

            return quangKetQua.ID;
        }

        /// <summary>
        /// Upsert phương án phối với logic clone quặng gang kết quả khi tạo mới
        /// </summary>
        public async Task<int> UpsertPhuongAnPhoiAsync(Phuong_An_PhoiUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.ID is null or 0)
            {
                // Tạo mới phương án phối
                var entity = new Phuong_An_Phoi
                {
                    Ten_Phuong_An = dto.Phuong_An_Phoi.Ten_Phuong_An,
                    ID_Quang_Dich = dto.Phuong_An_Phoi.ID_Quang_Dich,
                    Ngay_Tinh_Toan = dto.Phuong_An_Phoi.Ngay_Tinh_Toan,
                    Phien_Ban = dto.Phuong_An_Phoi.Phien_Ban,
                    Trang_Thai = dto.Phuong_An_Phoi.Trang_Thai,
                    Muc_Tieu = dto.Phuong_An_Phoi.Muc_Tieu,
                    Ghi_Chu = dto.Phuong_An_Phoi.Ghi_Chu,
                    Da_Xoa = false
                };

                _db.Phuong_An_Phoi.Add(entity);
                await _db.SaveChangesAsync(ct);

                // Clone quặng gang kết quả cho phương án này
                // Lấy ID_Quang_Dich làm gang đích để clone
                await CloneQuangKetQuaAsync(dto.Phuong_An_Phoi.ID_Quang_Dich, entity.ID, dto.Phuong_An_Phoi.ID_Quang_Dich, ct);

                return entity.ID;
            }
            else
            {
                // Cập nhật phương án phối (không clone quặng)
                var entity = await _db.Phuong_An_Phoi.FindAsync(dto.ID.Value);
                if (entity == null) return 0;

                entity.Ten_Phuong_An = dto.Phuong_An_Phoi.Ten_Phuong_An;
                entity.ID_Quang_Dich = dto.Phuong_An_Phoi.ID_Quang_Dich;
                entity.Ngay_Tinh_Toan = dto.Phuong_An_Phoi.Ngay_Tinh_Toan;
                entity.Phien_Ban = dto.Phuong_An_Phoi.Phien_Ban;
                entity.Trang_Thai = dto.Phuong_An_Phoi.Trang_Thai;
                entity.Muc_Tieu = dto.Phuong_An_Phoi.Muc_Tieu;
                entity.Ghi_Chu = dto.Phuong_An_Phoi.Ghi_Chu;

                _db.Phuong_An_Phoi.Update(entity);
                await _db.SaveChangesAsync(ct);
                return dto.ID.Value;
            }
        }

        /// <summary>
        /// Xóa hoàn toàn phương án và tất cả dữ liệu liên quan
        /// </summary>
        public async Task<bool> DeletePlanWithRelatedDataAsync(int planId, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // 1. Xóa quặng kết quả (Gang và Xỉ) và thành phần hóa học
                var quangKetQuaIds = await _db.PA_Quang_KQ
                    .Where(x => x.ID_PhuongAn == planId)
                    .Select(x => x.ID_Quang)
                    .ToListAsync(ct);

                if (quangKetQuaIds.Any())
                {
                    // Xóa thành phần hóa học của quặng kết quả
                    var thanhPhanToDelete = await _db.Quang_TP_PhanTich
                        .Where(x => quangKetQuaIds.Contains(x.ID_Quang))
                        .ToListAsync(ct);
                    _db.Quang_TP_PhanTich.RemoveRange(thanhPhanToDelete);

                    // Xóa quặng kết quả
                    var quangToDelete = await _db.Quang
                        .Where(x => quangKetQuaIds.Contains(x.ID))
                        .ToListAsync(ct);
                    _db.Quang.RemoveRange(quangToDelete);

                    // Xóa mapping PA_Quang_KQ
                    var mappingToDelete = await _db.PA_Quang_KQ
                        .Where(x => x.ID_PhuongAn == planId)
                        .ToListAsync(ct);
                    _db.PA_Quang_KQ.RemoveRange(mappingToDelete);
                }

                // 2. Xóa công thức phối và chi tiết liên quan
                var congThucIds = await _db.PA_LuaChon_CongThuc
                    .Where(x => x.ID_Phuong_An == planId)
                    .Select(x => x.ID_Cong_Thuc_Phoi)
                    .ToListAsync(ct);

                if (congThucIds.Any())
                {
                    // Lấy danh sách quặng đầu ra từ công thức phối
                    var quangDauRaIds = await _db.Cong_Thuc_Phoi
                        .Where(x => congThucIds.Contains(x.ID))
                        .Select(x => x.ID_Quang_DauRa)
                        .ToListAsync(ct);

                    // Xóa thành phần hóa học của quặng đầu ra
                    if (quangDauRaIds.Any())
                    {
                        var thanhPhanDauRaToDelete = await _db.Quang_TP_PhanTich
                            .Where(x => quangDauRaIds.Contains(x.ID_Quang))
                            .ToListAsync(ct);
                        _db.Quang_TP_PhanTich.RemoveRange(thanhPhanDauRaToDelete);

                        // Xóa quặng đầu ra
                        var quangDauRaToDelete = await _db.Quang
                            .Where(x => quangDauRaIds.Contains(x.ID))
                            .ToListAsync(ct);
                        _db.Quang.RemoveRange(quangDauRaToDelete);
                    }

                    // Xóa chi tiết quặng và thành phần hóa học
                    var chiTietQuangIds = await _db.CTP_ChiTiet_Quang
                        .Where(x => congThucIds.Contains(x.ID_Cong_Thuc_Phoi))
                        .Select(x => x.ID)
                        .ToListAsync(ct);

                    if (chiTietQuangIds.Any())
                    {
                        // Xóa thành phần hóa học chi tiết
                        var tpHHChiTietToDelete = await _db.CTP_ChiTiet_Quang_TPHH
                            .Where(x => chiTietQuangIds.Contains(x.ID_CTP_ChiTiet_Quang))
                            .ToListAsync(ct);
                        _db.CTP_ChiTiet_Quang_TPHH.RemoveRange(tpHHChiTietToDelete);
                    }

                    // Xóa chi tiết quặng
                    var chiTietToDelete = await _db.CTP_ChiTiet_Quang
                        .Where(x => congThucIds.Contains(x.ID_Cong_Thuc_Phoi))
                        .ToListAsync(ct);
                    _db.CTP_ChiTiet_Quang.RemoveRange(chiTietToDelete);

                    // Xóa ràng buộc TPHH
                    var rangBuocToDelete = await _db.CTP_RangBuoc_TPHH
                        .Where(x => congThucIds.Contains(x.ID_Cong_Thuc_Phoi))
                        .ToListAsync(ct);
                    _db.CTP_RangBuoc_TPHH.RemoveRange(rangBuocToDelete);

                    // Xóa công thức phối
                    var congThucToDelete = await _db.Cong_Thuc_Phoi
                        .Where(x => congThucIds.Contains(x.ID))
                        .ToListAsync(ct);
                    _db.Cong_Thuc_Phoi.RemoveRange(congThucToDelete);
                }

                // 3. Xóa mapping công thức với plan
                var luaChonToDelete = await _db.PA_LuaChon_CongThuc
                    .Where(x => x.ID_Phuong_An == planId)
                    .ToListAsync(ct);
                _db.PA_LuaChon_CongThuc.RemoveRange(luaChonToDelete);

                // 4. Xóa tham số process
                var processParamToDelete = await _db.PA_ProcessParamValue
                    .Where(x => x.ID_Phuong_An == planId)
                    .ToListAsync(ct);
                _db.PA_ProcessParamValue.RemoveRange(processParamToDelete);

                // 5. Xóa snapshots
                var snapshotTpHHToDelete = await _db.PA_Snapshot_TPHH
                    .Where(x => x.ID_Phuong_An == planId)
                    .ToListAsync(ct);
                _db.PA_Snapshot_TPHH.RemoveRange(snapshotTpHHToDelete);

                var snapshotGiaToDelete = await _db.PA_Snapshot_Gia
                    .Where(x => x.ID_Phuong_An == planId)
                    .ToListAsync(ct);
                _db.PA_Snapshot_Gia.RemoveRange(snapshotGiaToDelete);

                // 6. Xóa kết quả tổng hợp
                var ketQuaToDelete = await _db.PA_KetQua_TongHop
                    .Where(x => x.ID_Phuong_An == planId)
                    .ToListAsync(ct);
                _db.PA_KetQua_TongHop.RemoveRange(ketQuaToDelete);

				// 7. Xóa cấu hình thống kê (PA_ThongKe_Result)
				var thongKeToDelete = await _db.PA_ThongKe_Result
					.Where(x => x.ID_PhuongAn == planId)
					.ToListAsync(ct);
				_db.PA_ThongKe_Result.RemoveRange(thongKeToDelete);

				// 8. Cuối cùng xóa chính phương án
				var planToDelete = await _db.Phuong_An_Phoi.FindAsync(planId);
				if (planToDelete != null)
				{
					_db.Phuong_An_Phoi.Remove(planToDelete);
				}

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return true;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private async Task SaveOutputOrePriceAsync(int quangId, QuangGiaDto giaDto, DateTimeOffset effectiveDate, CancellationToken ct)
        {
            // Upsert theo quặng + khoảng hiệu lực (nếu đã có bản ghi đang hiệu lực thì update, nếu chưa thì insert)
            var active = await _db.Set<Quang_Gia_LichSu>()
                .FirstOrDefaultAsync(x => x.ID_Quang == quangId && !x.Da_Xoa && x.Hieu_Luc_Den == null, ct);

            if (active != null)
            {
                // Update in-place
                active.Don_Gia_USD_1Tan = giaDto.Gia_USD_1Tan;
                active.Ty_Gia_USD_VND = giaDto.Ty_Gia_USD_VND;
                active.Don_Gia_VND_1Tan = giaDto.Gia_VND_1Tan;
                active.Hieu_Luc_Tu = effectiveDate;
                active.Ghi_Chu = "Giá từ phối trộn quặng";
                _db.Set<Quang_Gia_LichSu>().Update(active);
            }
            else
            {
                // Insert mới
                var newPrice = new Quang_Gia_LichSu
                {
                    ID_Quang = quangId,
                    Don_Gia_USD_1Tan = giaDto.Gia_USD_1Tan,
                    Ty_Gia_USD_VND = giaDto.Ty_Gia_USD_VND,
                    Don_Gia_VND_1Tan = giaDto.Gia_VND_1Tan,
                    Tien_Te = "USD",
                    Hieu_Luc_Tu = effectiveDate,
                    Hieu_Luc_Den = null,
                    Ghi_Chu = "Giá từ phối trộn quặng",
                    Da_Xoa = false
                };
                await _db.Set<Quang_Gia_LichSu>().AddAsync(newPrice, ct);
            }
        }
    }
}