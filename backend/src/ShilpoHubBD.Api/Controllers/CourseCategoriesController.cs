using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/course-categories")]
public class CourseCategoriesController : ControllerBase
{
    private readonly ICourseCategoryService _categoryService;

    public CourseCategoriesController(ICourseCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CourseCategoryDto>>> GetAll(
        [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.GetAllAsync(activeOnly, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<CourseCategoryDto>> Create(CreateCourseCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), result);
    }
}
