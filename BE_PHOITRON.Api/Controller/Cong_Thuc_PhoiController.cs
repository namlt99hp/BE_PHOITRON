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
        // public async Task<ActionResult<ApiResponse<object>>> Upsert([FromBody] Cong_Thuc_PhoiUpsertDto dto, CancellationToken ct)
        // {
        //     try
        //     {
        //         var id = await service.UpsertAsync(dto, ct);
        //         if (id > 0) return Ok(ApiResponse<object>.Ok(new { id }, "Thành công"));
        //         return BadRequest(ApiResponse<object>.BadRequest("Thất bại"));
        //     }
        //     catch (InvalidOperationException ex)
        //     {
        //         return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
        //     }
        //     catch (DbUpdateException ex)
        //     {
        //         var (statusCode, message) = DatabaseExceptionHelper.HandleException(ex);
        //         return StatusCode(statusCode, ApiResponse<object>.Error(message, statusCode));
        //     }
        //     catch (Exception ex)
        //     {
        //         var (statusCode, message) = DatabaseExceptionHelper.HandleException(ex);
        //         return StatusCode(statusCode, ApiResponse<object>.Error(message, statusCode));
        //     }
        // }

        // [HttpDelete("[action]/{id:int}")]
        // public async Task<ActionResult<ApiResponse<object>>> SoftDelete(int id, CancellationToken ct)
        // {
        //     try
        //     {
        //         var success = await service.SoftDeleteAsync(id, ct);
        //         return success ? Ok(ApiResponse<object>.Ok(null, "Xóa thành công")) : NotFound(ApiResponse<object>.NotFound());
        //     }
        //     catch (DbUpdateException ex)
        //     {
        //         var (statusCode, message) = DatabaseExceptionHelper.HandleException(ex);
        //         return StatusCode(statusCode, ApiResponse<object>.Error(message, statusCode));
        //     }
        //     catch (Exception ex)
        //     {
        //         var (statusCode, message) = DatabaseExceptionHelper.HandleException(ex);
        //         return StatusCode(statusCode, ApiResponse<object>.Error(message, statusCode));
        //     }
        // }

        [HttpDelete("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCongThucPhoi(int id, CancellationToken ct)
        {
            try
            {
                var success = await service.DeleteCongThucPhoiAsync(id, ct);
                return success ? Ok(ApiResponse<object>.Ok(null, "Xóa công thức phối và dữ liệu liên quan thành công")) : NotFound(ApiResponse<object>.NotFound("Không tìm thấy công thức phối"));
            }
            catch (InvalidOperationException ex)
            {
                // Return 409 Conflict for business rule violations
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

        [HttpGet("[action]/quang-daura/{idQuangDauRa:int}")]
        public async Task<IActionResult> GetByQuangDauRa(int idQuangDauRa, CancellationToken ct)
        {
            var data = await service.GetByQuangDauRaAsync(idQuangDauRa, ct);
            return data is null ? NotFound(ApiResponse<Cong_Thuc_PhoiResponse>.NotFound("Không tìm thấy công thức phối")) : Ok(ApiResponse<Cong_Thuc_PhoiResponse>.Ok(data));
        }

        [HttpGet("[action]/active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var data = await service.GetActiveAsync(ct);
            return Ok(data);
        }
    }
}
