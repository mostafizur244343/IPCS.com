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

        [HttpPost]
        public async Task<IActionResult> Create(GlobalUnitConversion model)
        {
            var result = await _service.CreateAsync(model);
            if (result) return Ok(new { Message = "Global Conversion Added Successfully!" });
            return BadRequest("Error saving data");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (result) return Ok(new { Message = "Deleted successfully!" });
            return BadRequest("Error deleting data");
        }
    }
}
