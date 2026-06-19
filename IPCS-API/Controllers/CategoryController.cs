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
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [PermissionAuthorize(Permissions.Category.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            var dtos = categories.Select(c => new CategoryDTO
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                IsActive = c.IsActive
            });
            return Ok(dtos);
        }

        [PermissionAuthorize(Permissions.Category.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = new Category
            {
                CategoryName = model.CategoryName,
                Description = model.Description,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name,
                CreatedDate = DateTime.Now
            };

            await _categoryService.CreateAsync(category);
            
            model.CategoryId = category.CategoryId;
            return Ok(model);
        }

        [PermissionAuthorize(Permissions.Category.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound("Category not found");

            category.CategoryName = model.CategoryName;
            category.Description = model.Description;
            category.IsActive = model.IsActive;

            await _categoryService.UpdateAsync(category);
            return Ok(new { Message = "Updated successfully" });
        }

        [PermissionAuthorize(Permissions.Category.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteAsync(id);
            return Ok(new { Message = "Successfully deleted" });
        }
    }
}

