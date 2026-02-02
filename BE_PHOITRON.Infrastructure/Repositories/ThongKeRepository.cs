using System;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BE_PHOITRON.Infrastructure.Repositories;

public class ThongKeRepository : IThongKeRepository
{
    private readonly AppDbContext _db;

    public ThongKeRepository(AppDbContext db)
    {
        _db = db;
    }

    // ThongKe_Function operations
    public async Task<List<ThongKeFunctionDto>> GetAllFunctionsAsync(CancellationToken ct = default)
    {
        return await _db.Set<ThongKe_Function>()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new ThongKeFunctionDto(
                x.ID,
                x.Code,
                x.Ten,
                x.MoTa,
                x.DonVi,
                x.HighlightClass,
                x.IsAutoCalculated,
                x.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<(int total, IReadOnlyList<ThongKeFunctionDto> data)> SearchFunctionsPagedAsync(
        int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
    {
        var query = _db.Set<ThongKe_Function>()
            .Where(x => x.IsActive);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(x => 
                x.Code.ToLower().Contains(searchLower) ||
                x.Ten.ToLower().Contains(searchLower) ||
                (x.MoTa != null && x.MoTa.ToLower().Contains(searchLower)));
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "code" => sortDir?.ToLower() == "desc" ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "ten" => sortDir?.ToLower() == "desc" ? query.OrderByDescending(x => x.Ten) : query.OrderBy(x => x.Ten),
            "donvi" => sortDir?.ToLower() == "desc" ? query.OrderByDescending(x => x.DonVi) : query.OrderBy(x => x.DonVi),
            "isactive" => sortDir?.ToLower() == "desc" ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => query.OrderBy(x => x.Code)
        };

        // Get total count
        var total = await query.CountAsync(ct);

        // Apply pagination
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ThongKeFunctionDto(
                x.ID,
                x.Code,
                x.Ten,
                x.MoTa,
                x.DonVi,
                x.HighlightClass,
                x.IsAutoCalculated,
                x.IsActive
            ))
            .ToListAsync(ct);

        return (total, data);
    }

    public async Task<ThongKeFunctionDto?> GetFunctionByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Set<ThongKe_Function>()
            .FirstOrDefaultAsync(x => x.ID == id && x.IsActive, ct);

        return entity == null ? null : new ThongKeFunctionDto(
            entity.ID,
            entity.Code,
            entity.Ten,
            entity.MoTa,
            entity.DonVi,
            entity.HighlightClass,
            entity.IsAutoCalculated,
            entity.IsActive
        );
    }

    public async Task<ThongKeFunctionDto?> GetFunctionByCodeAsync(string code, CancellationToken ct = default)
    {
        var entity = await _db.Set<ThongKe_Function>()
            .FirstOrDefaultAsync(x => x.Code == code && x.IsActive, ct);

        return entity == null ? null : new ThongKeFunctionDto(
            entity.ID,
            entity.Code,
            entity.Ten,
            entity.MoTa,
            entity.DonVi,
            entity.HighlightClass,
            entity.IsAutoCalculated,
            entity.IsActive
        );
    }

    public async Task<int> CreateFunctionAsync(ThongKeFunctionUpsertDto dto, CancellationToken ct = default)
    {
        var entity = new ThongKe_Function
        {
            Code = dto.Code,
            Ten = dto.Ten,
            MoTa = dto.MoTa,
            DonVi = dto.DonVi,
            HighlightClass = dto.HighlightClass,
            IsAutoCalculated = dto.IsAutoCalculated,
            IsActive = dto.IsActive,
            Ngay_Tao = DateTime.Now,
            Nguoi_Tao = dto.Nguoi_Tao
        };

        _db.Set<ThongKe_Function>().Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.ID;
    }

    public async Task<bool> UpdateFunctionAsync(int id, ThongKeFunctionUpsertDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Set<ThongKe_Function>()
            .FirstOrDefaultAsync(x => x.ID == id, ct);

        if (entity == null) return false;

        entity.Code = dto.Code;
        entity.Ten = dto.Ten;
        entity.MoTa = dto.MoTa;
        entity.DonVi = dto.DonVi;
        entity.HighlightClass = dto.HighlightClass;
        entity.IsAutoCalculated = dto.IsAutoCalculated;
        entity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<int> UpsertFunctionAsync(int? id, ThongKeFunctionUpsertDto dto, CancellationToken ct = default)
    {
        if (id.HasValue && id.Value > 0)
        {
            var updated = await UpdateFunctionAsync(id.Value, dto, ct);
            return updated ? id.Value : 0;
        }

        return await CreateFunctionAsync(dto, ct);
    }

    public async Task<bool> DeleteFunctionAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Set<ThongKe_Function>()
            .FirstOrDefaultAsync(x => x.ID == id, ct);

        if (entity == null) return false;

        // Kiểm tra xem ThongKe_Function có đang được sử dụng trong bảng phụ không
        var usedInPAThongKeResult = await _db.PA_ThongKe_Result
            .AnyAsync(x => x.ID_ThongKe_Function == id, ct);
        
        if (usedInPAThongKeResult)
        {
            throw new InvalidOperationException("Không thể xóa hàm thống kê này. Hàm đang được sử dụng trong kết quả thống kê phương án phối.");
        }

        entity.IsActive = false; // Soft delete
        await _db.SaveChangesAsync(ct);
        return true;
    }

    

