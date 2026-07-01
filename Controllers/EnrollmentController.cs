using Microsoft.AspNetCore.Mvc;
using Josi_TmsApi.Dtos;
using Josi_TmsApi.Services;
using Tms.Api.Dtos;

namespace Josi_TmsApi.Controllers;


// [ApiController]
// [Route("api/enrollments")]
// public class EnrollmentController(IEnrollmentService enrollmentService) : ControllerBase
// {
//     //Get/api/enrollments returns all enrollments records
//     [HttpGet]
//     public async Task<IActionResult> GetAll()
//     {
//         var enrollments = await enrollmentService.GetAllAsync();
//         return Ok(enrollments);
//     }

//     // GET/api/enrollments/{id} returns one or 404
//     [HttpGet("{id}")]
//     public async Task<IActionResult> GetById(string id)
//     {
//         var record = await enrollmentService.GetByIdAsync(id);
//         return record is not null ? Ok(record) : NotFound();

//     }
//     // POST /api/enrollments creates and returns 201 with Location header
// [HttpPost]
// public async Task<IActionResult> Create([FromBody]CreateEnrollmentRequest request)
// {
//     var record=await enrollmentService.EnrollAsync(request.StudentId,request.CourseCode);
//     return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
// } 
//    // Addthe request model at the bottom of the same file (or a separate Models folder):
// public record CreateEnrollmentRequest(string StudentId, string CourseCode);


// // DELETE /api/enrollments/{id} returns 204 or 404
// [HttpDelete("{id}")]
// public async Task<IActionResult> Delete(string id)
// {
// var deleted = await enrollmentService.DeleteAsync(id);
// return deleted ? NoContent() : NotFound();
// }

// }

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
public class EnrollmentsController : ControllerBase
{
<<<<<<< HEAD
    private readonly ICourseService courseService;
    private readonly IEnrollmentService enrollmentService;

    public EnrollmentsController(
        ICourseService courseService,
        IEnrollmentService enrollmentService)
=======
    //Get/api/enrollments returns all enrollments records
    [HttpGet]
    public async Task<IActionResult> GetAll()
>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
    {
        this.courseService = courseService;
        this.enrollmentService = enrollmentService;
    }

    // GET: api/courses/{courseId}/enrollments/{id}
    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    public async Task<IActionResult> GetEnrollment(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);

        if (enrollment == null)
            return NotFound();

        return Ok(enrollment);
    }

    // POST: api/courses/{courseId}/enrollments
    [HttpPost]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course == null)
        {
            return NotFound();
        }

        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var enrollment = await enrollmentService.CreateAsync(courseId, request, ct);

        return CreatedAtAction(
            nameof(GetEnrollment),
            new
            {
                courseId,
                id = enrollment.Id
            },
            enrollment);
    }
}

