using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPCS_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalUnitConversionController : ControllerBase
    {
        private readonly IGlobalUnitConversionService _service;

        public GlobalUnitConversionController(IGlobalUnitConversionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var conversion = await _service.GetByIdAsync(id);
            if (conversion == null) return NotFound("Conversion not found");
            return Ok(conversion);
        }

        [HttpPost]
        public async Task<IActionResult> Create(GlobalUnitConversion model)
        {
            var result = await _service.CreateAsync(model);
            if (result) return Ok(new { Message = "Global Conversion Added Successfully!" });
            return BadRequest("Error saving data");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, GlobalUnitConversion model)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound("Conversion not found");

            model.Id = id;
            var result = await _service.UpdateAsync(model);
            if (result) return Ok(new { Message = "Global Conversion Updated Successfully!" });
            return BadRequest("Error updating data");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (result) return Ok(new { Message = "Deleted successfully!" });
                return BadRequest("Error deleting data");
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [HttpGet("trash")]
        public async Task<IActionResult> GetTrash() => Ok(await _service.GetDeletedListAsync());

        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var result = await _service.RestoreAsync(id);
                if (result) return Ok(new { Message = "Conversion restored successfully" });
                return NotFound(new { Message = "Conversion not found or not deleted" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }
    }
}
