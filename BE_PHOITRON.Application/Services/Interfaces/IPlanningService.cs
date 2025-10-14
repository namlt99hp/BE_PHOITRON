using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface IPlanningService
    {
        Task<PlanValidationResult> ValidatePlanAsync(ValidatePlanRequest request, CancellationToken ct = default);
        Task<ComputePlanResult> ComputePlanAsync(ComputePlanRequest request, CancellationToken ct = default);
        Task<ComparePlansResult> ComparePlansAsync(ComparePlansRequest request, CancellationToken ct = default);
    }
}


