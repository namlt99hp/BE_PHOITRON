using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Application.ResponsesModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoCaoProcessParamController : ControllerBase
    {
        private readonly ILoCaoProcessParamService _svc;

        public LoCaoProcessParamController(ILoCaoProcessParamService svc)
        {
            _svc = svc;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<ApiResponse<PagedResult<LoCao_ProcessParam>>>> Search(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            CancellationToken ct = default)
        {
            var (items, total) = await _svc.SearchPagedAsync(page - 1, pageSize, sortBy, sortDir, search, ct);
            return Ok(ApiResponse<PagedResult<LoCao_ProcessParam>>.Ok(new PagedResult<LoCao_ProcessParam>(total, page, pageSize, items)));
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LoCao_ProcessParam>>>> GetAll(CancellationToken ct = default)
        {
            var items = await _svc.GetAllAsync(ct);
            return Ok(ApiResponse<IEnumerable<LoCao_ProcessParam>>.Ok(items));
        }

        [HttpGet("[action]/{paLuaChonCongThucId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetConfiguredByPaId([FromRoute] int paLuaChonCongThucId, CancellationToken ct = default)
        {
            // Get all process params with their configured values for this PA
            var items = await _svc.GetConfiguredByPaIdAsync(paLuaChonCongThucId, ct);
            return Ok(ApiResponse<IEnumerable<object>>.Ok(items));
        }

        [HttpGet("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var dto = await _svc.GetDetailByIdAsync(id, ct);
            if (dto == null) return NotFound(ApiResponse<object>.NotFound());
            return Ok(ApiResponse<object>.Ok(dto));
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<LoCao_ProcessParam>>> Upsert([FromBody] LoCao_ProcessParam payload, CancellationToken ct)
        {
            try
            {
                var result = await _svc.UpsertAsync(payload, ct);
                if (payload.ID == 0)
                {
                    return CreatedAtAction(nameof(GetById), new { id = result.ID }, ApiResponse<LoCao_ProcessParam>.Ok(result, "Tạo mới thành công"));
                }
                return Ok(ApiResponse<LoCao_ProcessParam>.Ok(result, "Cập nhật thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<LoCao_ProcessParam>.BadRequest(ex.Message));
            }
        }

        [HttpPut("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Update([FromRoute] int id, [FromBody] LoCao_ProcessParam payload, CancellationToken ct)
        {
            var item = await _svc.GetByIdAsync(id, ct);
            if (item == null) return NotFound(ApiResponse<object>.NotFound());
            await _svc.UpdateAsync(id, payload, ct);
            return Ok(ApiResponse<object>.Ok(new { id }, "Cập nhật thành công"));
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> ConfigureProcessParamsForPlan([FromBody] ConfigureProcessParamsRequest request, CancellationToken ct)
        {
            try
            {
                await _svc.ConfigureProcessParamsForPlanAsync(request.PaLuaChonCongThucId, request.ProcessParamIds, request.ThuTuParams, ct);
                return Ok(ApiResponse<object>.Ok(null, "Cấu hình tham số quy trình thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
        }

        public sealed class ConfigureProcessParamsRequest
        {
            public int PaLuaChonCongThucId { get; set; }
            public List<int> ProcessParamIds { get; set; } = new();
            public List<int> ThuTuParams { get; set; } = new(); // Corresponding sequence order for each ProcessParamId
        }

        public sealed class UpsertProcessParamValuesRequest
        {
            public int PaLuaChonCongThucId { get; set; }
            public List<UpsertProcessParamValueItem> Items { get; set; } = new();
        }

        public sealed class UpsertProcessParamValueItem
        {
            public int IdProcessParam { get; set; }
            public decimal GiaTri { get; set; }
            public int? ThuTuParam { get; set; }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ApiResponse<object>>> UpsertValuesForPlan([FromBody] UpsertProcessParamValuesRequest request, CancellationToken ct)
        {
            await _svc.UpsertValuesForPlanAsync(request.PaLuaChonCongThucId, request.Items.Select(x => (x.IdProcessParam, x.GiaTri, x.ThuTuParam)).ToList(), ct);
            return Ok(ApiResponse<object>.Ok(null, "Upsert values thành công"));
        }

        // Removed GetValuesByPaId; use GetConfiguredByPaId


        [HttpDelete("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> SoftDelete([FromRoute] int id, CancellationToken ct)
        {
            var item = await _svc.GetByIdAsync(id, ct);
            if (item == null) return NotFound(ApiResponse<object>.NotFound());
            await _svc.SoftDeleteAsync(id, ct);
            return Ok(ApiResponse<object>.Ok(null, "Xóa thành công"));
        }

        public sealed class LinkOreRequest
        {
            public int? OreId { get; set; }
        }

        [HttpPut("[action]/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> LinkOre([FromRoute] int id, [FromBody] LinkOreRequest req, CancellationToken ct)
        {
            var item = await _svc.GetByIdAsync(id, ct);
            if (item == null) return NotFound(ApiResponse<object>.NotFound());
            await _svc.LinkOreAsync(id, req.OreId, ct);
            return Ok(ApiResponse<object>.Ok(null, "Thiết lập quặng liên kết thành công"));
        }
    }
}


