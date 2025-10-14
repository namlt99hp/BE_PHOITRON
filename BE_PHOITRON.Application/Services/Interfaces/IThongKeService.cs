using BE_PHOITRON.Application.DTOs;

namespace BE_PHOITRON.Application.Services.Interfaces;

public interface IThongKeService
{
    // ThongKe_Function operations
    Task<List<ThongKeFunctionDto>> GetAllFunctionsAsync(CancellationToken ct = default);
    Task<(int total, IReadOnlyList<ThongKeFunctionDto> data)> SearchFunctionsPagedAsync(
        int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
    Task<ThongKeFunctionDto?> GetFunctionByIdAsync(int id, CancellationToken ct = default);
    Task<ThongKeFunctionDto?> GetFunctionByCodeAsync(string code, CancellationToken ct = default);
    Task<int> CreateFunctionAsync(ThongKeFunctionUpsertDto dto, CancellationToken ct = default);
    Task<bool> UpdateFunctionAsync(int id, ThongKeFunctionUpsertDto dto, CancellationToken ct = default);
    Task<int> UpsertFunctionAsync(int? id, ThongKeFunctionUpsertDto dto, CancellationToken ct = default);
    Task<bool> DeleteFunctionAsync(int id, CancellationToken ct = default);
    
    
    // PA_ThongKe_Result operations
    Task<List<ThongKeResultDto>> GetResultsByPlanIdAsync(int planId, CancellationToken ct = default);
    Task<List<ThongKeResultDto>> CalculateAndSaveAsync(int planId, CalculationContextDto context, CancellationToken ct = default);
    Task<bool> DeleteResultsByPlanIdAsync(int planId, CancellationToken ct = default);
    Task<int> UpsertResultsForPlanAsync(int planId, List<PlanResultsUpsertItemDto> items, CancellationToken ct = default);
}
