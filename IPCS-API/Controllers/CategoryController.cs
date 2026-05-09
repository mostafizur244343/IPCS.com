using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

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
            try
            {
                var categories = await _categoryService.GetAllAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Category.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Input Format is Wrong please try Accurately");
                }

                var category = new Category
                {
                    CategoryName = model.CategoryName,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedBy = User.Identity?.Name
                };


                var result = await _categoryService.CreateAsync(category);
                if (result)
                {
                    return Ok(new { Message = "Category Created Successfully" });
                }
                return BadRequest(new { Message = "Category Create Error... Please try again" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
