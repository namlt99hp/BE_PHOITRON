using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class Quang_Gia_LichSuController(IQuang_Gia_LichSuService service) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<ActionResult<ApiResponse<PagedResult<Quang_Gia_LichSuResponse>>>> Search(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            CancellationToken ct = default)
        {
            var (total, data) = await service.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            return Ok(ApiResponse<PagedResult<Quang_Gia_LichSuResponse>>.Ok(new PagedResult<Quang_Gia_LichSuResponse>(total, page, pageSize, data)));
        }

        // [HttpPost("[action]")]
        // public async Task<IActionResult> Create([FromBody] Quang_Gia_LichSuCreateDto dto, CancellationToken ct)
        // {
        //     try
        //     {
        //         var id = await service.CreateAsync(dto, ct);
        //         return CreatedAtAction(nameof(GetById), new { id }, new { id });
        //     }
        //     catch (InvalidOperationException ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // [HttpGet("[action]/{id:int}")]
        // public async Task<IActionResult> GetById(int id, CancellationToken ct)
        //     => (await service.GetByIdAsync(id, ct)) is { } dto ? Ok(dto) : NotFound();

        // [HttpPut("[action]")]
        // public async Task<IActionResult> Update([FromBody] Quang_Gia_LichSuUpdateDto dto, CancellationToken ct)
        // {
        //     try
        //     {
        //         var success = await service.UpdateAsync(dto, ct);
        //         return success ? Ok(new { message = "Cập nhật thành công" }) : NotFound();
        //     }
        //     catch (InvalidOperationException ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> Upsert([FromBody] Quang_Gia_LichSuUpsertDto dto, CancellationToken ct)
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

        [HttpDelete("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> SoftDelete(int id, CancellationToken ct)
        {
            var success = await service.SoftDeleteAsync(id, ct);
            return success ? Ok(ApiResponse<object>.Ok(null, "Xóa thành công")) : NotFound(ApiResponse<object>.NotFound());
        }

        // [HttpGet("[action]/quang/{idQuang:int}/date/{ngayTinh:datetime}")]
        // public async Task<IActionResult> GetByQuangAndDate(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct)
        // {
        //     var data = await service.GetByQuangAndDateAsync(idQuang, ngayTinh, ct);
        //     return Ok(data);
        // }

        // [HttpGet("[action]/quang/{idQuang:int}")]
        // public async Task<IActionResult> GetByQuang(int idQuang, CancellationToken ct)
        // {
        //     var data = await service.GetByQuangAsync(idQuang, ct);
        //     return Ok(data);
        // }

        // [HttpGet("[action]/quang/{idQuang:int}/gia/{ngayTinh:datetime}")]
        // public async Task<IActionResult> GetGiaByQuangAndDate(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct)
        // {
        //     var gia = await service.GetGiaByQuangAndDateAsync(idQuang, ngayTinh, ct);
        //     return Ok(new { gia });
        // }

        // [HttpGet("[action]/overlap-check")]
        // public async Task<IActionResult> HasOverlappingPeriod(
        //     [FromQuery] int idQuang,
        //     [FromQuery] DateTimeOffset hieuLucTu,
        //     [FromQuery] DateTimeOffset? hieuLucDen = null,
        //     [FromQuery] int? excludeId = null,
        //     CancellationToken ct = default)
        // {
        //     var hasOverlap = await service.HasOverlappingPeriodAsync(idQuang, hieuLucTu, hieuLucDen, excludeId, ct);
        //     return Ok(new { hasOverlap });
        // }
    }
}
