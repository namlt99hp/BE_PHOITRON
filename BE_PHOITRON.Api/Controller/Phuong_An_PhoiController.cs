using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class Phuong_An_PhoiController(IPhuong_An_PhoiService service) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<ActionResult<ApiResponse<PagedResult<Phuong_An_PhoiResponse>>>> Search(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            CancellationToken ct = default)
        {
            var (total, data) = await service.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            return Ok(ApiResponse<PagedResult<Phuong_An_PhoiResponse>>.Ok(new PagedResult<Phuong_An_PhoiResponse>(total, page, pageSize, data)));
        }

        

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> Upsert([FromBody] Phuong_An_PhoiUpsertDto dto, CancellationToken ct)
        {
            try
            {
                var id = await service.UpsertAsync(dto, ct);
                if (id > 0) return Ok(ApiResponse<object>.Ok(new { id }, "Thành công"));
                return BadRequest(ApiResponse<object>.BadRequest("Thất bại"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> Mix([FromBody] MixQuangRequestDto dto, CancellationToken ct)
        {
            try
            {
                var idQuangOut = await service.MixAsync(dto, ct);
                return Ok(ApiResponse<object>.Ok(new { idQuangOut }, "Mix thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> MixWithCompleteData([FromBody] MixWithCompleteDataDto dto, CancellationToken ct)
        {
            try
            {
                var idQuangOut = await service.MixWithCompleteDataAsync(dto, ct);
                return Ok(ApiResponse<object>.Ok(new { idQuangOut }, "Mix với dữ liệu đầy đủ thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
        }

        // Mix độc lập: không liên kết phương án. Vẫn ghi CTP_ChiTiet_Quang và CTP_ChiTiet_Quang_TPHH như luồng plan
        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> MixStandalone([FromBody] MixQuangRequestDto dto, CancellationToken ct)
        {
            try
            {
                // Đảm bảo không liên kết phương án
                dto = dto with { CongThucPhoi = dto.CongThucPhoi with { ID_Phuong_An = 0 } };
                var idQuangOut = await service.MixAsync(dto, ct);
                return Ok(ApiResponse<object>.Ok(new { idQuangOut }, "Mix standalone thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
        }

        [HttpDelete("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> SoftDelete(int id, CancellationToken ct)
        {
            var success = await service.SoftDeleteAsync(id, ct);
            return success ? Ok(ApiResponse<object>.Ok(null, "Xóa thành công")) : NotFound(ApiResponse<object>.NotFound());
        }

        [HttpDelete("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct)
        {
            var success = await service.DeleteAsync(id, ct);
            return success ? Ok(ApiResponse<object>.Ok(null, "Xóa thành công")) : NotFound(ApiResponse<object>.NotFound());
        }

        [HttpGet("[action]/quang-dich/{idQuangDich:int}")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<object>>>> GetByQuangDich(int idQuangDich, CancellationToken ct)
        {
            var data = await service.GetByQuangDichAsync(idQuangDich, ct);
            var minimal = data
                .OrderBy(x => x.Ngay_Tinh_Toan)
                .Select(x => new { id = x.ID, ten_Phuong_An = x.Ten_Phuong_An, ngay_Tinh_Toan = x.Ngay_Tinh_Toan })
                .ToList();
            return Ok(ApiResponse<IReadOnlyList<object>>.Ok(minimal));
        }

        
        [HttpGet("[action]/{congThucPhoiId:int}")]
        public async Task<ActionResult<ApiResponse<CongThucPhoiDetailResponse>>> GetCongThucPhoiDetail(int congThucPhoiId, CancellationToken ct)
        {
            var data = await service.GetCongThucPhoiDetailAsync(congThucPhoiId, ct);
            if (data == null)
                return NotFound(ApiResponse<CongThucPhoiDetailResponse>.NotFound());
            
            return Ok(ApiResponse<CongThucPhoiDetailResponse>.Ok(data));
        }

        [HttpGet("[action]/{idPhuongAn:int}")]
        public async Task<ActionResult<ApiResponse<PhuongAnWithFormulasResponse>>> GetFormulasByPlan(int idPhuongAn, CancellationToken ct)
        {
            var data = await service.GetFormulasByPlanAsync(idPhuongAn, ct);
            if (data == null)
                return NotFound(ApiResponse<PhuongAnWithFormulasResponse>.NotFound());

            return Ok(ApiResponse<PhuongAnWithFormulasResponse>.Ok(data));
        }

        [HttpGet("[action]/{idPhuongAn:int}")]
        public async Task<ActionResult<ApiResponse<PhuongAnWithMilestonesResponse>>> GetFormulasByPlanWithDetails(int idPhuongAn, CancellationToken ct)
        {
            var data = await service.GetFormulasByPlanWithDetailsAsync(idPhuongAn, ct);
            if (data == null)
                return NotFound(ApiResponse<PhuongAnWithMilestonesResponse>.NotFound());

            return Ok(ApiResponse<PhuongAnWithMilestonesResponse>.Ok(data));
        }



        [HttpGet("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<CongThucPhoiDetailMinimal>>> GetDetailMinimal(int id, CancellationToken ct)
        {
            var data = await service.GetDetailMinimalAsync(id, ct);
            if (data == null)
                return NotFound(ApiResponse<CongThucPhoiDetailMinimal>.NotFound());

            return Ok(ApiResponse<CongThucPhoiDetailMinimal>.Ok(data));
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> ClonePlan([FromBody] ClonePlanRequestDto dto, CancellationToken ct)
        {
            var id = await service.ClonePlanAsync(dto, ct);
            return Ok(ApiResponse<object>.Ok(new { id }, "Clone plan thành công"));
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> CloneMilestones([FromBody] CloneMilestonesRequestDto dto, CancellationToken ct)
        {
            var id = await service.CloneMilestonesAsync(dto, ct);
            return Ok(ApiResponse<object>.Ok(new { id }, "Clone milestones thành công"));
        }

        // Combined sections for all plans under a gang target
        [HttpGet("[action]/gang-dich/{gangDichId:int}")]
        public async Task<ActionResult<ApiResponse<List<PlanSectionDto>>>> GetPlanSectionsByGangDich(
            int gangDichId, 
            [FromQuery] bool includeThieuKet = true, 
            [FromQuery] bool includeLoCao = true, 
            CancellationToken ct = default)
        {
            var data = await service.GetPlanSectionsByGangDichAsync(gangDichId, includeThieuKet, includeLoCao, ct);
            return Ok(ApiResponse<List<PlanSectionDto>>.Ok(data));
        }

        [HttpDelete("[action]/{planId:int}")]
        public async Task<ActionResult<ApiResponse<object>>> DeletePlanWithRelatedData(int planId, CancellationToken ct)
        {
            try
            {
                var success = await service.DeletePlanWithRelatedDataAsync(planId, ct);
                if (success)
                {
                    return Ok(ApiResponse<object>.Ok(null, "Xóa phương án và tất cả dữ liệu liên quan thành công"));
                }
                else
                {
                    return NotFound(ApiResponse<object>.NotFound("Không tìm thấy phương án để xóa"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest($"Lỗi khi xóa phương án: {ex.Message}"));
            }
        }

    }
}
