using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

namespace IPCS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductUnitConversionController : ControllerBase
    {
        private readonly IProductUnitConversionService _service;
        public ProductUnitConversionController(IProductUnitConversionService service) { _service = service; }

        [PermissionAuthorize(Permissions.Product.View)]
        [HttpGet("by-product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId) => Ok(await _service.GetByProductIdAsync(productId));

        [PermissionAuthorize(Permissions.Product.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllConversionsAsync());

        [PermissionAuthorize(Permissions.Product.Edit)]
        [HttpPost]
        public async Task<IActionResult> Create(ProductUnitConversionDTO dto)
        {
            var model = new ProductUnitConversion
            {
                ProductId = dto.ProductId,
                FromUnitId = dto.FromUnitId,
                ToUnitId = dto.ToUnitId,
                Factor = dto.Factor,
                Level = dto.Level
            };
            return Ok(await _service.AddConversionAsync(model));
        }

        [PermissionAuthorize(Permissions.Product.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductUnitConversionDTO dto)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound("Conversion not found");

            existing.ProductId = dto.ProductId;
            existing.FromUnitId = dto.FromUnitId;
            existing.ToUnitId = dto.ToUnitId;
            existing.Factor = dto.Factor;
            existing.Level = dto.Level;

            var result = await _service.UpdateAsync(existing);
            if (result) return Ok(new { Message = "Conversion updated successfully" });
            return BadRequest("Error updating conversion");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) => Ok(await _service.DeleteConversionAsync(id));

        [PermissionAuthorize(Permissions.Product.View)]
        [HttpGet("trash")]
        public async Task<IActionResult> GetTrash() => Ok(await _service.GetDeletedConversionsAsync());

        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var result = await _service.RestoreConversionAsync(id);
                if (result) return Ok(new { Message = "Conversion restored successfully" });
                return NotFound(new { Message = "Conversion not found or not deleted" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }
    }
}