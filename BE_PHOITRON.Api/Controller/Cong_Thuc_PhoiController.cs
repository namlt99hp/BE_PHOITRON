using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class Cong_Thuc_PhoiController(ICong_Thuc_PhoiService service) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<ActionResult<ApiResponse<PagedResult<Cong_Thuc_PhoiResponse>>>> Search(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            CancellationToken ct = default)
        {
            var (total, data) = await service.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            return Ok(ApiResponse<PagedResult<Cong_Thuc_PhoiResponse>>.Ok(new PagedResult<Cong_Thuc_PhoiResponse>(total, page, pageSize, data)));
        }

        // [HttpPost("[action]")]
        // public async Task<IActionResult> Create([FromBody] Cong_Thuc_PhoiCreateDto dto, CancellationToken ct)
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
        // public async Task<IActionResult> Update([FromBody] Cong_Thuc_PhoiUpdateDto dto, CancellationToken ct)
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
        public async Task<ActionResult<ApiResponse<object>>> Upsert([FromBody] Cong_Thuc_PhoiUpsertDto dto, CancellationToken ct)
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

        [HttpGet("[action]/quang-daura/{idQuangDauRa:int}")]
        public async Task<IActionResult> GetByQuangDauRa(int idQuangDauRa, CancellationToken ct)
        {
            var data = await service.GetByQuangDauRaAsync(idQuangDauRa, ct);
            return Ok(data);
        }

        [HttpGet("[action]/active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var data = await service.GetActiveAsync(ct);
            return Ok(data);
        }

        // [HttpGet("[action]/exists/{maCongThuc}")]
        // public async Task<IActionResult> ExistsByCode(string maCongThuc, CancellationToken ct)
        // {
        //     var exists = await service.ExistsByCodeAsync(maCongThuc, ct);
        //     return Ok(new { exists });
        // }

        // [HttpGet("[action]/{id:int}/validate-percentage")]
        // public async Task<IActionResult> ValidateTotalPercentage(int id, CancellationToken ct)
        // {
        //     var isValid = await service.ValidateTotalPercentageAsync(id, ct);
        //     return Ok(new { isValid, message = isValid ? "Tổng phần trăm hợp lệ" : "Tổng phần trăm không hợp lệ" });
        // }
    }
}
