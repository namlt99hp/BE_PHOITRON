using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Infrastructure.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoaiQuangController(ILoaiQuangService service) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<ActionResult<ApiResponse<PagedResult<LoaiQuangResponse>>>> Search(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            CancellationToken ct = default)
        {
            var (total, data) = await service.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            return Ok(ApiResponse<PagedResult<LoaiQuangResponse>>.Ok(new PagedResult<LoaiQuangResponse>(total, page, pageSize, data)));
        }

        [HttpGet("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<LoaiQuangResponse>>> GetById(int id, CancellationToken ct)
            => (await service.GetByIdAsync(id, ct)) is { } dto
                ? Ok(ApiResponse<LoaiQuangResponse>.Ok(dto))
                : NotFound(ApiResponse<LoaiQuangResponse>.NotFound());

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> Upsert([FromBody] LoaiQuangUpsertDto dto, CancellationToken ct)
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
        public async Task<ActionResult<ApiResponse<IReadOnlyList<LoaiQuangResponse>>>> GetActive(CancellationToken ct)
        {
            var data = await service.GetActiveAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<LoaiQuangResponse>>.Ok(data));
        }
    }
}

