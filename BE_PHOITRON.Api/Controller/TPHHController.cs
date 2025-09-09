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
    public class TPHHController(ITPHHService service) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<ActionResult<PagedResult<TPHHResponse>>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken ct = default)
        {
            var (total, data) = await service.ListAsync(page, pageSize, search, sortBy, sortDir, ct);
            return Ok(new PagedResult<TPHHResponse>(total, page, pageSize, data));
        }

        [HttpGet("[action]/{id:int}")]
        public async Task<IActionResult> Get(int id, CancellationToken ct)
            => (await service.GetAsync(id, ct)) is { } dto ? Ok(dto) : NotFound();

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] TPHHCreateDto dto, CancellationToken ct)
        {
            var id = await service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id }, new { id });
        }

        [HttpPut("[action]/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TPHHUpdateDto dto, CancellationToken ct)
            => await service.UpdateAsync(id, dto, ct) ? Ok(new { success = true, id }) : NotFound();

        [HttpDelete("[action]/{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
            => await service.DeleteAsync(id, ct) ? Ok(new { success = true, id }) : NotFound();
        
        [HttpPost("[action]")]
        public async Task<IActionResult> GetByListIds([FromBody] List<int> IDs, CancellationToken ct)
        {
            var result = await service.GetByListIdsAsync(IDs, ct);
            return Ok(result);
        }
    }
}
