using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModel;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuangController(IQuangService service) : ControllerBase
    {

        //[HttpGet]
        //public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
        //{
        //    var (total, data) = await service.ListAsync(page, pageSize, search, ct);
        //    return Ok(new { total, data, page, pageSize });
        //}
        [HttpGet("[action]")]
        public async Task<ActionResult<PagedResult<QuangResponse>>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken ct = default)
        {
            var (total, data) = await service.ListAsync(page, pageSize, search, sortBy, sortDir, ct);
            return Ok(new PagedResult<QuangResponse>(total, page, pageSize, data));
        }


        [HttpGet("[action]/{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await service.GetAsync(id, ct)) is { } dto ? Ok(dto) : NotFound();

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] QuangCreateDto dto, CancellationToken ct)
        {
            var id = await service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("[action]/{id:int}")]
        public async Task<IActionResult> UpdateById(int id, [FromBody] QuangUpdateDto dto, CancellationToken ct)
            => await service.UpdateAsync(id, dto, ct) ? Ok(new { success = true, id }) : NotFound();

        [HttpDelete("[action]/{id:int}")]
        public async Task<IActionResult> DeleteById(int id, CancellationToken ct)
            => await service.DeleteAsync(id, ct) ? Ok(new { success = true, id }) : NotFound();

        [HttpPut]
        [Route("[action]")]
        public async Task<ActionResult> UpdateTPHHChoQuang(
        [FromBody] Quang_TPHHUpdateDto dto,
        CancellationToken ct)
        {
            var affected = await service.UpdateTPHH(dto, ct);
            return Ok(new
            {
                message = "Đồng bộ thành công.",
                affected
            });
        }

        
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult> GetDetailQuang(
        [FromQuery] int id,
        CancellationToken ct)
        {
            var result = await service.GetDetailQuang(id, ct);
            return Ok(result);
        }

        
        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> UpsertQuangMua(
        [FromBody] UpsertQuangMuaDto dto,
        CancellationToken ct)
        {
            var result = await service.UpsertAsync(dto, ct);
            return Ok(result);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> GetOreChemistryBatch([FromBody] List<int> id_Quangs, CancellationToken ct)
        {
            var result = await service.getOreChemistryBatch(id_Quangs, ct);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetByListIds([FromBody] List<int> IDs, CancellationToken ct)
        {
            var result = await service.GetByListIdsAsync(IDs, ct);
            return Ok(result);
        }
    }
}
