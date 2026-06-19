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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) => Ok(await _service.DeleteConversionAsync(id));
    }
}