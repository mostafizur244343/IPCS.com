using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

namespace IPCS_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // 1. All Active Product List
        [PermissionAuthorize(Permissions.Product.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var results = await _productService.GetAllActiveAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Loading Product Error", Error = ex.Message });
            }
        }

        // 2. List of Deleted Product For Dashboard
        [PermissionAuthorize(Permissions.Product.View)]
        [HttpGet("trash")]
        public async Task<IActionResult> GetTrashList()
        {
            try
            {
                var results = await _productService.GetDeletedListAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error Loading Trash List", Error = ex.Message });
            }
        }

        // 3 See Product details By ID
        [PermissionAuthorize(Permissions.Product.View)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null) return NotFound(new { Message = "Product Not Found" });
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 4.Creating New Product With (Opening Stock & Moving Average Logic)
        [PermissionAuthorize(Permissions.Product.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDTO model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Please Enter Valid Info...");

                var product = new Product
                {
                    ProductName = model.ProductName,
                    SKU = model.SKU,
                    Strength = model.Strength,
                    MRP = model.MRP,
                    SalesPrice = model.SalesPrice,
                    BaseUOMId = model.BaseUOMId,
                    ReorderLevel = model.ReorderLevel,
                    MinOrderQuantity = model.MinOrderQuantity,
                    CategoryId = model.CategoryId,
                    BrandId = model.BrandId,
                    UOMId = model.UOMId,
                    GenericId = model.GenericId,
                    LocationId = model.LocationId,
                    IsService = model.IsService,
                    IsActive = model.IsActive,
                    CreatedBy = User.Identity?.Name
                };

                // Sending Opening Qty and Cost in Service Level
                await _productService.CreateAsync(product, model.OpeningQuantity, model.OpeningCostPrice, model.SelectedPurchaseUnitId);
                return Ok(new { Message = "Create Successfully..." });
            }
            catch (Exception ex)
            {
                // Handling Unique Index Validation
                if (ex.InnerException?.Message.Contains("IX_Product_ProductName") == true)
                    return BadRequest(new { Message = "Already have in stock" });

                return BadRequest(new { Message = ex.Message });
            }
        }

        // 5. Updating Product
        [PermissionAuthorize(Permissions.Product.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDTO model)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null) return NotFound(new { Message = "Product Not Found" });

                product.ProductName = model.ProductName;
                product.SKU = model.SKU;
                product.Strength = model.Strength;
                product.MRP = model.MRP;
                product.SalesPrice = model.SalesPrice;
                product.BaseUOMId = model.BaseUOMId;
                product.ReorderLevel = model.ReorderLevel;
                product.MinOrderQuantity = model.MinOrderQuantity;
                product.CategoryId = model.CategoryId;
                product.BrandId = model.BrandId;
                product.UOMId = model.UOMId;
                product.GenericId = model.GenericId;
                product.LocationId = model.LocationId;
                product.IsService = model.IsService;
                product.IsActive = model.IsActive;

                await _productService.UpdateAsync(product);
                return Ok(new { Message = "Update Successfully..." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 6. Soft Delete
        [PermissionAuthorize(Permissions.Product.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _productService.SoftDeleteAsync(id);
                if (!result) return NotFound(new { Message = "Not Found For Delete" });
                return Ok(new { Message = "Product Moved to Trash Successfully..." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 7. Restore from Trash
        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var result = await _productService.RestoreAsync(id);
                if (!result) return NotFound(new { Message = "Product Not Found in Trash" });
                return Ok(new { Message = "Product Restored Successfully..." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}

