using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Infrastructure.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class TP_HoaHocController(ITP_HoaHocService service) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<ActionResult<ApiResponse<PagedResult<TP_HoaHocResponse>>>> Search(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            CancellationToken ct = default)
        {
            var (total, data) = await service.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            return Ok(ApiResponse<PagedResult<TP_HoaHocResponse>>.Ok(new PagedResult<TP_HoaHocResponse>(total, page, pageSize, data)));
        }

        // [HttpPost("[action]")]
        // public async Task<IActionResult> Create([FromBody] TP_HoaHocCreateDto dto, CancellationToken ct)
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

        [HttpGet("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<TP_HoaHocResponse>>> GetById(int id, CancellationToken ct)
            => (await service.GetByIdAsync(id, ct)) is { } dto ? Ok(ApiResponse<TP_HoaHocResponse>.Ok(dto)) : NotFound(ApiResponse<TP_HoaHocResponse>.NotFound());

        // [HttpPut("[action]")]
        // public async Task<IActionResult> Update([FromBody] TP_HoaHocUpdateDto dto, CancellationToken ct)
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
        public async Task<ActionResult<ApiResponse<object>>> Upsert([FromBody] TP_HoaHocUpsertDto dto, CancellationToken ct)
        {
            try
            {
                var id = await service.UpsertAsync(dto, ct);
                if (id > 0)
                    return Ok(ApiResponse<object>.Ok(new { id }, "Thành công"));
                return BadRequest(ApiResponse<object>.BadRequest("Thất bại"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
            catch (DbUpdateException ex)
            {
                var (statusCode, message) = DatabaseExceptionHelper.HandleException(ex);
                return StatusCode(statusCode, ApiResponse<object>.Error(message, statusCode));
            }
            catch (Exception ex)
            {
                var (statusCode, message) = DatabaseExceptionHelper.HandleException(ex);
                return StatusCode(statusCode, ApiResponse<object>.Error(message, statusCode));
            }
        }

        [HttpDelete("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct)
        {
            try
            {
                var success = await service.DeleteAsync(id, ct);
                return success
                    ? Ok(ApiResponse<object>.Ok(null, "Xóa thành công"))
                    : NotFound(ApiResponse<object>.NotFound());
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.Conflict(ex.Message));
            }
            catch (DbUpdateException ex)
            {
                var (statusCode, message) = DatabaseExceptionHelper.HandleException(ex);
                return StatusCode(statusCode, ApiResponse<object>.Error(message, statusCode));
            }
            catch (Exception ex)
            {
                var (statusCode, message) = DatabaseExceptionHelper.HandleException(ex);
                return StatusCode(statusCode, ApiResponse<object>.Error(message, statusCode));
            }
        }

        [HttpGet("[action]/active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var data = await service.GetActiveAsync(ct);
            return Ok(data);
        }

        // [HttpGet("[action]/exists/{maTPHH}")]
        // public async Task<IActionResult> ExistsByCode(string maTPHH, CancellationToken ct)
        // {
        //     var exists = await service.ExistsByCodeAsync(maTPHH, ct);
        //     return Ok(new { exists });
        // }
    }
}
