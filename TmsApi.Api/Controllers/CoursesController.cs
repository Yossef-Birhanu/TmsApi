//Exercise 1: Implement the CoursesController class to handle HTTP requests related to courses.
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
namespace TmsApi.Api.Controllers;
[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(ICourseService CourseService, LinkGenerator linkGenerator) : ControllerBase
{

[HttpGet("{id:int}", Name = nameof(GetCourseById))]
[ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[EndpointSummary("Get a course by ID")]
[EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
public async Task<IActionResult> GetCourseById(
    int id,
    CancellationToken ct)
{
    var course = await CourseService.GetByIdAsync(id, ct);

    if (course is null)
    {
        return NotFound();
    }

    // Build URLs

    var selfHref = linkGenerator.GetPathByName(
        HttpContext,
        nameof(GetCourseById),
        new { id });

    var enrollmentsHref = linkGenerator.GetPathByAction(
        HttpContext,
        action: "GetEnrollments",
        controller: "Enrollments",
        values: new { courseId = id });
  // Build the links for the course detail DTO
    var links = new List<LinkDto>
    {
        new LinkDto(selfHref!,"self", "GET"),

        new LinkDto(selfHref!,"update","PUT"),

        new LinkDto(selfHref!,"delete","DELETE"),

        new LinkDto( enrollmentsHref!, "enrollments", "GET")
    };

    // Only expose the enroll action if the course has room

    if (course.EnrollmentCount < course.MaxCapacity)
    {
        links.Add(
            new LinkDto(
                enrollmentsHref!,
                "enroll",
                "POST"));
    }

    var detailDto = new CourseDetailDto
    {
        Id = course.Id,
        Code = course.Code,
        Title = course.Title,
        MaxCapacity = course.MaxCapacity,
        EnrollmentCount = course.EnrollmentCount,
        Links = links
    };

    return Ok(detailDto);
}
[HttpGet]
[ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
[EndpointSummary("List courses with pagination")]
[EndpointDescription("Returns a paginated, optionally filtered listof TMS courses. PageSize is capped at 50.")]
public async Task<IActionResult> GetCourses(
[FromQuery] PagedRequest request, CancellationToken ct)
{

var result = await CourseService.GetCoursesAsync(request, ct);
return Ok(result);
}

[HttpPost]
[ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[EndpointSummary("Create a new course")]
[EndpointDescription("Creates a course with a unique code. Returns409 if the course code already exists.")]
// public async Task<IActionResult> CreateCourse(Course course, CancellationToken ct)
// {
//     var createdCourse = await CourseService.CreateAsync(course, ct);
//     return CreatedAtRoute(nameof(GetCourseById), new { id = createdCourse.Id }, createdCourse);
//     throw new NotImplementedException();
// }
public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
{
    
   // Check if the course code already exists
    if (await CourseService.CodeExistsAsync(request.Code, ct))
    {
        return Conflict(new ProblemDetails
        {
            Title = "Course code already exists",
            Detail = $"A course with code '{request.Code}' is already registered.",
            Status = StatusCodes.Status409Conflict
        });
    }

    // Create the course
    var result = await CourseService.CreateAsync(request, ct);

    return CreatedAtAction(
        nameof(GetCourseById),
        new { id = result.Id },
        result);
}
}


 

