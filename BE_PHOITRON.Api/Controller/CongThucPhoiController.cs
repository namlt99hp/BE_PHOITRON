using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModel;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_PHOITRON.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CongThucPhoiController(ICongThucPhoiService service) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<ActionResult<PagedResult<CongThucPhoiResponse>>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken ct = default)
        {
            var (total, data) = await service.ListAsync(page, pageSize, search, sortBy, sortDir, ct);
            return Ok(new PagedResult<CongThucPhoiResponse>(total, page, pageSize, data));
        }

        [HttpGet("[action]/{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await service.GetAsync(id, ct)) is { } dto ? Ok(dto) : NotFound();

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] CreateCongThucPhoiDto dto, CancellationToken ct)
        {
            var id = await service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromBody] UpdateCongThucPTDto dto, CancellationToken ct)
        {
            var id = await service.UpdateCongThucPTDto(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetDetail([FromQuery] int id, CancellationToken ct)
        {
            var result = await service.GetCongThucPhoiDetail(id, ct);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpsertCongThucPhoiTron([FromBody] UpsertCongThucPTDto dto, CancellationToken ct)
        {
            var id = await service.UpsertCongThucPhoiTron(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        //-----------------------------------------
        [HttpPost("[action]")]
        public async Task<ActionResult<UpsertResult>> UpsertAndConfirm([FromBody] UpsertAndConfirmDto dto, CancellationToken ct)
        {
            var result = await service.UpsertAndConfirmAsync(dto, ct);
            return Ok(result);
        }
       

        [HttpGet("[action]")]
        public async Task<ActionResult<CongThucEditVm?>> GetForEdit([FromRoute] int id, CancellationToken ct)
        {
            var result = await service.GetForEditAsync(id, ct);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<NeoDashboardVm?>> GetByNeoAsync([FromRoute] int neoId, CancellationToken ct)
        {
            var result = await service.GetByNeoAsync(neoId, ct);
            return Ok(result);
        }
    }
}
