using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class Quang_TP_PhanTichController(IQuang_TP_PhanTichService service) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<ActionResult<PagedResult<Quang_TP_PhanTichResponse>>> Search(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            CancellationToken ct = default)
        {
            var (total, data) = await service.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            return Ok(new PagedResult<Quang_TP_PhanTichResponse>(total, page, pageSize, data));
        }

        // [HttpPost("[action]")]
        // public async Task<IActionResult> Create([FromBody] Quang_TP_PhanTichCreateDto dto, CancellationToken ct)
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
        // public async Task<IActionResult> Update([FromBody] Quang_TP_PhanTichUpdateDto dto, CancellationToken ct)
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
        public async Task<IActionResult> Upsert([FromBody] Quang_TP_PhanTichUpsertDto dto, CancellationToken ct)
        {
            try
            {
                var id = await service.UpsertAsync(dto, ct);
                return Ok(new { id, message = id > 0 ? "Thành công" : "Thất bại" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // [HttpDelete("[action]/{id:int}")]
        // public async Task<IActionResult> SoftDelete(int id, CancellationToken ct)
        // {
        //     var success = await service.SoftDeleteAsync(id, ct);
        //     return success ? Ok(new { message = "Xóa thành công" }) : NotFound();
        // }

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

        // [HttpGet("[action]/overlap-check")]
        // public async Task<IActionResult> HasOverlappingPeriod(
        //     [FromQuery] int idQuang,
        //     [FromQuery] int idTPHH,
        //     [FromQuery] DateTimeOffset hieuLucTu,
        //     [FromQuery] DateTimeOffset? hieuLucDen = null,
        //     [FromQuery] int? excludeId = null,
        //     CancellationToken ct = default)
        // {
        //     var hasOverlap = await service.HasOverlappingPeriodAsync(idQuang, idTPHH, hieuLucTu, hieuLucDen, excludeId, ct);
        //     return Ok(new { hasOverlap });
        // }

        [HttpPost("[action]/{quangId:int}")]
        public async Task<ActionResult<ApiResponse<Dictionary<int, decimal>>>> CalculateFormulas(int quangId, CancellationToken ct)
        {
            try
            {
                var results = await service.CalculateTPHHFormulasAsync(quangId, ct);
                return Ok(ApiResponse<Dictionary<int, decimal>>.Ok(results, "Tính toán công thức thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<int, decimal>>.BadRequest(ex.Message));
            }
        }
    }
}
