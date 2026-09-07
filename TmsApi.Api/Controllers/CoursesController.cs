//Exercise 1: Implement the CoursesController class to handle HTTP requests related to courses.
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
namespace TmsApi.Api.Controllers;
[Authorize(Roles = "Instructor,Admin")]
[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService CourseService,
    LinkGenerator linkGenerator,
    IAuthorizationService authorizationService) : ControllerBase
{
    // =========================================================
    // GET COURSE BY ID
    // =========================================================

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(
        typeof(CourseDetailDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription(
        "Returns course details with HATEOAS links. " +
        "Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetCourseById(
        int id,
        CancellationToken ct)
    {
        var course = await CourseService.GetByIdAsync(id, ct);

        if (course is null)
        {
            return NotFound();
        }

        // Build URL for the current course.
        var selfHref = linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetCourseById),
            new { id });

        // Build URL for course enrollments.
        var enrollmentsHref = linkGenerator.GetPathByAction(
            HttpContext,
            action: "GetEnrollments",
            controller: "Enrollments",
            values: new { courseId = id });

        // Build HATEOAS links.
        var links = new List<LinkDto>
        {
            new LinkDto(
                selfHref!,
                "self",
                "GET"),

            new LinkDto(
                selfHref!,
                "update",
                "PUT"),

            new LinkDto(
                selfHref!,
                "delete",
                "DELETE"),

            new LinkDto(
                enrollmentsHref!,
                "enrollments",
                "GET")
        };

        // Only expose the enroll action when the course
        // still has available capacity.
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


    // =========================================================
    // GET ALL COURSES
    // =========================================================

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResponse<CourseResponseDto>),
        StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription(
        "Returns a paginated, optionally filtered list " +
        "of TMS courses. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var result = await CourseService.GetCoursesAsync(
            request,
            ct);

        return Ok(result);
    }


    // =========================================================
    // CREATE COURSE
    // =========================================================

    [HttpPost]
    [ProducesResponseType(
        typeof(CourseResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription(
        "Creates a course with a unique code. " +
        "Returns 409 if the course code already exists.")]
    public async Task<IActionResult> CreateCourse(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        // Check whether the course code already exists.
        if (await CourseService.CodeExistsAsync(
            request.Code,
            ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail =
                    $"A course with code '{request.Code}' " +
                    "is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        // Create the course.
        var result = await CourseService.CreateAsync(
            request,
            ct);

        return CreatedAtAction(
            nameof(GetCourseById),
            new { id = result.Id },
            result);
    }


    // =========================================================
    // UPDATE COURSE
    // =========================================================

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a course")]
    [EndpointDescription(
        "Updates a course after checking the CanEditCourse " +
        "authorization policy.")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        [FromBody] UpdateCourseRequest request,
        CancellationToken ct)
    {
        // -----------------------------------------------------
        // Find the course first.
        // -----------------------------------------------------
        var course = await CourseService.GetByIdAsync(
            id,
            ct);

        if (course is null)
        {
            // Course does not exist.
            return NotFound();
        }

        // -----------------------------------------------------
        // Enforce the CanEditCourse policy.
        //
        // User   = current logged-in user
        // course = resource being edited
        // -----------------------------------------------------
        var authResult =
            await authorizationService.AuthorizeAsync(
                User,
                course,
                "CanEditCourse");

        // -----------------------------------------------------
        // If authorization fails, return 403 Forbidden.
        // -----------------------------------------------------
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        // -----------------------------------------------------
        // Update the course.
        // -----------------------------------------------------
        var updated = await CourseService.UpdateAsync(
            id,
            request,
            ct);

        if (!updated)
        {
            return NotFound();
        }

        // -----------------------------------------------------
        // Update successful.
        // HTTP 204 = No Content.
        // -----------------------------------------------------
        return NoContent();
    }
}