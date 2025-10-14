using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlanningController(IPlanningService service) : ControllerBase
    {
        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<PlanValidationResult>>> Validate([FromBody] ValidatePlanRequest request, CancellationToken ct)
        {
            var result = await service.ValidatePlanAsync(request, ct);
            return Ok(ApiResponse<PlanValidationResult>.Ok(result));
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<ComputePlanResult>>> Compute([FromBody] ComputePlanRequest request, CancellationToken ct)
        {
            var result = await service.ComputePlanAsync(request, ct);
            return Ok(ApiResponse<ComputePlanResult>.Ok(result));
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<ComparePlansResult>>> Compare([FromBody] ComparePlansRequest request, CancellationToken ct)
        {
            var result = await service.ComparePlansAsync(request, ct);
            return Ok(ApiResponse<ComparePlansResult>.Ok(result));
        }
    }
}