    // PA_ThongKe_Result operations
    public async Task<List<ThongKeResultDto>> GetResultsByPlanIdAsync(int planId, CancellationToken ct = default)
    {
        // Get results for the plan
        var results = await _db.Set<PA_ThongKe_Result>()
            .Where(x => x.ID_PhuongAn == planId)
            .ToListAsync(ct);

        // Get function details for those results
        var functionIds = results.Select(r => r.ID_ThongKe_Function).Distinct().ToList();
        var functions = await _db.Set<ThongKe_Function>()
            .Where(f => functionIds.Contains(f.ID) && f.IsActive)
            .ToListAsync(ct);

        // Join results with function definitions
        return results.Join(functions,
                r => r.ID_ThongKe_Function,
                f => f.ID,
                (r, f) => new ThongKeResultDto(
                    r.ID_ThongKe_Function,
                    f.Code,
                    f.Ten,
                    f.MoTa,
                    f.DonVi,
                    r.GiaTri,
                    f.HighlightClass,
                    r.ThuTu,
                    f.IsAutoCalculated
                ))
            .OrderBy(x => x.ThuTu ?? int.MaxValue)
            .ThenBy(x => x.FunctionCode)
            .ToList();
    }

    public async Task<int> SaveResultsAsync(int planId, List<PA_ThongKe_ResultDto> results, CancellationToken ct = default)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // Xóa kết quả cũ
            var existingResults = await _db.Set<PA_ThongKe_Result>()
                .Where(x => x.ID_PhuongAn == planId)
                .ToListAsync(ct);
            
            _db.Set<PA_ThongKe_Result>().RemoveRange(existingResults);
            await _db.SaveChangesAsync(ct);

            // Thêm kết quả mới
            var newResults = results.Select(x => new PA_ThongKe_Result
            {
                ID_PhuongAn = planId,
                ID_ThongKe_Function = x.ID_ThongKe_Function,
                GiaTri = x.GiaTri,
                Ngay_Tinh = DateTime.Now,
                Nguoi_Tinh = "System",
                ThuTu = x.ThuTu
            }).ToList();

            await _db.Set<PA_ThongKe_Result>().AddRangeAsync(newResults, ct);
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return newResults.Count;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<bool> DeleteResultsByPlanIdAsync(int planId, CancellationToken ct = default)
    {
        var results = await _db.Set<PA_ThongKe_Result>()
            .Where(x => x.ID_PhuongAn == planId)
            .ToListAsync(ct);

        _db.Set<PA_ThongKe_Result>().RemoveRange(results);
        var result = await _db.SaveChangesAsync(ct);
        return result > 0;
    }

    public async Task<int> UpsertResultsForPlanAsync(int planId, List<PlanResultsUpsertItemDto> items, CancellationToken ct = default)
    {
        // Load existing by plan
        var existing = await _db.Set<PA_ThongKe_Result>()
            .Where(x => x.ID_PhuongAn == planId)
            .ToListAsync(ct);

        var existingLookup = existing.ToDictionary(x => x.ID_ThongKe_Function, x => x);
        var incomingIds = items.Select(i => i.ID_ThongKe_Function).ToHashSet();

        // Update or add
        var toAdd = new List<PA_ThongKe_Result>();
        foreach (var item in items)
        {
            if (existingLookup.TryGetValue(item.ID_ThongKe_Function, out var row))
            {
                if (item.GiaTri.HasValue)
                {
                    row.GiaTri = item.GiaTri.Value;
                }
                row.Ngay_Tinh = DateTime.Now;
                row.Nguoi_Tinh = "System";
                if (item.ThuTu.HasValue)
                {
                    row.ThuTu = item.ThuTu.Value;
                }
            }
            else
            {
                toAdd.Add(new PA_ThongKe_Result
                {
                    ID_PhuongAn = planId,
                    ID_ThongKe_Function = item.ID_ThongKe_Function,
                    GiaTri = item.GiaTri ?? 0,
                    Ngay_Tinh = DateTime.Now,
                    Nguoi_Tinh = "System",
                    ThuTu = item.ThuTu
                });
            }
        }

        // Remove excess
        var toRemove = existing.Where(x => !incomingIds.Contains(x.ID_ThongKe_Function)).ToList();
        if (toRemove.Count > 0)
        {
            _db.Set<PA_ThongKe_Result>().RemoveRange(toRemove);
        }
        if (toAdd.Count > 0)
        {
            await _db.Set<PA_ThongKe_Result>().AddRangeAsync(toAdd, ct);
        }

        return await _db.SaveChangesAsync(ct);
    }
}
