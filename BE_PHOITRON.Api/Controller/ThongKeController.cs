using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_PHOITRON.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class ThongKeController(IThongKeService thongKeService) : ControllerBase
{
    [HttpGet("[action]")]
    public async Task<ActionResult<ApiResponse<PagedResult<ThongKeFunctionResponse>>>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken ct = default)
    {
        var (total, data) = await thongKeService.SearchFunctionsPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
        var responseData = data.Select(dto => new ThongKeFunctionResponse(
            dto.ID, dto.Code, dto.Ten, dto.MoTa, dto.DonVi, dto.HighlightClass, 
            dto.IsAutoCalculated, dto.IsActive
        )).ToList();
        return Ok(ApiResponse<PagedResult<ThongKeFunctionResponse>>.Ok(new PagedResult<ThongKeFunctionResponse>(total, page, pageSize, responseData)));
    }

    [HttpGet("[action]")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ThongKeFunctionResponse>>>> GetAll(CancellationToken ct = default)
    {
        var data = await thongKeService.GetAllFunctionsAsync(ct);
        var responseData = data.Select(dto => new ThongKeFunctionResponse(
            dto.ID, dto.Code, dto.Ten, dto.MoTa, dto.DonVi, dto.HighlightClass, 
            dto.IsAutoCalculated, dto.IsActive
        )).ToList();
        return Ok(ApiResponse<IReadOnlyList<ThongKeFunctionResponse>>.Ok(responseData));
    }

    [HttpGet("[action]/{id:int}")]
    public async Task<ActionResult<ApiResponse<ThongKeFunctionResponse>>> GetById(int id, CancellationToken ct = default)
    {
        var dto = await thongKeService.GetFunctionByIdAsync(id, ct);
        if (dto == null) return NotFound(ApiResponse<ThongKeFunctionResponse>.NotFound());
        
        var response = new ThongKeFunctionResponse(
            dto.ID, dto.Code, dto.Ten, dto.MoTa, dto.DonVi, dto.HighlightClass, 
            dto.IsAutoCalculated, dto.IsActive
        );
        return Ok(ApiResponse<ThongKeFunctionResponse>.Ok(response));
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<ApiResponse<object>>> Upsert([FromBody] ThongKeFunctionUpsertWithIdDto dto, CancellationToken ct = default)
    {
        try
        {
            var id = await thongKeService.UpsertFunctionAsync(dto.ID, new ThongKeFunctionUpsertDto(
                dto.Code,
                dto.Ten,
                dto.MoTa,
                dto.DonVi,
                dto.HighlightClass,
                dto.IsAutoCalculated,
                dto.IsActive
            ), ct);
            return Ok(ApiResponse<object>.Ok(new { id }, "Thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
        }
    }

    [HttpDelete("[action]/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct = default)
    {
        var success = await thongKeService.DeleteFunctionAsync(id, ct);
        return success ? Ok(ApiResponse<object>.Ok(null, "Xóa thành công")) : NotFound(ApiResponse<object>.NotFound());
    }

    [HttpGet("[action]/{planId:int}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ThongKeResultResponse>>>> GetResultsByPlanId(int planId, CancellationToken ct = default)
    {
        var data = await thongKeService.GetResultsByPlanIdAsync(planId, ct);
        var responseData = data.Select(dto => new ThongKeResultResponse(
            dto.ID_ThongKe_Function, dto.FunctionCode, dto.Ten, dto.DonVi,
            dto.GiaTri, dto.HighlightClass, dto.ThuTu, dto.MoTa, dto.IsAutoCalculated
        )).ToList();
        return Ok(ApiResponse<IReadOnlyList<ThongKeResultResponse>>.Ok(responseData));
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<ApiResponse<object>>> UpsertResults([FromBody] UpsertPlanResultsRequestDto request, CancellationToken ct = default)
    {
        try
        {
            var affected = await thongKeService.UpsertResultsForPlanAsync(request.PlanId, request.Items, ct);
            return Ok(ApiResponse<object>.Ok(new { affected }, "Lưu kết quả thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
        }
    }

    [HttpDelete("[action]/{planId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteResults(int planId, CancellationToken ct = default)
    {
        var success = await thongKeService.DeleteResultsByPlanIdAsync(planId, ct);
        return success ? Ok(ApiResponse<object>.Ok(null, "Xóa kết quả thành công")) : NotFound(ApiResponse<object>.NotFound());
    }
}


