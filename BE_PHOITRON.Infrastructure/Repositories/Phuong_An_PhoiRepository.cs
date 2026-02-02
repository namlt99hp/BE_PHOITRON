using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Enums;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class Phuong_An_PhoiRepository : BaseRepository<Phuong_An_Phoi>, IPhuong_An_PhoiRepository
    {
        public Phuong_An_PhoiRepository(AppDbContext db) : base(db) { }


        // ========== ThieuKet Section ==========

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
                var result = await MixInternalAsync(dto, ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private async Task<int> MixInternalAsync(MixQuangRequestDto dto, CancellationToken ct = default)
        {
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
                            // Chỉ xử lý nếu PhanTram có giá trị (không null và > 0)
                            // Nếu null hoặc <= 0, không lưu record (hoặc xóa record cũ nếu có)
                            if (tp.PhanTram == null || tp.PhanTram <= 0)
                            {
                                // Nếu có record cũ, xóa nó (soft delete)
                                if (byChem.TryGetValue(tp.Id, out var childToDelete))
                                {
                                    childToDelete.Da_Xoa = true;
                                    _db.Set<CTP_ChiTiet_Quang_TPHH>().Update(childToDelete);
                                }
                                continue; // Bỏ qua, không tạo record mới
                            }

                            if (byChem.TryGetValue(tp.Id, out var child))
                            {
                                child.Gia_Tri_PhanTram = tp.PhanTram.Value;
                                child.Da_Xoa = false;
                                _db.Set<CTP_ChiTiet_Quang_TPHH>().Update(child);
                            }
                            else
                            {
                                tpToAdd.Add(new CTP_ChiTiet_Quang_TPHH
                                {
                                    ID_CTP_ChiTiet_Quang = ctqId,
                                    ID_TPHH = tp.Id,
                                    Gia_Tri_PhanTram = tp.PhanTram.Value,
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
                        Nguoi_Tao = dto.Nguoi_Tao,
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
                        Da_Xoa = false,
                        Ngay_Tao = dto.CongThucPhoi.Ngay_Tao ?? DateTimeOffset.Now,
                        Nguoi_Tao = dto.Nguoi_Tao
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

                                // Thêm dữ liệu mới đã chỉnh sửa - chỉ lưu các record có PhanTram > 0
                                // Nếu PhanTram = null hoặc <= 0, không tạo record
                                var newTps = new List<CTP_ChiTiet_Quang_TPHH>();
                                foreach (var tp in input.TP_HoaHocs)
                                {
                                    if (tp.PhanTram.HasValue && tp.PhanTram.Value > 0)
                                    {
                                        newTps.Add(new CTP_ChiTiet_Quang_TPHH
                                        {
                                            ID_CTP_ChiTiet_Quang = ctqId,
                                            ID_TPHH = tp.Id,
                                            Gia_Tri_PhanTram = tp.PhanTram.Value,
                                            Da_Xoa = false
                                        });
                                    }
                                }
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
                if (dto.QuangThanhPham?.Gia != null)
                {
                    var gia = dto.QuangThanhPham.Gia;
                    // Dùng đúng thời điểm do FE chọn cho tỷ giá/giá
                    var eff = gia.Ngay_Chon_TyGia;
                    await SaveOutputOrePriceAsync(quang.ID, gia, eff, ct);
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

                // Upsert bảng chi phí cho công thức (nếu FE gửi kèm)
                if (dto.BangChiPhi?.Any() == true)
                {
                    await UpsertBangChiPhiAsync(congThuc.ID, dto.BangChiPhi, ct);
                }
                else
                {
                    // Tự động tạo bảng chi phí từ ChiTietQuang và giá quặng đầu vào
                    await AutoCreateBangChiPhiAsync(congThuc.ID, dto, ct);
                }

                // Tính và lưu giá cuối cùng vào Quang_Gia_LichSu
                await CalculateAndSaveFinalPriceAsync(congThuc.ID, quang.ID, dto.CongThucPhoi.Ngay_Tao ?? DateTimeOffset.Now, ct);
                
                return isUpdate ? congThuc.ID_Quang_DauRa : quang.ID;
            }
            catch
            {
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
                    Created_By_User_ID = dto.Nguoi_Tao,
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
                congThuc.Ten_Cong_Thuc ?? string.Empty,
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
            
            // Batch load all chemistry data for input ores
            var allEditedChems = await _db.Set<CTP_ChiTiet_Quang_TPHH>().AsNoTracking()
                .Where(x => ctqs.Select(c => c.a.ID).Contains(x.ID_CTP_ChiTiet_Quang) && !x.Da_Xoa)
                .Join(_db.Set<TP_HoaHoc>().AsNoTracking(), a => a.ID_TPHH, b => b.ID, (a, b) => new { a, b })
                .OrderBy(x => x.b.Ma_TPHH)
                .Select(x => new { 
                    CTP_ChiTiet_Quang_ID = x.a.ID_CTP_ChiTiet_Quang,
                    TPHHValue = new TPHHValue(x.a.ID_TPHH, x.a.Gia_Tri_PhanTram, null)
                })
                .ToListAsync(ct);

            var allOriginalChems = await _db.Set<Quang_TP_PhanTich>().AsNoTracking()
                .Where(x => inputOreIds.Contains(x.ID_Quang) && !x.Da_Xoa)
                .OrderBy(x => x.ThuTuTPHH)
                .ThenBy(x => x.ID_TPHH)
                .Select(x => new { 
                    ID_Quang = x.ID_Quang,
                    TPHHValue = new TPHHValue(x.ID_TPHH, x.Gia_Tri_PhanTram, x.ThuTuTPHH)
                })
                .ToListAsync(ct);

            // Group chemistry data by ore ID
            var chemDict = new Dictionary<int, List<TPHHValue>>();
            foreach (var ctq in ctqs)
            {
                var editedChems = allEditedChems
                    .Where(x => x.CTP_ChiTiet_Quang_ID == ctq.a.ID)
                    .Select(x => x.TPHHValue)
                    .ToList();
                
                if (editedChems.Any())
                {
                    chemDict[ctq.q.ID] = editedChems;
                }
                else
                {
                    // Fallback về dữ liệu gốc từ Quang_TP_PhanTich
                    var originalChems = allOriginalChems
                        .Where(x => x.ID_Quang == ctq.q.ID)
                        .Select(x => x.TPHHValue)
                        .ToList();
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
                    // milestone-specific fields from CTP_ChiTiet_Quang
                    x.a.Khau_Hao,
                    x.a.Ti_Le_KhaoHao,
                    x.a.KL_VaoLo,
                    x.a.Ti_Le_HoiQuang,
                    x.a.KL_Nhan
                );
            }).ToList();


            // Sort inputs: mixed ores (Loai_Quang = 1 hoặc 7) first, then others
            chiTiet = chiTiet
                .OrderBy(c => (c.Loai_Quang == 1 || c.Loai_Quang == 7) ? 0 : 1)
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

            // Load BangChiPhi items for this formula
            var costItems = await _db.Set<CTP_BangChiPhi>()
                .AsNoTracking()
                .Where(x => x.ID_CongThucPhoi == cong.ID)
                .Join(_db.Set<Cong_Thuc_Phoi>().AsNoTracking(), 
                    bcp => bcp.ID_CongThucPhoi, 
                    ctp => ctp.ID, 
                    (bcp, ctp) => new { bcp, ctp })
                .GroupJoin(_db.Set<Quang>().AsNoTracking(),
                    x => x.bcp.ID_Quang,
                    q => q.ID,
                    (x, qs) => new { x.bcp, x.ctp, quang = qs.FirstOrDefault() })
                .Select(x => new BangChiPhiItem(
                    x.bcp.ID_CongThucPhoi,
                    x.bcp.ID_Quang,
                    x.bcp.LineType,
                    x.bcp.Tieuhao,
                    x.bcp.DonGiaVND,
                    x.bcp.DonGiaUSD,
                    x.ctp.ID_Quang_DauRa,
                    x.quang != null ? x.quang.Ten_Quang : null,
                    x.quang != null ? x.quang.Loai_Quang : null
                ))
                .ToListAsync(ct);

            // Nếu milestone = 1 (Thiêu kết), lấy thêm BangChiPhi của các quặng thành phần được phối ra quặng loại 7
            if (milestone == 1)
            {
                // Tìm các quặng loại 7 trong chiTiet
                var loai7OreIds = chiTiet
                    .Where(c => c.Loai_Quang == 7)
                    .Select(c => c.ID_Quang)
                    .ToList();

                if (loai7OreIds.Any())
                {
                    // Với mỗi quặng loại 7, tìm công thức phối của nó
                    var formulasForLoai7 = await _db.Set<Cong_Thuc_Phoi>()
                        .AsNoTracking()
                        .Where(f => loai7OreIds.Contains(f.ID_Quang_DauRa) && !f.Da_Xoa)
                        .ToListAsync(ct);

                    var formulaIdsForLoai7 = formulasForLoai7.Select(f => f.ID).ToList();

                    if (formulaIdsForLoai7.Any())
                    {
                        // Lấy BangChiPhi của các quặng thành phần trong các công thức đó
                        // Join với Cong_Thuc_Phoi để lấy ID_Quang_DauRa (quặng loại 7) và Quang để lấy tên
                        var componentCostItems = await _db.Set<CTP_BangChiPhi>()
                            .AsNoTracking()
                            .Where(x => formulaIdsForLoai7.Contains(x.ID_CongThucPhoi))
                            .Join(_db.Set<Cong_Thuc_Phoi>().AsNoTracking(), 
                                bcp => bcp.ID_CongThucPhoi, 
                                ctp => ctp.ID, 
                                (bcp, ctp) => new { bcp, ctp })
                            .GroupJoin(_db.Set<Quang>().AsNoTracking(),
                                x => x.bcp.ID_Quang,
                                q => q.ID,
                                (x, qs) => new { x.bcp, x.ctp, quang = qs.FirstOrDefault() })
                            .Select(x => new BangChiPhiItem(
                                x.bcp.ID_CongThucPhoi,
                                x.bcp.ID_Quang,
                                x.bcp.LineType,
                                x.bcp.Tieuhao,
                                x.bcp.DonGiaVND,
                                x.bcp.DonGiaUSD,
                                x.ctp.ID_Quang_DauRa, // Quặng loại 7
                                x.quang != null ? x.quang.Ten_Quang : null,
                                x.quang != null ? x.quang.Loai_Quang : (int?)null
                            ))
                            .ToListAsync(ct);

                        // Thêm vào costItems (chỉ lấy các dòng có ID_Quang, không lấy chi phí khác)
                        costItems = costItems.Concat(componentCostItems.Where(x => x.ID_Quang.HasValue)).ToList();
                    }
                }
            }

            return new CongThucPhoiDetailMinimal(
                new CongThucInfo(cong.ID, cong.Ma_Cong_Thuc, cong.Ten_Cong_Thuc, cong.Ghi_Chu),

                new QuangChem(quangOut.ID, quangOut.Ma_Quang, quangOut.Ten_Quang ?? string.Empty, outChems),
                chiTiet,

                rbs,
                milestone,
                costItems
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
                            milestoneFromMap,
                            detail.BangChiPhi
                    );
                    details.Add(detailWithCorrectMilestone);
                }
            }

            // Sort by ThuTuPhoi (thứ tự phối trong plan)
            var sortedDetails = details
                .OrderBy(d => {
                    // Tìm ThuTuPhoi từ formulas dựa trên ID công thức
                    foreach (var formula in formulas)
                    {
                        if (formula.ID == d.CongThuc.Id)
                        {
                            return formula.ThuTuPhoi;
                        }
                    }
                    return 0;
                })
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




        // ============================================================
        // CLONE OPERATIONS (Plan & Milestones)
        // ============================================================

        // Helper: Convert string to slug (lowercase, replace spaces and special chars with hyphens)
        private static string ToSlug(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove diacritics (Vietnamese accents)
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            var withoutAccents = sb.ToString();

            // Convert to lowercase and replace spaces/special chars with hyphens
            var slug = Regex.Replace(withoutAccents.ToLowerInvariant(), @"[^a-z0-9]+", "-");
            // Remove leading/trailing hyphens
            slug = slug.Trim('-');
            return slug;
        }

        // Helper: Get milestone display name
        private static string GetMilestoneName(Milestone? milestone)
        {
            return milestone switch
            {
                Milestone.Standard => "Quặng sắt",
                Milestone.ThietKet => "Thiêu kết",
                Milestone.LoCao => "Lò Cao",
                _ => "Quặng sắt"
            };
        }

        // Helper: Get milestone slug
        private static string GetMilestoneSlug(Milestone? milestone)
        {
            return milestone switch
            {
                Milestone.Standard => "quang-sat",
                Milestone.ThietKet => "thieu-ket",
                Milestone.LoCao => "lo-cao",
                _ => "quang-sat"
            };
        }

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

            // Get milestone from source link (or default to Standard)
            var srcLink = await _db.Set<PA_LuaChon_CongThuc>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID_Cong_Thuc_Phoi == sourceCongThucId && !x.Da_Xoa, ct);
            var milestone = srcLink?.Milestone.HasValue == true 
                ? (Milestone)srcLink.Milestone.Value 
                : Milestone.Standard;

            // 1) Clone output ore (Quang)
            var planInfo = await _db.Set<Phuong_An_Phoi>().Where(x => x.ID == targetPlanId)
                .Select(x => new { x.Ten_Phuong_An, x.ID })
                .FirstAsync(ct);
            
            var gangDichInfo = await _db.Set<Phuong_An_Phoi>().Where(x => x.ID == targetPlanId)
                .Join(_db.Set<Quang>(), p => p.ID_Quang_Dich, q => q.ID, (p, q) => new { q.Ma_Quang, q.Ten_Quang })
                .FirstAsync(ct);
            
            var dateStr = (cloneDate ?? DateTimeOffset.Now).ToString("yyyyMMdd");
            
            // Generate mã quặng: tên milestone dạng slug - tên phương án dạng slug - mã gang đích dạng slug - dateStr
            var milestoneSlug = GetMilestoneSlug(milestone);
            var planSlug = ToSlug(planInfo.Ten_Phuong_An);
            var gangMaSlug = ToSlug(gangDichInfo.Ma_Quang);
            var maQuang = $"quang-{milestoneSlug}-{planSlug}-{gangMaSlug}";
            
            // Generate tên quặng: Tên milestone - tên phương án - tên quặng đích
            var milestoneName = GetMilestoneName(milestone);
            var tenQuang = $"Quặng {milestoneName}";
            
            var newOut = new Quang
            {
                Ma_Quang = maQuang,
                Ten_Quang = tenQuang,
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
            // Generate mã công thức: CTP-{mã phương án}-{mã gang đích}-{ngaythangnam}
            var maCongThuc = $"CTP-{planInfo.ID}-{gangDichInfo.Ma_Quang}-{dateStr}";
            
            // Generate tên công thức: CTP - {Tên phương án} - {tên gang đích}
            var tenCongThuc = $"CTP - {planInfo.Ten_Phuong_An} - {gangDichInfo.Ten_Quang}";
            
            var newCong = new Cong_Thuc_Phoi
            {
                Ma_Cong_Thuc = maCongThuc,
                Ten_Cong_Thuc = tenCongThuc,
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
            // (srcLink already loaded earlier for milestone)
            
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
                var result = await ClonePlanCoreAsync(dto, ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// Core logic để clone plan (không tạo transaction, để có thể gọi từ trong transaction khác)
        /// </summary>
        private async Task<int> ClonePlanCoreAsync(ClonePlanRequestDto dto, CancellationToken ct = default)
        {
            var source = await _db.Set<Phuong_An_Phoi>().FirstOrDefaultAsync(x => x.ID == dto.SourcePlanId && !x.Da_Xoa, ct)
                ?? throw new InvalidOperationException($"Không tìm thấy Phương Án nguồn ID {dto.SourcePlanId}");

            // Determine gang đích: nếu có NewGangDichId thì dùng, không thì dùng của source
            var targetGangDichId = dto.NewGangDichId ?? source.ID_Quang_Dich;
            
            // Create target plan
            var target = new Phuong_An_Phoi
            {
                Ten_Phuong_An = dto.NewPlanName,
                ID_Quang_Dich = targetGangDichId,
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

            // Clone gang và xỉ kết quả từ phương án gốc (không phải từ template)
            // Mỗi phương án có bộ cấu hình riêng, độc lập với nhau
            await CloneGangAndSlagFromSourcePlanAsync(
                source.ID,
                target.ID,
                target.Ten_Phuong_An,
                targetGangDichId, // Truyền gang đích mới (hoặc cũ nếu không có)
                ct);

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

            // Clone bảng chi phí CTP_BangChiPhi
            await CloneBangChiPhiAsync(source.ID, target.ID, formulaMap, outOreMap, ct);

            return target.ID;
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

        public async Task<bool> DeletePlanAsync(int id, ICong_Thuc_PhoiRepository congThucRepo, IQuangRepository quangRepo, CancellationToken ct = default)
        {
            // Delegate to the comprehensive deletion to ensure all related data is removed consistently
            return await DeletePlanWithRelatedDataAsync(id, congThucRepo, quangRepo, ct);
        }

        private async Task UpsertBangChiPhiAsync(int idCongThucPhoi, IReadOnlyList<Application.DTOs.CTP_BangChiPhiItemDto> items, CancellationToken ct)
        {
            // Key: (ID_CongThucPhoi, LineType, ID_Quang)
            var existing = await _db.Set<Domain.Entities.CTP_BangChiPhi>()
                .Where(x => x.ID_CongThucPhoi == idCongThucPhoi)
                .ToListAsync(ct);

            var map = existing.ToDictionary(x => (x.ID_CongThucPhoi, x.LineType.ToLower(), x.ID_Quang), x => x);

            foreach (var it in items)
            {
                var key = (ID_CongThucPhoi: idCongThucPhoi, LineType: it.LineType.ToLower(), ID_Quang: it.ID_Quang);
                if (map.TryGetValue(key, out var row))
                {
                    row.Tieuhao = it.Tieuhao;
                    row.DonGiaVND = it.DonGiaVND;
                    row.DonGiaUSD = it.DonGiaUSD;
                    _db.Set<Domain.Entities.CTP_BangChiPhi>().Update(row);
                }
                else
                {
                    var add = new Domain.Entities.CTP_BangChiPhi
                    {
                        ID_CongThucPhoi = idCongThucPhoi,
                        ID_Quang = it.ID_Quang,
                        LineType = it.LineType,
                        Tieuhao = it.Tieuhao,
                        DonGiaVND = it.DonGiaVND,
                        DonGiaUSD = it.DonGiaUSD
                    };
                    await _db.Set<Domain.Entities.CTP_BangChiPhi>().AddAsync(add, ct);
                }
            }
            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Tự động tạo bảng chi phí từ ChiTietQuang và giá quặng đầu vào
        /// </summary>
        private async Task AutoCreateBangChiPhiAsync(int idCongThucPhoi, MixQuangRequestDto dto, CancellationToken ct)
        {
            if (dto.ChiTietQuang == null || !dto.ChiTietQuang.Any())
                return;

            var ngayTinh = dto.CongThucPhoi.Ngay_Tao ?? DateTimeOffset.Now;
            var items = new List<Application.DTOs.CTP_BangChiPhiItemDto>();

            // Lấy ID của MKN (Mat Khi Nung) để tính tiêu hao
            int? mknId = null;
            if (dto.Milestone.HasValue)
            {
                var mkn = await _db.Set<TP_HoaHoc>().AsNoTracking()
                    .Where(x => !x.Da_Xoa && x.Ma_TPHH == "MKN")
                    .Select(x => (int?)x.ID)
                    .FirstOrDefaultAsync(ct);
                mknId = mkn;
            }

            foreach (var input in dto.ChiTietQuang)
            {
                // Lấy giá hiện tại của quặng đầu vào
                var price = await _db.Set<Quang_Gia_LichSu>().AsNoTracking()
                    .Where(x => x.ID_Quang == input.ID_Quang && !x.Da_Xoa && x.Hieu_Luc_Tu <= ngayTinh)
                    .OrderByDescending(x => x.Hieu_Luc_Tu)
                    .FirstOrDefaultAsync(ct);

                if (price == null)
                    continue; // Bỏ qua nếu không có giá

                var donGiaVND = price.Don_Gia_VND_1Tan;
                var tyGia = price.Ty_Gia_USD_VND;
                var donGiaUSD = price.Don_Gia_USD_1Tan;

                // Tính tiêu hao dựa trên tỷ lệ phần trăm và MKN (nếu có)
                decimal tieuhao = input.Ti_Le_PhanTram;
                if (dto.Milestone.HasValue && mknId.HasValue && input.TP_HoaHocs?.Any() == true)
                {
                    var mknValue = input.TP_HoaHocs.FirstOrDefault(x => x.Id == mknId.Value);
                    if (mknValue != null && mknValue.PhanTram.HasValue)
                    {
                        var mkn = (decimal)mknValue.PhanTram.Value;
                        tieuhao = input.Ti_Le_PhanTram * (1 - (mkn / 100m));
                    }
                }

                items.Add(new Application.DTOs.CTP_BangChiPhiItemDto(
                    idCongThucPhoi,
                    input.ID_Quang,
                    "QUANG", // LineType cho quặng
                    tieuhao,
                    donGiaVND,
                    donGiaUSD
                ));
            }

            // Tạo bảng chi phí
            if (items.Any())
            {
                await UpsertBangChiPhiAsync(idCongThucPhoi, items, ct);
            }
        }

        /// <summary>
        /// Tính tổng chi phí từ giá quặng đầu vào và tỷ lệ nhập, sau đó lưu giá cuối cùng vào Quang_Gia_LichSu
        /// </summary>
        private async Task CalculateAndSaveFinalPriceAsync(int idCongThucPhoi, int idQuangDauRa, DateTimeOffset effectiveDate, CancellationToken ct)
        {
            // Lấy tất cả quặng đầu vào (ChiTietQuang) của công thức phối
            var chiTietQuang = await _db.Set<CTP_ChiTiet_Quang>()
                .AsNoTracking()
                .Where(x => x.ID_Cong_Thuc_Phoi == idCongThucPhoi && !x.Da_Xoa)
                .ToListAsync(ct);

            if (!chiTietQuang.Any())
                return;

            decimal totalVND = 0m;
            decimal totalUSD = 0m;
            decimal? tyGia = null;
            var quangIds = chiTietQuang.Select(x => x.ID_Quang_DauVao).Distinct().ToList();

            // Lấy giá của tất cả quặng đầu vào
            var prices = await _db.Set<Quang_Gia_LichSu>()
                .AsNoTracking()
                .Where(x => quangIds.Contains(x.ID_Quang) && !x.Da_Xoa && x.Hieu_Luc_Tu <= effectiveDate)
                .GroupBy(x => x.ID_Quang)
                .Select(g => g.OrderByDescending(x => x.Hieu_Luc_Tu).First())
                .ToDictionaryAsync(x => x.ID_Quang, x => x, ct);

            // Tính tổng chi phí dựa trên tỷ lệ phần trăm và giá quặng đầu vào
            foreach (var ctq in chiTietQuang)
            {
                if (!prices.TryGetValue(ctq.ID_Quang_DauVao, out var price))
                    continue; // Bỏ qua nếu không có giá

                var tiLe = ctq.Ti_Le_Phan_Tram / 100m; // Chuyển từ % sang decimal (ví dụ: 50% = 0.5)
                var donGiaVND = price.Don_Gia_VND_1Tan;
                var donGiaUSD = price.Don_Gia_USD_1Tan;

                // Chi phí = Tỷ lệ phần trăm * Giá quặng
                totalVND += tiLe * donGiaVND;
                totalUSD += tiLe * donGiaUSD;

                // Lấy tỷ giá từ giá quặng đầu vào (ưu tiên giá đầu tiên có tỷ giá hợp lệ)
                if (!tyGia.HasValue && price.Ty_Gia_USD_VND > 0)
                {
                    tyGia = price.Ty_Gia_USD_VND;
                }
            }

            // Nếu không có tỷ giá, tính từ totalVND và totalUSD
            if (!tyGia.HasValue && totalUSD > 0)
            {
                tyGia = totalVND / totalUSD;
            }

            // Nếu vẫn không có tỷ giá, mặc định là 1
            if (!tyGia.HasValue || tyGia.Value <= 0)
            {
                tyGia = 1m;
            }

            // Lưu giá cuối cùng vào Quang_Gia_LichSu
            var giaDto = new Application.DTOs.QuangGiaDto(
                Gia_USD_1Tan: totalUSD,
                Gia_VND_1Tan: totalVND,
                Ty_Gia_USD_VND: tyGia.Value,
                Ngay_Chon_TyGia: effectiveDate
            );

            await SaveOutputOrePriceAsync(idQuangDauRa, giaDto, effectiveDate, ct);
        }

        /// <summary>
        /// Clone quặng (gang hoặc xỉ) thành quặng kết quả cho phương án
        /// </summary>
        /// <param name="idQuangNguon">ID quặng nguồn (gang đích hoặc quặng kết quả hiện tại)</param>
        /// <param name="idPhuongAn">ID phương án đích</param>
        /// <param name="idGangDich">ID gang đích của phương án (để set cho gang kết quả)</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>ID quặng kết quả mới được tạo</returns>
        public async Task<int> CloneQuangKetQuaAsync(int idQuangNguon, int idPhuongAn, int? idGangDich = null, int? nguoiTao = null, CancellationToken ct = default)
        {
            // Lấy thông tin phương án để tạo tên
            var phuongAn = await _db.Phuong_An_Phoi.FindAsync(idPhuongAn);
            if (phuongAn == null) throw new ArgumentException($"Phương án {idPhuongAn} không tồn tại");

            // Lấy thông tin quặng nguồn
            var quangNguon = await _db.Quang.FindAsync(idQuangNguon);
            if (quangNguon == null) throw new ArgumentException($"Quặng nguồn {idQuangNguon} không tồn tại");

            // Tạo tên và mã cho quặng kết quả
            var tenQuangKetQua = $"{phuongAn.Ten_Phuong_An} quặng kết quả";
            var maQuangKetQua = tenQuangKetQua.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("--", "-")
                .Trim('-');

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
                Nguoi_Tao = nguoiTao,
                ID_Quang_Gang = idGangDich  // Gang kết quả link với gang đích, Xỉ giữ nguyên
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
                Gia_Tri_PhanTram = 0, // Luôn set = 0 khi clone
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
        /// Clone gang và xỉ kết quả từ template gang đích cho phương án mới
        /// </summary>
        private async Task CloneGangAndSlagFromTemplateAsync(int idGangDich, int idPhuongAn, string tenPhuongAn, int? nguoiTao, CancellationToken ct = default)
        {
            // Lấy thông tin gang đích
            var gangDich = await _db.Quang.FindAsync(idGangDich);
            if (gangDich == null) throw new ArgumentException($"Gang đích {idGangDich} không tồn tại");

            var maGangDich = gangDich.Ma_Quang ?? "";
            var tenGangDich = gangDich.Ten_Quang ?? "";

            // Normalize tên phương án cho mã (loại bỏ ký tự đặc biệt)
            var tenPhuongAnNormalized = Regex.Replace(tenPhuongAn, @"[^a-zA-Z0-9\s]", "")
                .Replace(" ", "_")
                .Replace("__", "_")
                .Trim('_');
            var maGangDichNormalized = Regex.Replace(maGangDich, @"[^a-zA-Z0-9\s]", "")
                .Replace(" ", "_")
                .Replace("__", "_")
                .Trim('_');

            // Tạo mã và tên cho gang kết quả
            var maGangKetQua = $"gang_{maGangDichNormalized}_{tenPhuongAnNormalized}".ToLowerInvariant();
            var tenGangKetQua = $"Gang - {tenGangDich} - {tenPhuongAn}";

            // Clone gang kết quả từ template gang đích
            await CloneQuangKetQuaWithCustomNameAsync(
                idGangDich, 
                idPhuongAn, 
                idGangDich, 
                maGangKetQua, 
                tenGangKetQua, 
                nguoiTao,
                ct);

            // Tìm xỉ của gang đích (nếu có)
            var slagDich = await _db.Quang
                .FirstOrDefaultAsync(x => x.ID_Quang_Gang == idGangDich && x.Loai_Quang == 4 && !x.Da_Xoa, ct);

            if (slagDich != null)
            {
                // Tạo mã và tên cho xỉ kết quả
                var maXiKetQua = $"xi_{maGangDichNormalized}_{tenPhuongAnNormalized}".ToLowerInvariant();
                var tenXiKetQua = $"xỉ - {tenGangDich} - {tenPhuongAn}";

                // Clone xỉ kết quả từ template xỉ đích
                await CloneQuangKetQuaWithCustomNameAsync(
                    slagDich.ID, 
                    idPhuongAn, 
                    idGangDich, 
                    maXiKetQua, 
                    tenXiKetQua, 
                    nguoiTao,
                    ct);
            }
        }

        /// <summary>
        /// Clone quặng kết quả với mã và tên tùy chỉnh
        /// </summary>
        private async Task<int> CloneQuangKetQuaWithCustomNameAsync(
            int idQuangNguon, 
            int idPhuongAn, 
            int? idGangDich, 
            string maQuangKetQua, 
            string tenQuangKetQua,
            int? nguoiTao = null,
            CancellationToken ct = default)
        {
            // Lấy thông tin quặng nguồn
            var quangNguon = await _db.Quang.FindAsync(idQuangNguon);
            if (quangNguon == null) throw new ArgumentException($"Quặng nguồn {idQuangNguon} không tồn tại");

            // Tạo quặng kết quả mới
            var quangKetQua = new Quang
            {
                Ma_Quang = maQuangKetQua,
                Ten_Quang = tenQuangKetQua,
                Loai_Quang = quangNguon.Loai_Quang, // Giữ nguyên loại (Gang=2, Xỉ=4)
                Dang_Hoat_Dong = true,
                Da_Xoa = false,
                Ghi_Chu = $"Quặng kết quả cho phương án {idPhuongAn}",
                Ngay_Tao = DateTime.Now,
                Nguoi_Tao = nguoiTao,
                ID_Quang_Gang = idGangDich  // Gang kết quả link với gang đích, Xỉ giữ nguyên
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
                Gia_Tri_PhanTram = 0, // Luôn set = 0 khi clone
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
        /// Clone Process Params và Thống kê từ template gang đích vào phương án mới
        /// </summary>
        private async Task CloneProcessParamsAndThongKeFromTemplateAsync(
            int idGangDich,
            int idPhuongAn,
            CancellationToken ct = default)
        {
            // Clone Process Params từ template
            var processParamTemplates = await _db.Gang_Dich_Template_Config
                .AsNoTracking()
                .Where(x => x.ID_Gang_Dich == idGangDich
                            && x.Loai_Template == 1 // ProcessParam
                            && !x.Da_Xoa)
                .OrderBy(x => x.ThuTu)
                .ToListAsync(ct);

            if (processParamTemplates.Any())
            {
                var processParamValues = processParamTemplates.Select(template => new PA_ProcessParamValue
                {
                    ID_Phuong_An = idPhuongAn,
                    ID_ProcessParam = template.ID_Reference,
                    ThuTuParam = template.ThuTu,
                    GiaTri = 0m, // Giá trị ban đầu = 0
                    Ngay_Tao = DateTime.Now,
                    Nguoi_Tao = template.Nguoi_Tao.HasValue ? template.Nguoi_Tao.Value.ToString() : null
                }).ToList();

                await _db.PA_ProcessParamValue.AddRangeAsync(processParamValues, ct);
            }

            // Clone Thống kê từ template
            var thongKeTemplates = await _db.Gang_Dich_Template_Config
                .AsNoTracking()
                .Where(x => x.ID_Gang_Dich == idGangDich
                            && x.Loai_Template == 2 // ThongKe
                            && !x.Da_Xoa)
                .OrderBy(x => x.ThuTu)
                .ToListAsync(ct);

            if (thongKeTemplates.Any())
            {
                var thongKeResults = thongKeTemplates.Select(template => new PA_ThongKe_Result
                {
                    ID_PhuongAn = idPhuongAn,
                    ID_ThongKe_Function = template.ID_Reference,
                    ThuTu = template.ThuTu,
                    GiaTri = 0m, // Giá trị ban đầu = 0
                    Ngay_Tinh = DateTime.Now,
                    Nguoi_Tinh = null
                }).ToList();

                await _db.PA_ThongKe_Result.AddRangeAsync(thongKeResults, ct);
            }

            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Clone gang và xỉ kết quả từ phương án gốc (không phải từ template)
        /// </summary>
        private async Task CloneGangAndSlagFromSourcePlanAsync(
            int sourcePlanId,
            int targetPlanId,
            string tenPhuongAn,
            int gangDichId, // Gang đích để tạo mã/tên (có thể là gang mới hoặc gang cũ)
            CancellationToken ct = default)
        {
            // Lấy gang và xỉ kết quả từ phương án gốc
            var sourceQuangKQ = await _db.PA_Quang_KQ
                .AsNoTracking()
                .Where(x => x.ID_PhuongAn == sourcePlanId)
                .Join(_db.Quang.AsNoTracking(),
                    pa => pa.ID_Quang,
                    q => q.ID,
                    (pa, q) => new { pa, q })
                .ToListAsync(ct);

            if (!sourceQuangKQ.Any()) return;

            // Lấy thông tin gang đích để tạo mã (dùng gang đích được truyền vào)
            var gangDich = await _db.Quang.FindAsync(gangDichId);
            if (gangDich == null) return;

            var maGangDich = gangDich.Ma_Quang ?? "";
            var tenGangDich = gangDich.Ten_Quang ?? "";

            // Normalize tên phương án cho mã
            var tenPhuongAnNormalized = Regex.Replace(tenPhuongAn, @"[^a-zA-Z0-9\s]", "")
                .Replace(" ", "_")
                .Replace("__", "_")
                .Trim('_');
            var maGangDichNormalized = Regex.Replace(maGangDich, @"[^a-zA-Z0-9\s]", "")
                .Replace(" ", "_")
                .Replace("__", "_")
                .Trim('_');

            foreach (var item in sourceQuangKQ)
            {
                var sourceQuang = item.q;
                var loaiQuang = item.pa.LoaiQuang; // 2 = Gang, 4 = Xỉ

                // Tạo mã và tên cho quặng kết quả mới
                string maQuangKetQua;
                string tenQuangKetQua;

                if (loaiQuang == 2) // Gang
                {
                    maQuangKetQua = $"gang_{maGangDichNormalized}_{tenPhuongAnNormalized}".ToLowerInvariant();
                    tenQuangKetQua = $"Gang - {tenGangDich} - {tenPhuongAn}";
                }
                else // Xỉ
                {
                    maQuangKetQua = $"xi_{maGangDichNormalized}_{tenPhuongAnNormalized}".ToLowerInvariant();
                    tenQuangKetQua = $"xỉ - {tenGangDich} - {tenPhuongAn}";
                }

                // Clone quặng từ phương án gốc
                await CloneQuangKetQuaWithCustomNameAsync(
                    sourceQuang.ID,
                    targetPlanId,
                    gangDichId, // Dùng gang đích được truyền vào (có thể là gang mới)
                    maQuangKetQua,
                    tenQuangKetQua,
                    null,
                    ct);
            }
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
                    CreatedAt = DateTimeOffset.Now,
                    CreatedBy = dto.Phuong_An_Phoi.CreatedBy,
                    Da_Xoa = false
                };

                _db.Phuong_An_Phoi.Add(entity);
                await _db.SaveChangesAsync(ct);

                // Clone gang và xỉ kết quả từ template gang đích với format mới
                await CloneGangAndSlagFromTemplateAsync(
                    dto.Phuong_An_Phoi.ID_Quang_Dich, 
                    entity.ID, 
                    entity.Ten_Phuong_An, 
                    dto.Phuong_An_Phoi.CreatedBy,
                    ct);

                // Clone Process Params và Thống kê từ template gang đích
                await CloneProcessParamsAndThongKeFromTemplateAsync(
                    dto.Phuong_An_Phoi.ID_Quang_Dich,
                    entity.ID,
                    ct);

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
        /// Cấp 3: Xóa Phương án và các bảng liên quan
        /// - Xóa PA_ProcessParamValue
        /// - Xóa PA_Snapshot_Gia
        /// - Xóa PA_Snapshot_TPHH
        /// - Xóa PA_ThongKe_Result
        /// - Xóa PA_LuaChon_CongThuc và từ đó lấy danh sách ID_Cong_Thuc_Phoi để gọi hàm xóa công thức phối
        /// - Xóa PA_Quang_KQ và từ đó lấy danh sách ID_Quang để gọi hàm xóa quặng
        /// </summary>
        public async Task<bool> DeletePlanWithRelatedDataAsync(int planId, ICong_Thuc_PhoiRepository congThucRepo, IQuangRepository quangRepo, CancellationToken ct = default)
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
                // 1. Xóa PA_ProcessParamValue
                var processParamValue = await _db.PA_ProcessParamValue
                    .Where(x => x.ID_Phuong_An == planId)
                    .ToListAsync(ct);
                if (processParamValue.Any())
                {
                    _db.PA_ProcessParamValue.RemoveRange(processParamValue);
                }

                // 2. Xóa PA_Snapshot_Gia
                var snapshotGiaPlan = await _db.PA_Snapshot_Gia
                    .Where(x => x.ID_Phuong_An == planId)
                    .ToListAsync(ct);
                if (snapshotGiaPlan.Any())
                {
                    _db.PA_Snapshot_Gia.RemoveRange(snapshotGiaPlan);
                }

                // 3. Xóa PA_Snapshot_TPHH
                var snapshotTPHH = await _db.PA_Snapshot_TPHH
                    .Where(x => x.ID_Phuong_An == planId)
                    .ToListAsync(ct);
                if (snapshotTPHH.Any())
                {
                    _db.PA_Snapshot_TPHH.RemoveRange(snapshotTPHH);
                }

                // 4. Xóa PA_ThongKe_Result
                var thongKeResult = await _db.PA_ThongKe_Result
                    .Where(x => x.ID_PhuongAn == planId)
                    .ToListAsync(ct);
                if (thongKeResult.Any())
                {
                    _db.PA_ThongKe_Result.RemoveRange(thongKeResult);
                }

                // 5. Xóa PA_LuaChon_CongThuc và lấy danh sách công thức để xóa
                // Lấy danh sách công thức phối và order by ID desc để xóa ngược thứ tự
                // (xóa từ công thức mới nhất về cũ nhất để tránh lỗi khi công thức A là đầu vào của công thức B)
                var congThucIds = await _db.PA_LuaChon_CongThuc
                    .Where(x => x.ID_Phuong_An == planId)
                    .Join(_db.Cong_Thuc_Phoi,
                        lc => lc.ID_Cong_Thuc_Phoi,
                        ctp => ctp.ID,
                        (lc, ctp) => ctp.ID)
                    .Distinct()
                    .OrderByDescending(id => id) // Order by desc ID để xóa ngược thứ tự
                    .ToListAsync(ct);
                
                var luaChonCongThuc = await _db.PA_LuaChon_CongThuc
                    .Where(x => x.ID_Phuong_An == planId)
                    .ToListAsync(ct);
                
                if (luaChonCongThuc.Any())
                {
                    _db.PA_LuaChon_CongThuc.RemoveRange(luaChonCongThuc);
                    await _db.SaveChangesAsync(ct);
                }

                // 6. Xóa các công thức phối (gọi hàm xóa công thức phối - Cấp 2)
                // Xóa theo thứ tự ngược lại (từ công thức mới nhất về cũ nhất)
                foreach (var congThucId in congThucIds)
                {
                    await congThucRepo.DeleteCongThucPhoiWithRelatedDataAsync(congThucId, ct);
                }

                // 7. Xóa PA_Quang_KQ và lấy danh sách quặng để xóa
                var paQuangKQ = await _db.PA_Quang_KQ
                    .Where(x => x.ID_PhuongAn == planId)
                    .ToListAsync(ct);
                
                var quangIds = new List<int>();
                if (paQuangKQ.Any())
                {
                    quangIds = paQuangKQ.Select(x => x.ID_Quang).Distinct().ToList();
                    _db.PA_Quang_KQ.RemoveRange(paQuangKQ);
                    await _db.SaveChangesAsync(ct);
                }

                // 8. Xóa các quặng kết quả (gọi hàm xóa quặng - Cấp 1)
                foreach (var quangId in quangIds)
                {
                    // Kiểm tra xem Quang này có được dùng bởi phương án khác thông qua PA_Quang_KQ không
                    var isUsedInOtherPlan = await _db.PA_Quang_KQ
                        .Where(x => x.ID_Quang == quangId && x.ID_PhuongAn != planId)
                        .AnyAsync(ct);

                    // Kiểm tra xem Quang này có được dùng như ID_Quang_DauVao trong CTP_ChiTiet_Quang không
                    // Check trong bảng CTP_ChiTiet_Quang xem ID_Quang có nằm trong list ID_Quang_DauVao của công thức phối nào không
                    var isUsedAsInputOre = await _db.CTP_ChiTiet_Quang
                        .Where(ctq => ctq.ID_Quang_DauVao == quangId  // Quặng được dùng như đầu vào
                                  && !ctq.Da_Xoa)                     // Chỉ lấy record chưa xóa
                        .Join(_db.Cong_Thuc_Phoi.Where(ctp => !ctp.Da_Xoa),  // Chỉ lấy công thức chưa xóa
                            ctq => ctq.ID_Cong_Thuc_Phoi, 
                            ctp => ctp.ID, 
                            (ctq, ctp) => ctp.ID)
                        .AnyAsync(ct);

                    // Chỉ xóa nếu quặng không được dùng bởi phương án khác và không được dùng như đầu vào
                    if (!isUsedInOtherPlan && !isUsedAsInputOre)
                    {
                        await quangRepo.DeleteQuangWithRelatedDataAsync(quangId, congThucRepo, ct);
                    }
                }

                // 9. Xóa PA_KetQua_TongHop
                var ketQuaTongHop = await _db.PA_KetQua_TongHop
                    .Where(x => x.ID_Phuong_An == planId)
                    .ToListAsync(ct);
                if (ketQuaTongHop.Any())
                {
                    _db.PA_KetQua_TongHop.RemoveRange(ketQuaTongHop);
                }

                await _db.SaveChangesAsync(ct);

                // 10. Xóa Phuong_An_Phoi
                // Reload entity để tránh tracking conflict
                var planToDelete = await _db.Phuong_An_Phoi
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ID == planId, ct);
                if (planToDelete == null)
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync(ct);
                        await transaction.DisposeAsync();
                    }
                    return false;
                }

                // Attach và remove
                _db.Phuong_An_Phoi.Attach(planToDelete);
                _db.Phuong_An_Phoi.Remove(planToDelete);
                await _db.SaveChangesAsync(ct);
                
                if (transaction != null)
                {
                    await transaction.CommitAsync(ct);
                    await transaction.DisposeAsync();
                }
                return true;
            }
            catch
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(ct);
                    await transaction.DisposeAsync();
                }
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

        /// <summary>
        /// Mix với tất cả dữ liệu liên quan trong một transaction
        /// </summary>
        public async Task<int> MixWithCompleteDataAsync(MixWithCompleteDataDto dto, CancellationToken ct = default)
        {
            using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // 1. Thực hiện MixInternalAsync cơ bản (không có transaction riêng)
                var congThucPhoiId = await MixInternalAsync(new MixQuangRequestDto(
                    dto.CongThucPhoi,
                    dto.ChiTietQuang,
                    dto.RangBuocTPHH,
                    dto.QuangThanhPham,
                    dto.Milestone,
                    dto.BangChiPhi
                ), ct);

                // 2. Lưu Process Param Values nếu có
                if (dto.ProcessParamValues?.Any() == true)
                {
                    await UpsertProcessParamValuesAsync(dto.CongThucPhoi.ID_Phuong_An, dto.ProcessParamValues, ct);
                }

                // 3. Lưu Gang/Slag data nếu có
                if (dto.GangSlagData != null)
                {
                    await UpsertGangSlagDataAsync(dto.CongThucPhoi.ID_Phuong_An, dto.GangSlagData, ct);
                }

                // 4. Lưu Thống kê Results nếu có
                if (dto.ThongKeResults?.Any() == true)
                {
                    await UpsertThongKeResultsAsync(dto.CongThucPhoi.ID_Phuong_An, dto.ThongKeResults, ct);
                }

                await transaction.CommitAsync(ct);
                return congThucPhoiId;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// Upsert Process Param Values cho plan
        /// </summary>
        private async Task UpsertProcessParamValuesAsync(int planId, IReadOnlyList<ProcessParamValueDto> values, CancellationToken ct = default)
        {
            if (!values.Any()) return;

            // Lấy tất cả PA_ProcessParamValue hiện tại của plan
            var existingValues = await _db.Set<PA_ProcessParamValue>().AsNoTracking()
                .Where(x => x.ID_Phuong_An == planId)
                .ToListAsync(ct);

            var valuesToUpsert = new List<PA_ProcessParamValue>();

            foreach (var valueDto in values)
            {
                var existing = existingValues.FirstOrDefault(x => x.ID_ProcessParam == valueDto.IdProcessParam);

                if (existing != null)
                {
                    // Update existing
                    existing.GiaTri = valueDto.GiaTri;
                    existing.ThuTuParam = valueDto.ThuTuParam ?? existing.ThuTuParam;
                    _db.Set<PA_ProcessParamValue>().Update(existing);
                }
                else
                {
                    // Create new
                    var newValue = new PA_ProcessParamValue
                    {
                        ID_Phuong_An = planId,
                        ID_ProcessParam = valueDto.IdProcessParam,
                        GiaTri = valueDto.GiaTri,
                        ThuTuParam = valueDto.ThuTuParam ?? 1,
                        Ngay_Tao = DateTime.Now,
                        Nguoi_Tao = null
                    };
                    valuesToUpsert.Add(newValue);
                }
            }

            if (valuesToUpsert.Any())
            {
                await _db.Set<PA_ProcessParamValue>().AddRangeAsync(valuesToUpsert, ct);
            }

            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Upsert Gang/Slag data cho plan
        /// </summary>
        private async Task UpsertGangSlagDataAsync(int planId, GangSlagDataDto data, CancellationToken ct = default)
        {
            if (data == null) return;

            // Lấy gang và slag quặng của plan từ bảng PA_Quang_KQ (kết quả quặng của plan)
            var gangSlagQuangIds = await GetGangSlagQuangIdsByPlanAsync(planId, ct);
            
            // Upsert Gang data
            if (data.GangData?.Any() == true && gangSlagQuangIds.gangQuangId.HasValue)
            {
                await UpsertQuangThanhPhanAsync(gangSlagQuangIds.gangQuangId.Value, data.GangData, ct);
            }

            // Upsert Slag data
            if (data.SlagData?.Any() == true && gangSlagQuangIds.slagQuangId.HasValue)
            {
                await UpsertQuangThanhPhanAsync(gangSlagQuangIds.slagQuangId.Value, data.SlagData, ct);
            }
        }

        /// <summary>
        /// Lấy ID của gang và slag quặng từ plan
        /// </summary>
        private async Task<(int? gangQuangId, int? slagQuangId)> GetGangSlagQuangIdsByPlanAsync(int planId, CancellationToken ct = default)
        {
            // Lấy gang và slag quặng từ bảng PA_Quang_KQ (kết quả quặng của plan)
            var resultQuangIds = await _db.Set<PA_Quang_KQ>().AsNoTracking()
                .Where(x => x.ID_PhuongAn == planId)
                .Join(_db.Set<Quang>(), 
                    pa => pa.ID_Quang, 
                    q => q.ID, 
                    (pa, q) => new { q.ID, q.Loai_Quang })
                .Where(x => x.Loai_Quang == 2 || x.Loai_Quang == 4) // 2 = Gang, 4 = Slag
                .ToListAsync(ct);

            var gangQuangId = resultQuangIds.FirstOrDefault(x => x.Loai_Quang == 2)?.ID;
            var slagQuangId = resultQuangIds.FirstOrDefault(x => x.Loai_Quang == 4)?.ID;

            // Nếu chưa có trong PA_Quang_KQ, tìm từ quặng đầu ra của các công thức phối trong plan
            if (gangQuangId == null && slagQuangId == null)
            {
                var outputQuangIds = await _db.Set<PA_LuaChon_CongThuc>().AsNoTracking()
                    .Where(x => x.ID_Phuong_An == planId && !x.Da_Xoa)
                    .Join(_db.Set<Cong_Thuc_Phoi>(), 
                        pa => pa.ID_Cong_Thuc_Phoi, 
                        ct => ct.ID, 
                        (pa, ct) => ct.ID_Quang_DauRa)
                    .Join(_db.Set<Quang>(), 
                        ct => ct, 
                        q => q.ID, 
                        (ct, q) => new { q.ID, q.Loai_Quang })
                    .Where(x => x.Loai_Quang == 2 || x.Loai_Quang == 4) // 2 = Gang, 4 = Slag
                    .ToListAsync(ct);

                gangQuangId = outputQuangIds.FirstOrDefault(x => x.Loai_Quang == 2)?.ID;
                slagQuangId = outputQuangIds.FirstOrDefault(x => x.Loai_Quang == 4)?.ID;
            }

            return (gangQuangId, slagQuangId);
        }

        /// <summary>
        /// Upsert thành phần hóa học cho quặng
        /// </summary>
        private async Task UpsertQuangThanhPhanAsync(int quangId, IReadOnlyList<GangSlagItemDto> items, CancellationToken ct = default)
        {
            if (!items.Any()) return;

            // Lấy thành phần hiện tại
            var existingItems = await _db.Set<Quang_TP_PhanTich>().AsNoTracking()
                .Where(x => x.ID_Quang == quangId && !x.Da_Xoa)
                .ToListAsync(ct);

            var itemsToUpsert = new List<Quang_TP_PhanTich>();

            foreach (var itemDto in items)
            {
                var existing = existingItems.FirstOrDefault(x => x.ID_TPHH == itemDto.TphhId);

                if (existing != null)
                {
                    // Update existing
                    existing.Gia_Tri_PhanTram = itemDto.Percentage;
                    existing.KhoiLuong = itemDto.Mass;
                    existing.CalcFormula = itemDto.CalcFormula;
                    existing.IsCalculated = itemDto.IsCalculated;
                    _db.Set<Quang_TP_PhanTich>().Update(existing);
                }
                else
                {
                    // Create new
                    var newItem = new Quang_TP_PhanTich
                    {
                        ID_Quang = quangId,
                        ID_TPHH = itemDto.TphhId,
                        Gia_Tri_PhanTram = itemDto.Percentage,
                        KhoiLuong = itemDto.Mass,
                        CalcFormula = itemDto.CalcFormula,
                        IsCalculated = itemDto.IsCalculated,
                        ThuTuTPHH = items.ToList().IndexOf(itemDto) + 1,
                        Hieu_Luc_Tu = DateTimeOffset.Now,
                        Da_Xoa = false
                    };
                    itemsToUpsert.Add(newItem);
                }
            }

            if (itemsToUpsert.Any())
            {
                await _db.Set<Quang_TP_PhanTich>().AddRangeAsync(itemsToUpsert, ct);
            }

            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Upsert Thống kê Results cho plan
        /// </summary>
        private async Task UpsertThongKeResultsAsync(int planId, IReadOnlyList<ThongKeResultUpsertDto> results, CancellationToken ct = default)
        {
            if (!results.Any()) return;

            // Lấy tất cả PA_ThongKe_Result hiện tại của plan
            var existingResults = await _db.Set<PA_ThongKe_Result>().AsNoTracking()
                .Where(x => x.ID_PhuongAn == planId)
                .ToListAsync(ct);

            var resultsToUpsert = new List<PA_ThongKe_Result>();

            foreach (var resultDto in results)
            {
                var existing = existingResults.FirstOrDefault(x => x.ID_ThongKe_Function == resultDto.ID_ThongKe_Function);

                if (existing != null)
                {
                    // Update existing
                    existing.GiaTri = resultDto.GiaTri;
                    existing.ThuTu = resultDto.ThuTu;
                    existing.Ngay_Tinh = DateTime.Now;
                    _db.Set<PA_ThongKe_Result>().Update(existing);
                }
                else
                {
                    // Create new
                    var newResult = new PA_ThongKe_Result
                    {
                        ID_PhuongAn = planId,
                        ID_ThongKe_Function = resultDto.ID_ThongKe_Function,
                        GiaTri = resultDto.GiaTri,
                        ThuTu = resultDto.ThuTu,
                        Ngay_Tinh = DateTime.Now,
                        Nguoi_Tinh = null
                    };
                    resultsToUpsert.Add(newResult);
                }
            }

            if (resultsToUpsert.Any())
            {
                await _db.Set<PA_ThongKe_Result>().AddRangeAsync(resultsToUpsert, ct);
            }

            await _db.SaveChangesAsync(ct);
        }

        
        public async Task<List<PlanSectionDto>> GetPlanSectionsByGangDichAsync(int gangDichId, bool includeThieuKet = true, bool includeLoCao = true, CancellationToken ct = default)
        {
            var plans = await _db.Set<Phuong_An_Phoi>().AsNoTracking()
                .Where(p => p.ID_Quang_Dich == gangDichId && !p.Da_Xoa)
                .OrderBy(p => p.Ngay_Tinh_Toan)
                .Select(p => new { p.ID, p.Ten_Phuong_An, p.Ngay_Tinh_Toan })
                .ToListAsync(ct);

            var result = new List<PlanSectionDto>(plans.Count);
            
            // Batch load tất cả dữ liệu cần thiết để tránh N+1 queries
            var planIds = plans.Select(p => p.ID).ToList();
            
            // Load tất cả PA_LuaChon_CongThuc cho tất cả plans
            var allLinks = await _db.Set<PA_LuaChon_CongThuc>().AsNoTracking()
                .Where(x => planIds.Contains(x.ID_Phuong_An) && !x.Da_Xoa)
                .ToListAsync(ct);
            
            // Load tất cả Cong_Thuc_Phoi
            var allCongThucIds = allLinks.Select(x => x.ID_Cong_Thuc_Phoi).Distinct().ToList();
            var allCongThuc = await _db.Set<Cong_Thuc_Phoi>().AsNoTracking()
                .Where(x => allCongThucIds.Contains(x.ID) && !x.Da_Xoa)
                .ToDictionaryAsync(x => x.ID, x => x, ct);
            
            // Load tất cả CTP_ChiTiet_Quang
            var allChiTietQuang = await _db.Set<CTP_ChiTiet_Quang>().AsNoTracking()
                .Where(x => allCongThucIds.Contains(x.ID_Cong_Thuc_Phoi) && !x.Da_Xoa)
                .ToListAsync(ct);
            
            // Load tất cả Quang
            var allQuangIds = allChiTietQuang.Select(x => x.ID_Quang_DauVao)
                .Concat(allCongThuc.Values.Select(x => x.ID_Quang_DauRa))
                .Distinct().ToList();
            var allQuang = await _db.Set<Quang>().AsNoTracking()
                .Where(x => allQuangIds.Contains(x.ID) && !x.Da_Xoa)
                .ToDictionaryAsync(x => x.ID, x => x, ct);
            // Load tất cả Quang_TP_PhanTich
            var allQuangDauRaIds = allCongThuc.Values.Select(x => x.ID_Quang_DauRa).Distinct().ToList();
            // var allQuangTPPhanTich = await _db.Set<Quang_TP_PhanTich>().AsNoTracking()
            //     .Where(x => allQuangDauRaIds.Contains(x.ID_Quang) && !x.Da_Xoa)
            //     .ToListAsync(ct);
            // Load tất cả PA_Quang_KQ để lấy ID_Quang của Gang và Xỉ
            var allQuangKQ = await _db.Set<PA_Quang_KQ>().AsNoTracking()
                .Where(x => planIds.Contains(x.ID_PhuongAn))
                .ToListAsync(ct);
            
            // Lấy ID_Quang của Gang (LoaiQuang = 2) và Xỉ (LoaiQuang = 4)
            var gangSlagIds = allQuangKQ
                .Where(x => x.LoaiQuang == 2 || x.LoaiQuang == 4)
                .Select(x => x.ID_Quang)
                .Distinct()
                .ToList();
            
            // Load tất cả Quang_TP_PhanTich cho Gang và Xỉ
            var allQuangTPPhanTich = new List<Quang_TP_PhanTich>();
            if (gangSlagIds.Any())
            {
                allQuangTPPhanTich = await _db.Set<Quang_TP_PhanTich>().AsNoTracking()
                    .Where(x => gangSlagIds.Contains(x.ID_Quang) && !x.Da_Xoa)
                    .ToListAsync(ct);
            }
            
            // Load tất cả TP_HoaHoc
            var allTPHHIds = allQuangTPPhanTich.Select(x => x.ID_TPHH).Distinct().ToList();
            var allTPHH = await _db.Set<TP_HoaHoc>().AsNoTracking()
                .Where(x => allTPHHIds.Contains(x.ID))
                .ToDictionaryAsync(x => x.ID, x => x, ct);
            
            // Load tất cả CTP_BangChiPhi cho tất cả các công thức
            var allBangChiPhi = new List<CTP_BangChiPhi>();
            if (allCongThucIds.Any())
            {
                allBangChiPhi = await _db.Set<CTP_BangChiPhi>().AsNoTracking()
                    .Where(x => allCongThucIds.Contains(x.ID_CongThucPhoi))
                    .ToListAsync(ct);
            }
            
            // Load tất cả PA_ThongKe_Result cho tất cả plans
            var allThongKeResults = await _db.Set<PA_ThongKe_Result>().AsNoTracking()
                .Where(x => planIds.Contains(x.ID_PhuongAn))
                .ToListAsync(ct);
            
            // Load tất cả ThongKe_Function để map code
            var allThongKeFunctionIds = allThongKeResults.Select(x => x.ID_ThongKe_Function).Distinct().ToList();
            var allThongKeFunctions = await _db.Set<ThongKe_Function>().AsNoTracking()
                .Where(x => allThongKeFunctionIds.Contains(x.ID))
                .ToDictionaryAsync(x => x.ID, x => x, ct);
            
            // Load tất cả Quang_Gia_LichSu cho các quặng đầu ra
            var allQuangGiaLichSu = await _db.Set<Quang_Gia_LichSu>().AsNoTracking()
                .Where(x => allQuangDauRaIds.Contains(x.ID_Quang))
                .ToListAsync(ct);
            
            foreach (var plan in plans)
            {
                ThieuKetSectionDto? thieuKet = null;
                LoCaoSectionDto? loCao = null;
                List<BangChiPhiLoCaoDto>? bangChiPhiLoCao = null;

                // Load Thiêu Kết section if requested
                if (includeThieuKet)
                {
                    thieuKet = await GetThieuKetSectionByPlanOptimizedAsync(plan.ID, allLinks, allCongThuc, allChiTietQuang, allQuang, allQuangTPPhanTich, allTPHH, allThongKeResults, allThongKeFunctions, allQuangGiaLichSu, ct);
                }

                // Load Lò Cao section if requested
                if (includeLoCao)
                {
                    loCao = await GetLoCaoSectionByPlanOptimizedAsync(plan.ID, allLinks, allCongThuc, allChiTietQuang, allQuang, allQuangTPPhanTich, allTPHH, allThongKeResults, allThongKeFunctions, allQuangKQ, ct);
                    bangChiPhiLoCao = GetBangChiPhiLoCaoOptimized(plan.ID, allLinks, allCongThuc, allBangChiPhi, allQuang);
                }

                result.Add(new PlanSectionDto(
                    plan.ID,
                    plan.Ten_Phuong_An,
                    plan.Ngay_Tinh_Toan,
                    thieuKet,
                    loCao,
                    bangChiPhiLoCao
                ));
            }

            return result;
        }

        /// <summary>
        /// Clone bảng chi phí CTP_BangChiPhi từ plan nguồn sang plan đích
        /// </summary>
        /// <param name="sourcePlanId">ID plan nguồn</param>
        /// <param name="targetPlanId">ID plan đích</param>
        /// <param name="formulaMap">Map công thức cũ -> công thức mới</param>
        /// <param name="outOreMap">Map quặng đầu ra cũ -> quặng đầu ra mới</param>
        /// <param name="ct">CancellationToken</param>
        private async Task CloneBangChiPhiAsync(int sourcePlanId, int targetPlanId, Dictionary<int, int> formulaMap, Dictionary<int, int> outOreMap, CancellationToken ct = default)
        {
            // Lấy tất cả công thức của plan nguồn
            var sourceFormulas = await _db.Set<PA_LuaChon_CongThuc>().AsNoTracking()
                .Where(x => x.ID_Phuong_An == sourcePlanId && !x.Da_Xoa)
                .Select(x => x.ID_Cong_Thuc_Phoi)
                .ToListAsync(ct);

            if (!sourceFormulas.Any())
                return;

            // Lấy tất cả bảng chi phí của các công thức nguồn
            var sourceBangChiPhi = await _db.Set<CTP_BangChiPhi>().AsNoTracking()
                .Where(x => sourceFormulas.Contains(x.ID_CongThucPhoi))
                .ToListAsync(ct);

            if (!sourceBangChiPhi.Any())
                return;

            // Tạo bảng chi phí mới cho plan đích
            var newBangChiPhiItems = new List<CTP_BangChiPhi>();

            foreach (var sourceItem in sourceBangChiPhi)
            {
                // Map công thức cũ sang công thức mới
                if (formulaMap.TryGetValue(sourceItem.ID_CongThucPhoi, out var newCongThucPhoiId))
                {
                    // Map quặng cũ sang quặng mới (nếu có)
                    int? newQuangId = sourceItem.ID_Quang;
                    if (sourceItem.ID_Quang.HasValue && outOreMap.TryGetValue(sourceItem.ID_Quang.Value, out var mappedQuangId))
                    {
                        newQuangId = mappedQuangId;
                    }

                    var newItem = new CTP_BangChiPhi
                    {
                        ID_CongThucPhoi = newCongThucPhoiId,
                        ID_Quang = newQuangId, // ✅ Map sang quặng mới nếu có
                        LineType = sourceItem.LineType, // Giữ nguyên LineType
                        Tieuhao = sourceItem.Tieuhao, // Giữ nguyên tiêu hao
                        DonGiaVND = sourceItem.DonGiaVND, // Giữ nguyên đơn giá VND
                        DonGiaUSD = sourceItem.DonGiaUSD // Giữ nguyên đơn giá USD
                    };

                    newBangChiPhiItems.Add(newItem);
                }
            }

            if (newBangChiPhiItems.Any())
            {
                await _db.Set<CTP_BangChiPhi>().AddRangeAsync(newBangChiPhiItems, ct);
                await _db.SaveChangesAsync(ct);
            }
        }

        // ========== OPTIMIZED METHODS FOR BATCH LOADING ==========

        private Task<ThieuKetSectionDto> GetThieuKetSectionByPlanOptimizedAsync(
            int planId, 
            List<PA_LuaChon_CongThuc> allLinks, 
            Dictionary<int, Cong_Thuc_Phoi> allCongThuc,
            List<CTP_ChiTiet_Quang> allChiTietQuang,
            Dictionary<int, Quang> allQuang,
            List<Quang_TP_PhanTich> allQuangTPPhanTich,
            Dictionary<int, TP_HoaHoc> allTPHH,
            List<PA_ThongKe_Result> allThongKeResults,
            Dictionary<int, ThongKe_Function> allThongKeFunctions,
            List<Quang_Gia_LichSu> allQuangGiaLichSu,
            CancellationToken ct = default)
        {
            // 1) Lấy liên kết công thức Thiêu Kết của plan
            var link = allLinks
                .Where(x => x.ID_Phuong_An == planId && x.Milestone == 1)
                .OrderBy(x => x.ThuTuPhoi)
                .FirstOrDefault();
            
            if (link == null || !allCongThuc.TryGetValue(link.ID_Cong_Thuc_Phoi, out var congThuc))
                return Task.FromResult(new ThieuKetSectionDto(new List<ThieuKetOreComponentDto>(), null, null, null, null, null, null));

            var outputOreId = congThuc.ID_Quang_DauRa;

            // 2) Components: đầu vào trực tiếp + mở rộng 1 cấp nếu quặng phối nội bộ
            var producingMap = allLinks
                .Where(x => x.ID_Phuong_An == planId)
                .Join(allCongThuc.Values, a => a.ID_Cong_Thuc_Phoi, b => b.ID, (a, b) => new { a, b })
                .ToDictionary(k => k.b.ID_Quang_DauRa, v => v.a.ID_Cong_Thuc_Phoi);

            var parentInputs = allChiTietQuang
                .Where(x => x.ID_Cong_Thuc_Phoi == congThuc.ID)
                .Select(x => new { x.ID_Quang_DauVao, x.Ti_Le_Phan_Tram })
                .ToList();

            var mixedFormulaIds = parentInputs
                .Where(x => producingMap.ContainsKey(x.ID_Quang_DauVao))
                .Select(x => producingMap[x.ID_Quang_DauVao])
                .Distinct().ToList();

            var mixedInputs = new List<(int ParentOreId, int OreId, decimal RatioInMixed)>();
            if (mixedFormulaIds.Count > 0)
            {
                var childs = allChiTietQuang
                    .Where(x => mixedFormulaIds.Contains(x.ID_Cong_Thuc_Phoi))
                    .Select(x => new { x.ID_Cong_Thuc_Phoi, x.ID_Quang_DauVao, x.Ti_Le_Phan_Tram })
                    .ToList();
                
                var reverseMap = allCongThuc.Values
                    .Where(x => mixedFormulaIds.Contains(x.ID))
                    .ToDictionary(k => k.ID, v => v.ID_Quang_DauRa);
                
                foreach (var c in childs)
                    if (reverseMap.TryGetValue(c.ID_Cong_Thuc_Phoi, out var parentOre))
                        mixedInputs.Add((parentOre, c.ID_Quang_DauVao, c.Ti_Le_Phan_Tram));
            }

            var resultMap = new Dictionary<int, decimal>();
            foreach (var p in parentInputs)
            {
                if (producingMap.ContainsKey(p.ID_Quang_DauVao))
                {
                    foreach (var ch in mixedInputs.Where(m => m.ParentOreId == p.ID_Quang_DauVao))
                    {
                        var contrib = (p.Ti_Le_Phan_Tram * ch.RatioInMixed) / 100m;
                        if (resultMap.ContainsKey(ch.OreId)) resultMap[ch.OreId] += contrib; else resultMap[ch.OreId] = contrib;
                    }
                }
                else
                {
                    if (resultMap.ContainsKey(p.ID_Quang_DauVao)) resultMap[p.ID_Quang_DauVao] += p.Ti_Le_Phan_Tram; else resultMap[p.ID_Quang_DauVao] = p.Ti_Le_Phan_Tram;
                }
            }

            var components = resultMap
                .Select(kv => new ThieuKetOreComponentDto(
                    kv.Key,
                    allQuang.TryGetValue(kv.Key, out var quang) ? quang.Ma_Quang ?? string.Empty : string.Empty,
                    allQuang.TryGetValue(kv.Key, out var quang2) ? quang2.Ten_Quang ?? string.Empty : string.Empty,
                    kv.Value))
                .OrderBy(x => x.TenQuang)
                .ToList();

            // 3) Summary - sử dụng dữ liệu đã load
            var khauHao = allChiTietQuang
                .Where(x => x.ID_Cong_Thuc_Phoi == congThuc.ID)
                .Join(allQuang.Values, a => a.ID_Quang_DauVao, q => q.ID, (a, q) => new { a, q })
                .Where(x => x.q.Loai_Quang == 1 || x.q.Loai_Quang == 7) // Mixed ores: loại 1 (trộn bình thường) hoặc 7 (trộn trong phương án)
                .OrderBy(x => x.a.Thu_Tu)
                .Select(x => x.a.Ti_Le_KhaoHao)
                .FirstOrDefault();

            var chems = allQuangTPPhanTich
                .Where(x => x.ID_Quang == outputOreId)
                .Join(allTPHH.Values, a => a.ID_TPHH, b => b.ID, (a, b) => new { a, b })
                .Select(x => new { code = (x.b.Ma_TPHH ?? "").ToLower(), x.a.Gia_Tri_PhanTram, x.a.ID_TPHH })
                .ToList();

            decimal? findChem(string codeLower, int? fallbackId = null)
            {
                var found = chems.FirstOrDefault(x => x.code == codeLower);
                if (found != null) return found.Gia_Tri_PhanTram;
                if (fallbackId.HasValue)
                {
                    var fallback = chems.FirstOrDefault(x => x.ID_TPHH == fallbackId.Value);
                    return fallback?.Gia_Tri_PhanTram;
                }
                return null;
            }

            // Debug: Log available chemical codes
            var availableCodes = chems.Select(x => x.code).ToList();

            // tK_R2: Tính tỷ lệ cao/sio2 từ thành phần hóa học
            var cao = findChem("cao");
            var sio2 = findChem("sio2");
            decimal? tK_R2 = (cao.HasValue && sio2.HasValue && sio2.Value != 0) 
                ? cao.Value / sio2.Value 
                : (decimal?)null;
            
            // tK_COST: Lấy từ Quang_Gia_LichSu của quặng thành phẩm (đơn giá VND mới nhất)
            var tK_COST = allQuangGiaLichSu
                .Where(x => x.ID_Quang == outputOreId)
                .Select(x => x.Don_Gia_VND_1Tan)
                .FirstOrDefault();
            
            // tK_PHAM_VI_VAO_LO: Lấy từ PA_ThongKe_Result với code "ORE_QUALITY"
            var tK_PHAM_VI_VAO_LO = allThongKeResults
                .Where(x => x.ID_PhuongAn == planId)
                .Where(x => allThongKeFunctions.TryGetValue(x.ID_ThongKe_Function, out var func) && func.Code == "ORE_QUALITY")
                .Select(x => x.GiaTri)
                .FirstOrDefault();
            
            var tK_TIEU_HAO_QTK = allThongKeResults
                .Where(x => x.ID_PhuongAn == planId)
                .Where(x => allThongKeFunctions.TryGetValue(x.ID_ThongKe_Function, out var func) && func.Code == "ORE_CONSUMPTION")
                .Select(x => x.GiaTri)
                .FirstOrDefault();

            return Task.FromResult(new ThieuKetSectionDto(
                components,
                khauHao,
                findChem("sio2"),
                findChem("tfe"),
                tK_R2,
                tK_PHAM_VI_VAO_LO,
                tK_COST
            ));
        }

        private Task<LoCaoSectionDto> GetLoCaoSectionByPlanOptimizedAsync(
            int planId,
            List<PA_LuaChon_CongThuc> allLinks,
            Dictionary<int, Cong_Thuc_Phoi> allCongThuc,
            List<CTP_ChiTiet_Quang> allChiTietQuang,
            Dictionary<int, Quang> allQuang,
            List<Quang_TP_PhanTich> allQuangTPPhanTich,
            Dictionary<int, TP_HoaHoc> allTPHH,
            List<PA_ThongKe_Result> allThongKeResults,
            Dictionary<int, ThongKe_Function> allThongKeFunctions,
            List<PA_Quang_KQ> allQuangKQ,
            CancellationToken ct = default)
        {
            // 1) Lấy liên kết công thức Lò Cao của plan
            var link = allLinks
                .Where(x => x.ID_Phuong_An == planId && x.Milestone == 2)
                .OrderBy(x => x.ThuTuPhoi)
                .FirstOrDefault();
            
            if (link == null || !allCongThuc.TryGetValue(link.ID_Cong_Thuc_Phoi, out var congThuc))
                return Task.FromResult(new LoCaoSectionDto(new List<LoCaoOreComponentDto>(), null, null, null, null, null, null, null, null, null, null, null, null, null));

            // 2) Components: chỉ lấy quặng có Loai_Quang != 3 (không phải phụ liệu)
            var inputOres = allChiTietQuang
                .Where(x => x.ID_Cong_Thuc_Phoi == congThuc.ID)
                .Join(allQuang.Values, a => a.ID_Quang_DauVao, q => q.ID, (a, q) => new { a, q })
                .Where(x => x.q.Loai_Quang != 3) // Loại trừ phụ liệu
                .Select(x => new { x.a.ID_Quang_DauVao, x.a.Ti_Le_Phan_Tram, x.q.Ma_Quang, x.q.Ten_Quang, x.q.Loai_Quang })
                .ToList();

            var components = inputOres
                .Select(x => new LoCaoOreComponentDto(
                    x.ID_Quang_DauVao,
                    x.Ma_Quang ?? string.Empty,
                    x.Ten_Quang ?? string.Empty,
                    x.Ti_Le_Phan_Tram,
                    x.Loai_Quang))
                .OrderBy(x => x.TenQuang)
                .ToList();

            // 3) Summary - lấy dữ liệu Gang và Xỉ từ PA_Quang_KQ
            // Lấy ID_Quang của Gang (LoaiQuang = 2) và Xỉ (LoaiQuang = 4) cho plan này
            var planQuangKQ = allQuangKQ.Where(x => x.ID_PhuongAn == planId).ToList();
            var gangId = planQuangKQ.FirstOrDefault(x => x.LoaiQuang == 2)?.ID_Quang;
            var slagId = planQuangKQ.FirstOrDefault(x => x.LoaiQuang == 4)?.ID_Quang;
            
            // Lấy thành phần hóa học của Gang
            var gangChemicals = allQuangTPPhanTich
                .Where(x => gangId.HasValue && x.ID_Quang == gangId.Value)
                .Join(allTPHH.Values, a => a.ID_TPHH, b => b.ID, (a, b) => new { a, b })
                .Select(x => new { code = (x.b.Ma_TPHH ?? "").ToLower(), x.a.Gia_Tri_PhanTram })
                .ToList();

            decimal? findGangChem(string codeLower)
            {
                return gangChemicals.FirstOrDefault(x => x.code == codeLower)?.Gia_Tri_PhanTram;
            }

            // Debug: Log available chemical codes
            var availableCodes = gangChemicals.Select(x => x.code).ToList();
            decimal? getStat(string code)
            {
                return allThongKeResults
                    .Where(x => x.ID_PhuongAn == planId)
                    .Where(x => allThongKeFunctions.TryGetValue(x.ID_ThongKe_Function, out var func) && func.Code == code)
                    .Select(x => (decimal?)x.GiaTri)
                    .FirstOrDefault();
            }

            // Map required keys from statistics
            var lC_COKE_10_25 = getStat("COKE_10_25");
            var lC_COKE_25_80 = getStat("COKE_25_80");
            var lC_PHAM_VI_VAO_LO = getStat("ORE_QUALITY");
            var lC_R2 = getStat("R2_BASICITY");
            var lC_SAN_LUONG_GANG = getStat("GANG_OUTPUT");
            var lC_THAN_PHUN = getStat("PULVERIZED_COAL");
            var lC_TIEU_HAO_QUANG = getStat("ORE_CONSUMPTION");
            var lC_TONG_KLK_VAO_LO = getStat("TOTAL_KLK_INTO_BF");
            var lC_TONG_NHIEU_LIEU = getStat("TOTAL_FUEL");
            var lC_TONG_ZN_VAO_LO = getStat("TOTAL_ZN_INTO_BF");
            var lC_XUAT_LUONG_XI = getStat("SLAG_OUTPUT");
            var lC_MN_TRONG_GANG = findGangChem("mn");
            var lC_TI_TRONG_GANG = findGangChem("ti");

            return Task.FromResult(new LoCaoSectionDto(
                components,
                lC_SAN_LUONG_GANG,
                lC_TIEU_HAO_QUANG,
                lC_COKE_25_80,
                lC_COKE_10_25,
                lC_THAN_PHUN,
                lC_TONG_NHIEU_LIEU,
                lC_XUAT_LUONG_XI,
                lC_R2,
                lC_TONG_KLK_VAO_LO,
                lC_TONG_ZN_VAO_LO,
                lC_PHAM_VI_VAO_LO,
                lC_TI_TRONG_GANG,
                lC_MN_TRONG_GANG
            ));
        }

        private List<BangChiPhiLoCaoDto> GetBangChiPhiLoCaoOptimized(
            int planId,
            List<PA_LuaChon_CongThuc> allLinks,
            Dictionary<int, Cong_Thuc_Phoi> allCongThuc,
            List<CTP_BangChiPhi> allBangChiPhi,
            Dictionary<int, Quang> allQuang)
        {
            // Lấy tất cả công thức phối LoCao của plan
            var loCaoFormulas = allLinks
                .Where(x => x.ID_Phuong_An == planId && x.Milestone == 2)
                .Select(x => x.ID_Cong_Thuc_Phoi)
                .ToList();

            if (!loCaoFormulas.Any())
                return new List<BangChiPhiLoCaoDto>();

            // Lấy tất cả bảng chi phí (cả quặng và chi phí khác)
            var bangChiPhiItems = allBangChiPhi
                .Where(x => loCaoFormulas.Contains(x.ID_CongThucPhoi))
                .ToList();

            var result = new List<BangChiPhiLoCaoDto>();

            // Xử lý các dòng có quặng (ID_Quang != null)
            var quangItems = bangChiPhiItems.Where(x => x.ID_Quang.HasValue).ToList();
            foreach (var item in quangItems)
            {
                if (allQuang.TryGetValue(item.ID_Quang!.Value, out var quang))
                {
                    result.Add(new BangChiPhiLoCaoDto(
                        quang.Ten_Quang ?? string.Empty,
                        item.Tieuhao,
                        item.LineType
                    ));
                }
            }

            // Xử lý các dòng chi phí khác (ID_Quang == null)
            var otherCostItems = bangChiPhiItems.Where(x => !x.ID_Quang.HasValue).ToList();
            foreach (var item in otherCostItems)
            {
                // Tạo tên hiển thị dựa trên LineType
                string tenHienThi = item.LineType switch
                {
                    "CHI_PHI_SX_GANG_LONG" => "Chi phí sản xuất gang lỏng",
                    "QUANG_HOI" => "Quặng hồi",
                    "CHI_PHI_KHAC" => "Chi phí khác",
                    _ => item.LineType
                };

                result.Add(new BangChiPhiLoCaoDto(
                    tenHienThi,
                    item.Tieuhao,
                    item.LineType
                ));
            }

            return result.OrderBy(x => x.LineType)
                        .ThenBy(x => x.TenQuang)
                        .ToList();
        }

        /// <summary>
        /// Clone gang với tất cả phương án con của nó
        /// </summary>
        /// <param name="sourceGangId">ID của gang nguồn</param>
        /// <param name="newGangId">ID của gang mới (đã được clone từ FE)</param>
        /// <param name="baseOptions">Options cho việc clone phương án (ResetRatiosToZero, CopySnapshots, etc.)</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>Số lượng phương án đã clone</returns>
        public async Task<int> CloneGangWithAllPlansAsync(
            int sourceGangId, 
            int newGangId, 
            ClonePlanRequestDto baseOptions, 
            CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // Lấy tất cả phương án của gang cũ
                var sourcePlans = await _db.Set<Phuong_An_Phoi>()
                    .AsNoTracking()
                    .Where(p => p.ID_Quang_Dich == sourceGangId && !p.Da_Xoa)
                    .OrderBy(p => p.Ngay_Tinh_Toan)
                    .ToListAsync(ct);

                if (!sourcePlans.Any())
                {
                    await tx.CommitAsync(ct);
                    return 0;
                }

                int clonedCount = 0;

                // Clone từng phương án với gang đích mới
                // Sử dụng ClonePlanCoreAsync để tránh tạo transaction mới (đã có transaction ở đây)
                foreach (var sourcePlan in sourcePlans)
                {
                    var cloneDto = baseOptions with
                    {
                        SourcePlanId = sourcePlan.ID,
                        NewPlanName = sourcePlan.Ten_Phuong_An, // Giữ nguyên tên phương án
                        NewGangDichId = newGangId // Móc vào gang đích mới
                    };

                    await ClonePlanCoreAsync(cloneDto, ct);
                    clonedCount++;
                }

                await tx.CommitAsync(ct);
                return clonedCount;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
        
    }
}