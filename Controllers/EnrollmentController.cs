using Josi_TmsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Josi_TmsApi.Dtos;
namespace Josi_TmsApi.Controllers;
[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]
public class EnrollmentController(ICourseService courseService, IEnrollmentService enrollmentService) : ControllerBase
{

    [HttpGet(Name = "ListCourseEnrollments")]
[ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>),StatusCodes.Status404NotFound)]
[EndpointSummary("Get one enrolment for a course")]
public async Task<IActionResult> GetEnrollments(
    int courseId,
    CancellationToken ct)
{
    // Check that the parent course exists
    var course = await courseService.GetByIdAsync(courseId, ct);

    if (course is null)
    {
        return NotFound();
    }

    // Return all enrollments for the course
    var enrollments = await enrollmentService.GetByCourseAsync(courseId, ct);

    return Ok(enrollments);
}
    // GET: api/courses/{courseId}/enrollments/{id}
 [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
 [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[EndpointSummary("Get one enrolment for a course")]
public async Task<IActionResult> GetEnrollment(
    int courseId,
    int id,
    CancellationToken ct)
{
    var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);
    return enrollment is not null ? Ok(enrollment) : NotFound();
}


  // POST: api/courses/{courseId}/enrollments
    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[EndpointSummary("Enrol a student in a course")]
[EndpointDescription("Returns 404 if the course does not exist, 409 if the course has reached MaxCapacity.")]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // Check whether the course exists
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course is null)
        {
            return NotFound();
        }

        // Check whether the course has reached its maximum capacity
        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }
      // Create the enrollment
        var enrollment = await enrollmentService.CreateAsync(
            courseId,
            request,
            ct);

        return CreatedAtAction(
            nameof(GetEnrollment),
            new
            {
                courseId,
                id = enrollment.Id
            },
            enrollment);
    }

    // GET: api/courses/{courseId}/enrollments
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await enrollmentService.GetAllAsync();
        return Ok(enrollments);
    }

    // GET/api/enrollments/{id} returns one or 404
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById( int courseId,
    int id,
    CancellationToken ct)
    {
        var record = await enrollmentService.GetByIdAsync(courseId, id, ct);
        return record is not null ? Ok(record) : NotFound();

    }

    
    // POST /api/enrollments creates and returns 201 with Location header
[HttpPost]
public async Task<IActionResult> Create([FromBody]CreateEnrollmentRequest request)
{
    var record=await enrollmentService.EnrollAsync(request.StudentId,request.CourseCode);
    return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
} 
   // Addthe request model at the bottom of the same file (or a separate Models folder):
public record CreateEnrollmentRequest(string StudentId, string CourseCode);


// DELETE /api/enrollments/{id} returns 204 or 404
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id)
{
var deleted = await enrollmentService.DeleteAsync(id);
return deleted ? NoContent() : NotFound();
}
}

