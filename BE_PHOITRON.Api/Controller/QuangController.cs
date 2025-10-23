using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuangController(IQuangService service, IThongKeService thongKeService) : ControllerBase
    {
        public sealed class QuangSearchRequest
        {
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public string? Search { get; set; }
            public string? SortBy { get; set; }
            public string? SortDir { get; set; }
            public int[]? LoaiQuang { get; set; }
            public bool? IsGangTarget { get; set; }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<PagedResult<QuangResponse>>>> Search(
            [FromBody] QuangSearchRequest body,
            CancellationToken ct = default)
        {
            var page = body?.Page ?? 1;
            var pageSize = body?.PageSize ?? 20;
            var search = body?.Search;
            var sortBy = body?.SortBy;
            var sortDir = body?.SortDir;
            var loaiQuang = body?.LoaiQuang;
            var isGangTarget = body?.IsGangTarget;

            var (total, data) = await service.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, loaiQuang, isGangTarget, ct);
            return Ok(ApiResponse<PagedResult<QuangResponse>>.Ok(new PagedResult<QuangResponse>(total, page, pageSize, data)));
        }

        

        [HttpGet("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<QuangResponse>>> GetById(int id, CancellationToken ct)
            => (await service.GetByIdAsync(id, ct)) is { } dto ? Ok(ApiResponse<QuangResponse>.Ok(dto)) : NotFound(ApiResponse<QuangResponse>.NotFound());

        [HttpGet("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<QuangDetailResponse>>> GetDetailById(int id, CancellationToken ct)
            => (await service.GetDetailByIdAsync(id, ct)) is { } dto ? Ok(ApiResponse<QuangDetailResponse>.Ok(dto)) : NotFound(ApiResponse<QuangDetailResponse>.NotFound());

        
        [HttpDelete("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> SoftDelete(int id, CancellationToken ct)
        {
            var success = await service.SoftDeleteAsync(id, ct);
            return success ? Ok(ApiResponse<object>.Ok(null, "Xóa thành công")) : NotFound(ApiResponse<object>.NotFound());
        }

        [HttpGet("[action]/loai/{loaiQuang:int}")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<QuangResponse>>>> GetByLoai(int loaiQuang, CancellationToken ct)
        {
            var data = await service.GetByLoaiAsync(loaiQuang, ct);
            return Ok(ApiResponse<IReadOnlyList<QuangResponse>>.Ok(data));
        }

        [HttpGet("[action]/active")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<QuangResponse>>>> GetActive(CancellationToken ct)
        {
            var data = await service.GetActiveAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<QuangResponse>>.Ok(data));
        }

        [HttpPut("[action]/{id:int}/active/{isActive:bool}")]
        public async Task<ActionResult<ApiResponse<object>>> SetActive(int id, bool isActive, CancellationToken ct)
        {
            var success = await service.SetActiveAsync(id, isActive, ct);
            return success ? Ok(ApiResponse<object>.Ok(null, "Cập nhật trạng thái thành công")) : NotFound(ApiResponse<object>.NotFound());
        }

        [HttpGet("[action]/exists/{maQuang}")]
        public async Task<ActionResult<ApiResponse<object>>> ExistsByCode(string maQuang, CancellationToken ct)
        {
            var exists = await service.ExistsByCodeAsync(maQuang, ct);
            return Ok(ApiResponse<object>.Ok(new { exists }));
        }


        // Batch load chemistry details for selected ores
        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<OreChemistryBatchItem>>>> GetOreChemistryBatch([FromBody] List<int> id_Quangs, CancellationToken ct)
        {
            if (id_Quangs == null || id_Quangs.Count == 0)
            {
                return Ok(ApiResponse<IReadOnlyList<OreChemistryBatchItem>>.Ok([]));
            }

            var results = await service.GetOreChemistryBatchAsync(id_Quangs, ct);
            return Ok(ApiResponse<IReadOnlyList<OreChemistryBatchItem>>.Ok(results));
        }

        // Batch: get formulas by output ore ids
        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<FormulaByOutputOreResponse>>>> GetFormulasByOutputOreIds([FromBody] List<int> outputOreIds, CancellationToken ct)
        {
            if (outputOreIds == null || outputOreIds.Count == 0)
                return Ok(ApiResponse<IReadOnlyList<FormulaByOutputOreResponse>>.Ok([]));

            var results = await service.GetFormulasByOutputOreIdsAsync(outputOreIds, ct);
            return Ok(ApiResponse<IReadOnlyList<FormulaByOutputOreResponse>>.Ok(results));
        }



        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> UpsertWithThanhPhan([FromBody] QuangUpsertWithThanhPhanDto dto, CancellationToken ct)
        {
            try
            {
                var id = await service.UpsertWithThanhPhanAsync(dto, ct);
                return Ok(ApiResponse<object>.Ok(new { id }, "Upsert quặng thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
        }

        [HttpGet("[action]/{gangId:int}")]
        public async Task<ActionResult<ApiResponse<object>>> GetSlagIdByGang(int gangId, CancellationToken ct)
        {
            var id = await service.GetSlagIdByGangIdAsync(gangId, ct);
            return Ok(ApiResponse<object>.Ok(new { id }));
        }

        [HttpGet("[action]/plan/{planId:int}")]
        public async Task<ActionResult<ApiResponse<object>>> GetGangAndSlagChemistryByPlan(int planId, CancellationToken ct)
        {
            try
            {
                var (gang, slag) = await service.GetGangAndSlagChemistryByPlanAsync(planId, ct);
                return Ok(ApiResponse<object>.Ok(new { gang, slag }));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
        }

        // Locao bundle: gang/slag chemistry + statistics in one call
        [HttpGet("[action]/plan/{planId:int}")]
        public async Task<ActionResult<ApiResponse<object>>> GetLoCaoBundle(int planId, CancellationToken ct)
        {
            try
            {
                var (gang, slag) = await service.GetGangAndSlagChemistryByPlanAsync(planId, ct);
                var statsDtos = await thongKeService.GetResultsByPlanIdAsync(planId, ct);
                var statistics = statsDtos.Select(dto => new ThongKeResultResponse(
                    dto.ID_ThongKe_Function, dto.FunctionCode, dto.Ten, dto.DonVi,
                    dto.GiaTri, dto.HighlightClass, dto.ThuTu, dto.MoTa, dto.IsAutoCalculated
                )).ToList();

                return Ok(ApiResponse<object>.Ok(new { gang, slag, statistics }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> UpsertKetQuaWithThanhPhan([FromBody] QuangKetQuaUpsertDto dto, CancellationToken ct)
        {
            try
            {
                var id = await service.UpsertKetQuaWithThanhPhanAsync(dto, ct);
                return Ok(ApiResponse<object>.Ok(new { id }));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(ex.Message));
            }
        }
    }
}