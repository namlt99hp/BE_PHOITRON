using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class CTP_RangBuoc_TPHHController(ICTP_RangBuoc_TPHHService service) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<ActionResult<ApiResponse<PagedResult<CTP_RangBuoc_TPHHResponse>>>> Search(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            CancellationToken ct = default)
        {
            var (total, data) = await service.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            return Ok(ApiResponse<PagedResult<CTP_RangBuoc_TPHHResponse>>.Ok(new PagedResult<CTP_RangBuoc_TPHHResponse>(total, page, pageSize, data)));
        }

        // [HttpPost("[action]")]
        // public async Task<IActionResult> Create([FromBody] CTP_RangBuoc_TPHHCreateDto dto, CancellationToken ct)
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
        // public async Task<IActionResult> Update([FromBody] CTP_RangBuoc_TPHHUpdateDto dto, CancellationToken ct)
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
        public async Task<ActionResult<ApiResponse<object>>> Upsert([FromBody] CTP_RangBuoc_TPHHUpsertDto dto, CancellationToken ct)
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

        // [HttpGet("[action]/cong-thuc-phoi/{idCongThucPhoi:int}")]
        // public async Task<IActionResult> GetByCongThucPhoi(int idCongThucPhoi, CancellationToken ct)
        // {
        //     var data = await service.GetByCongThucPhoiAsync(idCongThucPhoi, ct);
        //     return Ok(data);
        // }
    }
}
