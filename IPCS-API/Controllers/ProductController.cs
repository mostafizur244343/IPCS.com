using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPCS_API.Attributes;
using IPCS_Model.Constants;
using System.Linq;

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

        [PermissionAuthorize(Permissions.Product.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllActiveAsync();
            var dtos = products.Select(MapToDTO);
            return Ok(dtos);
        }

        [PermissionAuthorize(Permissions.Product.View)]
        [HttpGet("trash")]
        public async Task<IActionResult> GetTrashList()
        {
            var products = await _productService.GetDeletedListAsync();
            var dtos = products.Select(MapToDTO);
            return Ok(dtos);
        }

        [PermissionAuthorize(Permissions.Product.View)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound("Product not found");
            return Ok(MapToDTO(product));
        }

        [PermissionAuthorize(Permissions.Product.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var product = new Product
            {
                ProductName = model.ProductName,
                SKU = model.SKU,
                Strength = model.Strength,
                PicturePath = model.PicturePath,
                MRP = model.MRP,
                SalesPrice = model.SalesPrice,
                BaseUOMId = model.BaseUOMId,
                ReorderLevel = model.ReorderLevel,
                MinOrderQuantity = model.MinOrderQuantity,
                CategoryId = model.CategoryId ?? 0,
                BrandId = model.BrandId ?? 0,
                UOMId = model.UOMId ?? 0,
                GenericId = model.GenericId ?? 0,
                LocationId = model.LocationId,
                IsService = model.IsService,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name,
                CreatedDate = DateTime.Now
            };

            await _productService.CreateAsync(
                product, 
                model.OpeningQuantity, 
                model.OpeningCostPrice, 
                model.SelectedPurchaseUnitId,
                model.NewGenericName,
                model.NewCategoryName,
                model.NewBrandName,
                model.NewLocationName,
                model.NewUOMName,
                User.Identity?.Name
            );
            return Ok(new { Message = "Created successfully", ProductCode = product.ProductCode });
        }

        [PermissionAuthorize(Permissions.Product.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound("Product not found");

            product.ProductName = model.ProductName;
            product.SKU = model.SKU;
            product.Strength = model.Strength;
            product.PicturePath = model.PicturePath;
            product.MRP = model.MRP;
            product.SalesPrice = model.SalesPrice;
            product.BaseUOMId = model.BaseUOMId;
            product.ReorderLevel = model.ReorderLevel;
            product.MinOrderQuantity = model.MinOrderQuantity;
            product.CategoryId = model.CategoryId ?? 0;
            product.BrandId = model.BrandId ?? 0;
            product.UOMId = model.UOMId ?? 0;
            product.GenericId = model.GenericId ?? 0;
            product.LocationId = model.LocationId;
            product.IsService = model.IsService;
            product.IsActive = model.IsActive;

            await _productService.UpdateAsync(
                product,
                model.NewGenericName,
                model.NewCategoryName,
                model.NewBrandName,
                model.NewLocationName,
                model.NewUOMName,
                User.Identity?.Name
            );
            return Ok(new { Message = "Updated successfully" });
        }

        [PermissionAuthorize(Permissions.Product.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.SoftDeleteAsync(id);
            return Ok(new { Message = "Successfully moved to trash" });
        }

        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            await _productService.RestoreAsync(id);
            return Ok(new { Message = "Successfully restored" });
        }

        private static ProductDTO MapToDTO(Product p) => new ProductDTO
        {
            ProductId = p.ProductId,
            ProductCode = p.ProductCode,
            ProductName = p.ProductName,
            SKU = p.SKU,
            Strength = p.Strength,
            PicturePath = p.PicturePath,
            MRP = p.MRP,
            SalesPrice = p.SalesPrice,
            CurrentStock = p.CurrentStock,
            CostPrice = p.CostPrice,
            BaseUOMId = p.BaseUOMId,
            BaseUOMName = p.BaseUOM?.UOMName,
            ReorderLevel = p.ReorderLevel,
            MinOrderQuantity = p.MinOrderQuantity,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.CategoryName,
            BrandId = p.BrandId,
            BrandName = p.Brand?.BrandName,
            UOMId = p.UOMId,
            GenericId = p.GenericId,
            GenericName = p.Generic?.GenericName,
            LocationId = p.LocationId,
            IsService = p.IsService,
            IsActive = p.IsActive
        };
    }
}

