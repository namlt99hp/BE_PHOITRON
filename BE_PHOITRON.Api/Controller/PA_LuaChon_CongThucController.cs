using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PA_LuaChon_CongThucController(IPA_LuaChon_CongThucService service) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<ActionResult<ApiResponse<PagedResult<PA_LuaChon_CongThucResponse>>>> Search(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            CancellationToken ct = default)
        {
            var (total, data) = await service.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            return Ok(ApiResponse<PagedResult<PA_LuaChon_CongThucResponse>>.Ok(new PagedResult<PA_LuaChon_CongThucResponse>(total, page, pageSize, data)));
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<ApiResponse<PagedResult<PA_LuaChon_CongThucResponse>>>> SearchAdvanced(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? idPhuongAn = null,
            [FromQuery] int? idQuangDauRa = null,
            [FromQuery] int? idCongThucPhoi = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            CancellationToken ct = default)
        {
            var (total, data) = await service.SearchPagedAdvancedAsync(page, pageSize, idPhuongAn, idQuangDauRa, idCongThucPhoi, search, sortBy, sortDir, ct);
            return Ok(ApiResponse<PagedResult<PA_LuaChon_CongThucResponse>>.Ok(new PagedResult<PA_LuaChon_CongThucResponse>(total, page, pageSize, data)));
        }

        // [HttpPost("[action]")]
        // public async Task<IActionResult> Create([FromBody] PA_LuaChon_CongThucCreateDto dto, CancellationToken ct)
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
        // public async Task<IActionResult> Update([FromBody] PA_LuaChon_CongThucUpdateDto dto, CancellationToken ct)
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
        public async Task<ActionResult<ApiResponse<object>>> Upsert([FromBody] PA_LuaChon_CongThucUpsertDto dto, CancellationToken ct)
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

        // [HttpDelete("[action]/{id:int}")]
        // public async Task<IActionResult> SoftDelete(int id, CancellationToken ct)
        // {
        //     var success = await service.SoftDeleteAsync(id, ct);
        //     return success ? Ok(new { message = "Xóa thành công" }) : NotFound();
        // }

        // [HttpGet("[action]/phuong-an/{idPhuongAn:int}")]
        // public async Task<IActionResult> GetByPhuongAn(int idPhuongAn, CancellationToken ct)
        // {
        //     var data = await service.GetByPhuongAnAsync(idPhuongAn, ct);
        //     return Ok(data);
        // }

        // [HttpGet("[action]/{idPhuongAn:int}/validate-circular")]
        // public async Task<IActionResult> ValidateNoCircularDependency(int idPhuongAn, CancellationToken ct)
        // {
        //     var isValid = await service.ValidateNoCircularDependencyAsync(idPhuongAn, ct);
        //     return Ok(new { isValid, message = isValid ? "Không có vòng lặp" : "Có vòng lặp trong lựa chọn công thức" });
        // }
    }
}
