using Microsoft.AspNetCore.Mvc;
using Josi_TmsApi.Entities;
using Josi_TmsApi.Services;
using Tms.Api.Dtos;

namespace Josi_TmsApi.Controllers;

public interface ICoursesController
{
    Task<IActionResult> CreateCourse(Course course, CancellationToken ct);
    Task<IActionResult> GetCourseById(int id, CancellationToken ct);
}

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase, ICoursesController
{
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        // Use GetAsync which is the common async accessor on ICourseService
         var course = await courseService.GetByIdAsync(id, ct);

    //     if (course is null)
    //     {
    //         return NotFound();
    //     }

    //     return Ok(course);
    // }

    // [HttpPost]
    // public async Task<IActionResult> CreateCourse(Course course, CancellationToken ct)
    // {
    //     var result = await courseService.CreateAsync(course, ct);

    //     return CreatedAtAction(nameof(GetCourseById), new { id = result.Id },result);
    // }

        return course is not null
            ? Ok(course)
            : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        
    if (await courseService.CodeExistsAsync(request.Code, ct))
    {
        return Conflict(new ProblemDetails
        {
            Title = "Course code already exists",
            Detail = $"A course with code '{request.Code}' is already registered.",
            Status = StatusCodes.Status409Conflict
        });
    }

    var result = await courseService.CreateAsync(request, ct);

    return CreatedAtAction(
        nameof(GetCourseById),
        new { id = result.Id },
        result);
          }

    public Task<IActionResult> CreateCourse(Course course, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
