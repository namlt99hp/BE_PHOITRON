using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.Services.Interfaces;

namespace BE_PHOITRON.Application.Services;

public class ThongKeService : IThongKeService
{
    private readonly IThongKeRepository _repository;

    public ThongKeService(IThongKeRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ThongKeFunctionDto>> GetAllFunctionsAsync(CancellationToken ct = default)
    {
        return await _repository.GetAllFunctionsAsync(ct);
    }

    public async Task<(int total, IReadOnlyList<ThongKeFunctionDto> data)> SearchFunctionsPagedAsync(
        int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
    {
        return await _repository.SearchFunctionsPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
    }

    public async Task<ThongKeFunctionDto?> GetFunctionByIdAsync(int id, CancellationToken ct = default)
    {
        return await _repository.GetFunctionByIdAsync(id, ct);
    }

    public async Task<ThongKeFunctionDto?> GetFunctionByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _repository.GetFunctionByCodeAsync(code, ct);
    }

    public async Task<int> CreateFunctionAsync(ThongKeFunctionUpsertDto dto, CancellationToken ct = default)
    {
        return await _repository.CreateFunctionAsync(dto, ct);
    }

    public async Task<bool> UpdateFunctionAsync(int id, ThongKeFunctionUpsertDto dto, CancellationToken ct = default)
    {
        await _repository.UpdateFunctionAsync(id, dto, ct);
        return true;
    }

    public async Task<int> UpsertFunctionAsync(int? id, ThongKeFunctionUpsertDto dto, CancellationToken ct = default)
    {
        return await _repository.UpsertFunctionAsync(id, dto, ct);
    }

    public async Task<bool> DeleteFunctionAsync(int id, CancellationToken ct = default)
    {
        return await _repository.DeleteFunctionAsync(id, ct);
    }

    public async Task<List<ThongKeResultDto>> GetResultsByPlanIdAsync(int planId, CancellationToken ct = default)
    {
        return await _repository.GetResultsByPlanIdAsync(planId, ct);
    }

    public async Task<int> UpsertResultsForPlanAsync(int planId, List<PlanResultsUpsertItemDto> items, CancellationToken ct = default)
    {
        return await _repository.UpsertResultsForPlanAsync(planId, items, ct);
    }

    public async Task<bool> DeleteResultsByPlanIdAsync(int planId, CancellationToken ct = default)
    {
        return await _repository.DeleteResultsByPlanIdAsync(planId, ct);
    }

    // public async Task<List<ThongKeResultDto>> CalculateAndSaveAsync(int planId, CalculationContextDto context, CancellationToken ct = default)
    // {
    //     var functions = await _repository.GetAllFunctionsAsync(ct);
    //     var now = DateTime.Now;
    //     var results = new List<PA_ThongKe_ResultDto>();
    //     int thuTu = 0;
    //     foreach (var func in functions)
    //     {
    //         var giaTri = DeriveGiaTriFromContext(func.Code, context);
    //         results.Add(new PA_ThongKe_ResultDto(
    //             planId,
    //             func.ID,
    //             giaTri,
    //             now,
    //             "System",
    //             thuTu++
    //         ));
    //     }
    //     await _repository.SaveResultsAsync(planId, results, ct);
    //     return await _repository.GetResultsByPlanIdAsync(planId, ct);
    // }

    // private static decimal DeriveGiaTriFromContext(string code, CalculationContextDto context)
    // {
    //     return code switch
    //     {
    //         "GANG_OUTPUT" => context.GangData?.Sum(x => x.Mass) ?? 0,
    //         "SLAG_OUTPUT" => context.XaData?.Sum(x => x.Mass) ?? 0,
    //         "ORE_CONSUMPTION" => context.MixData?.Sum(x => x.KlVaoLoResult) ?? 0,
    //         _ => 0
    //     };
    // }
}
