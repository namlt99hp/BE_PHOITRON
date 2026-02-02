using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class LoCaoProcessParamRepository : ILoCaoProcessParamRepository
    {
        private readonly AppDbContext _db;

        public LoCaoProcessParamRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<LoCao_ProcessParam>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.LoCao_ProcessParam.AsNoTracking()
                .Where(x => x.Da_Xoa == null || x.Da_Xoa == false)
                .OrderBy(x => x.ThuTu).ThenBy(x => x.Code)
                .ToListAsync(ct);
        }

        public async Task<LoCao_ProcessParam?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.LoCao_ProcessParam.AsNoTracking().FirstOrDefaultAsync(x => x.ID == id && (x.Da_Xoa == null || x.Da_Xoa == false), ct);
        }

        public async Task<object?> GetLinkedOreBasicAsync(int oreId, CancellationToken ct = default)
        {
            var ore = await _db.Quang.AsNoTracking().FirstOrDefaultAsync(x => x.ID == oreId && !x.Da_Xoa, ct);
            if (ore == null) return null;
            return new { id = ore.ID, ma = ore.Ma_Quang, ten = ore.Ten_Quang };
        }

        public async Task<LoCao_ProcessParam> AddAsync(LoCao_ProcessParam entity, CancellationToken ct = default)
        {
            _db.LoCao_ProcessParam.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(LoCao_ProcessParam entity, CancellationToken ct = default)
        {
            _db.LoCao_ProcessParam.Update(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<LoCao_ProcessParam> UpsertAsync(LoCao_ProcessParam entity, CancellationToken ct = default)
        {
            if (entity.ID == 0)
            {
                _db.LoCao_ProcessParam.Add(entity);
            }
            else
            {
                _db.LoCao_ProcessParam.Update(entity);
            }
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var item = await _db.LoCao_ProcessParam.FirstOrDefaultAsync(x => x.ID == id && (x.Da_Xoa == null || x.Da_Xoa == false), ct);
            if (item == null) return;
            item.Da_Xoa = true;
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var item = await _db.LoCao_ProcessParam.FirstOrDefaultAsync(x => x.ID == id, ct);
            if (item == null) return;

            // Kiểm tra xem LoCao_ProcessParam có đang được sử dụng trong bảng phụ không
            var usedInPAProcessParamValue = await _db.PA_ProcessParamValue
                .AnyAsync(x => x.ID_ProcessParam == id, ct);
            
            if (usedInPAProcessParamValue)
            {
                throw new InvalidOperationException("Không thể xóa tham số quy trình này. Tham số đang được sử dụng trong giá trị tham số phương án phối.");
            }

            _db.LoCao_ProcessParam.Remove(item);
            await _db.SaveChangesAsync(ct);
        }

        public async Task LinkOreAsync(int id, int? oreId, CancellationToken ct = default)
        {
            var item = await _db.LoCao_ProcessParam.FirstOrDefaultAsync(x => x.ID == id && (x.Da_Xoa == null || x.Da_Xoa == false), ct);
            if (item == null) return;

            if (oreId.HasValue)
            {
                var ore = await _db.Quang.AsNoTracking().AnyAsync(x => x.ID == oreId.Value && !x.Da_Xoa, ct);
                if (!ore) return; // or throw
            }

            item.ID_Quang_LienKet = oreId;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<(IReadOnlyList<LoCao_ProcessParam> Items, int Total)> SearchPagedAsync(
            int page, int size, string? sortBy, string? sortDir, string? search, CancellationToken ct = default)
        {
            var q = _db.LoCao_ProcessParam.AsNoTracking().Where(x => x.Da_Xoa == null || x.Da_Xoa == false);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(x => x.Code.ToLower().Contains(s) || x.Ten.ToLower().Contains(s) || x.DonVi.ToLower().Contains(s));
            }

            // Sorting
            bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            q = sortBy switch
            {
                nameof(LoCao_ProcessParam.Code) => (desc ? q.OrderByDescending(x => x.Code) : q.OrderBy(x => x.Code)),
                nameof(LoCao_ProcessParam.Ten) => (desc ? q.OrderByDescending(x => x.Ten) : q.OrderBy(x => x.Ten)),
                nameof(LoCao_ProcessParam.DonVi) => (desc ? q.OrderByDescending(x => x.DonVi) : q.OrderBy(x => x.DonVi)),
                nameof(LoCao_ProcessParam.ThuTu) => (desc ? q.OrderByDescending(x => x.ThuTu) : q.OrderBy(x => x.ThuTu)),
                _ => q.OrderBy(x => x.ThuTu).ThenBy(x => x.Code),
            };

            var total = await q.CountAsync(ct);
            var items = await q.Skip(page * size).Take(size).ToListAsync(ct);
            return (items, total);
        }

        public async Task<IReadOnlyList<BE_PHOITRON.Application.ResponsesModels.ProcessParamConfiguredResponse>> GetConfiguredByPaIdAsync(int paLuaChonCongThucId, CancellationToken ct = default)
        {
            // Get all process params that have values configured for this PA
            // Join PA_ProcessParamValue to include configured value (GiaTri) and filter by configured params
            var query = from p in _db.LoCao_ProcessParam.AsNoTracking()
                        join v in _db.PA_ProcessParamValue.AsNoTracking()
                            on p.ID equals v.ID_ProcessParam
                        where (p.Da_Xoa == null || p.Da_Xoa == false)
                              && v.ID_Phuong_An == paLuaChonCongThucId
                        orderby p.ThuTu, p.Code
                        select new BE_PHOITRON.Application.ResponsesModels.ProcessParamConfiguredResponse(
                            p.ID,
                            p.Code,
                            p.Ten,
                            p.DonVi,
                            p.ID_Quang_LienKet,
                            p.Scope,
                            p.ThuTu,
                            p.IsCalculated,
                            p.CalcFormula,
                            v.GiaTri,
                            v.ThuTuParam
                        );

            var list = await query.ToListAsync(ct);
            return list;
        }

        public async Task ConfigureProcessParamsForPlanAsync(int paLuaChonCongThucId, List<int> processParamIds, List<int> thuTuParams, CancellationToken ct = default)
        {
            // Validate that all process param IDs exist
            var validIdsList = await _db.LoCao_ProcessParam
                .Where(x => x.Da_Xoa == null || x.Da_Xoa == false)
                .Select(x => x.ID)
                .ToListAsync(ct);
            var validIds = validIdsList.ToHashSet();

            var invalidIds = processParamIds.Except(validIds).ToList();
            
            if (invalidIds.Any())
            {
                throw new ArgumentException($"Invalid process param IDs: {string.Join(", ", invalidIds)}");
            }

            // Delete existing configurations for this plan
            var existingConfigs = await _db.PA_ProcessParamValue
                .Where(x => x.ID_Phuong_An == paLuaChonCongThucId)
                .ToListAsync(ct);
            
            _db.PA_ProcessParamValue.RemoveRange(existingConfigs);

            // Add new configurations
            var newConfigs = processParamIds.Select((paramId, index) => new PA_ProcessParamValue
            {
                ID_Phuong_An = paLuaChonCongThucId,
                ID_ProcessParam = paramId,
                ThuTuParam = thuTuParams.Count > index ? thuTuParams[index] : index + 1, // Use provided order or fallback to sequential
                GiaTri = null, // Initial value
                Ngay_Tao = DateTime.Now,
                Nguoi_Tao = "System" // TODO: Get from current user context
            }).ToList();

            await _db.PA_ProcessParamValue.AddRangeAsync(newConfigs, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpsertValuesForPlanAsync(int paLuaChonCongThucId, IReadOnlyList<(int IdProcessParam, decimal GiaTri, int? ThuTuParam)> items, CancellationToken ct = default)
        {
            // Remove existing records for this plan, then insert provided items
            var existing = await _db.PA_ProcessParamValue
                .Where(x => x.ID_Phuong_An == paLuaChonCongThucId)
                .ToListAsync(ct);

            if (existing.Count > 0)
                _db.PA_ProcessParamValue.RemoveRange(existing);

            var now = DateTime.UtcNow;
            var toInsert = items.Select(x => new Domain.Entities.PA_ProcessParamValue
            {
                ID_Phuong_An = paLuaChonCongThucId,
                ID_ProcessParam = x.IdProcessParam,
                GiaTri = x.GiaTri,
                ThuTuParam = x.ThuTuParam ?? 0,
                Ngay_Tao = now,
                Nguoi_Tao = null
            }).ToList();

            if (toInsert.Count > 0)
                await _db.PA_ProcessParamValue.AddRangeAsync(toInsert, ct);

            // Persist changes
            await _db.SaveChangesAsync(ct);
        }

        // Removed GetValuesByPaIdAsync per request (use GetConfiguredByPaIdAsync including GiaTri)
    }
}


