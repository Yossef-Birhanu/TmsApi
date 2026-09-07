// using Asp.Versioning;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using TmsApi.Infrastructure.Persistence;
// namespace TmsApi.Api.Controllers.V2;

// [ApiController]
// [Route("api/v{version:apiVersion}/Courses")]
// [ApiVersion("2.0")]

// public class CoursesController(TmsDb1Context context) : ControllerBase
// {
//     [HttpGet]
//     public async Task<IActionResult>GetCourses(
//         [FromQuery] int page=1,
//         [FromQuery] int pageSize=20,
//         CancellationToken ct =default
//     )
//     {
//         page=Math.Max(1,page);
//         pageSize=Math.Clamp(pageSize,1,50);
//         var baseQuery= context.Courses.AsNoTracking();
//         var totalCount= await baseQuery.CountAsync(ct);

//         var rows =  await baseQuery
//         .OrderBy(c=>c.Title)
//         .Skip((page-1)*pageSize)
//         .Take(pageSize)
//         .Select(c => new
//         {
//             c.Id,
//             c.Title,
//             c.Code,
//             c.MaxCapacity,
//             EnrollmentCount=c.Enrollments.Count
//         }).ToListAsync(ct);
//         var totalPages=(int)Math.Ceiling(totalCount/(double)pageSize);
//         var hasNext=page<totalPages;
//         var hasPrevious =page>1;
//         // return Ok(new
//         // {
//         //     data=rows,
//         //     meta = new
//         //     {
//         //         totalCount,
//         //         page,
//         //         pageSize,
//         //         totalPages,
//         //         hasNext,
//         //         hasPrevious
//         //     },
//         //     links = new
//         //     {
//         //         self=$"/api/V2/Courses?page={page}&pageSize={pageSize}",
//         //         next=hasNext?$"api/V2/Courses?page={page+1}&pageSize={pageSize}":(string?)null,
//         //         prev=hasPrevious?$"api/V2/Courses?page={page-1}&pageSize={pageSize}":(string?)null,
//         //         enroll="/api/V2/enrollments"
//         //     }
//         // });

//         return Ok(new{
//     items = rows,
//     totalCount,
//     page,
//     pageSize,
//     totalPages,
//     hasNext,
//     hasPrevious,
//     links = new
//     {
//         self = $"/api/v2.0/courses?page={page}&pageSize={pageSize}",
//         next = hasNext
//             ? $"/api/v2.0/courses?page={page + 1}&pageSize={pageSize}"
//             : null,
//         prev = hasPrevious
//             ? $"/api/v2.0/courses?page={page - 1}&pageSize={pageSize}"
//             : null,
//         enroll = "/api/v2.0/enrollments"
//     }
// });
//     }

    
// }



using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/Courses")]
[ApiVersion("2.0")]
public class CoursesController(
    TmsDb1Context context,
    ICourseService courseService) : ControllerBase
{
    // =========================================================
    // GET COURSES
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var baseQuery = context.Courses.AsNoTracking();

        var totalCount = await baseQuery.CountAsync(ct);

        var rows = await baseQuery
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Code,
                c.MaxCapacity,
                EnrollmentCount = c.Enrollments.Count
            })
            .ToListAsync(ct);

        var totalPages =
            (int)Math.Ceiling(totalCount / (double)pageSize);

        var hasNext = page < totalPages;
        var hasPrevious = page > 1;

        return Ok(new
        {
            items = rows,
            totalCount,
            page,
            pageSize,
            totalPages,
            hasNext,
            hasPrevious,

            links = new
            {
                self =
                    $"/api/v2.0/courses?page={page}&pageSize={pageSize}",

                next = hasNext
                    ? $"/api/v2.0/courses?page={page + 1}&pageSize={pageSize}"
                    : null,

                prev = hasPrevious
                    ? $"/api/v2.0/courses?page={page - 1}&pageSize={pageSize}"
                    : null,

                enroll = "/api/v2.0/enrollments"
            }
        });
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
    public async Task<IActionResult> CreateCourse(
        [FromBody] CreateCourseRequest request,
        CancellationToken ct)
    {
        // Check whether the course code already exists.
        if (await courseService.CodeExistsAsync(
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
        var result = await courseService.CreateAsync(
            request,
            ct);

        return Created(
            $"/api/v2.0/courses/{result.Id}",
            result);
    }
}