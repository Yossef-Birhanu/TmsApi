using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.DTOs;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Hubs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;


// ============================================================
// V2 ENROLLMENTS CONTROLLER
// ============================================================
//
// Base URL:
// /api/v2/enrollments
//
// This controller is mainly used by the Angular Enrollment List.
//
// Available endpoints:
//
// GET  /api/v2/enrollments
// POST /api/v2/enrollments
// GET  /api/v2/enrollments/{studentId}/schedule
//
// ============================================================

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(
    IMediator mediator,
    IEnrollmentService enrollmentService,IHubContext<TmsHub, ITmsHubClient> hubContext) : ControllerBase
{
    // ========================================================
    // GET: /api/v2/enrollments
    // ========================================================
    //
    // Returns all enrollments.
    //
    // Angular can use this endpoint to display the
    // Enrollment List.
    //
    // Example:
    //
    // GET http://localhost:5094/api/v2/enrollments
    //
    // ========================================================

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct)
    {
        // Get all enrollments from the database.
        var enrollments =
            await enrollmentService.GetAllAsync(ct);

        // Return HTTP 200 OK with the enrollment list.
        return Ok(enrollments);
    }


    // ========================================================
    // POST: /api/v2/enrollments
    // ========================================================
    //
    // Creates an enrollment using the MediatR command.
    //
    // ========================================================

    [HttpPost]
    public async Task<IActionResult> Enroll(
        EnrollStudentCommand command,
        CancellationToken ct)
    {
        // Send the command to the appropriate MediatR handler.
        var result = await mediator.Send(command, ct);

        // Handle both successful and failed operations.
        return result.Match<IActionResult>(

            // ------------------------------------------------
            // Enrollment was created successfully.
            // ------------------------------------------------

            onSuccess: created =>
                CreatedAtAction(
                    nameof(GetSchedule),
                    new
                    {
                        studentId = created.StudentId
                    },
                    created),
   
            // ------------------------------------------------
            // Enrollment failed.
            // ------------------------------------------------

            onFailure: error =>
            {
                // Convert the application error code into
                // an appropriate HTTP status code.
                var status = error.Code switch
                {
                    // The requested course does not exist.
                    "course_not_found"
                        => StatusCodes.Status404NotFound,

                    // The course is full or the student is
                    // already enrolled.
                    "course_full" or "already_enrolled"
                        => StatusCodes.Status409Conflict,

                    // Any other application error.
                    _
                        => StatusCodes.Status400BadRequest
                };

                // Return a standard ProblemDetails response.
                return Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}");
            });
    }
 // ========================================================
// POST: /api/v2/enrollments/{id}/approve
// ========================================================
//
// Approves an enrollment and notifies all connected
// Angular clients through SignalR.
//
// ========================================================

[HttpPost("{id:int}/approve")]
public async Task<IActionResult> Approve(
    int id,
    CancellationToken ct)
{
    // ----------------------------------------------------
    // Step 1: Approve the enrollment in the database.
    // ----------------------------------------------------

    var approved = await enrollmentService.ApproveAsync(id, ct);

    // ----------------------------------------------------
    // Step 2: If the enrollment was not found, return 404.
    // ----------------------------------------------------

    if (!approved)
    {
        return NotFound();
    }

    // ----------------------------------------------------
    // Step 3: Database approval succeeded.
    //
    // Notify ALL connected Angular clients.
    // ----------------------------------------------------

    await hubContext.Clients.All.ReceiveEnrollmentStatusUpdate(
        id.ToString(),
        "Approved");

    // ----------------------------------------------------
    // Step 4: Return HTTP 204 No Content.
    // ----------------------------------------------------

    return NoContent();
}
    
    // ========================================================
    // GET:
    // /api/v2/enrollments/{studentId}/schedule
    // ========================================================
    //
    // Returns the course schedule for a student.
    //
    // Example:
    //
    // GET /api/v2/enrollments/5/schedule
    //
    // ========================================================

    [HttpGet("{studentId:int}/schedule")]
    public async Task<IActionResult> GetSchedule(
        int studentId,
        CancellationToken ct)
    {
        // Create the schedule query for the specified student.
        var query = new GetStudentScheduleQuery(studentId);

        // Send the query through MediatR.
        var schedule = await mediator.Send(query, ct);

        // Return HTTP 200 OK.
        return Ok(schedule);
    }
}


// ============================================================
// COURSE-SPECIFIC ENROLLMENT CONTROLLER
// ============================================================
//
// Base URL:
//
// /api/courses/{courseId}/enrollments
//
// Available endpoints:
//
// GET    /api/courses/{courseId}/enrollments
// GET    /api/courses/{courseId}/enrollments/{id}
// POST   /api/courses/{courseId}/enrollments
// DELETE /api/courses/{courseId}/enrollments/{id}
//
// ============================================================

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class EnrollmentController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    // ========================================================
    // GET:
    // /api/courses/{courseId}/enrollments
    // ========================================================
    //
    // Returns all enrollments for a specific course.
    //
    // ========================================================

    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(
        typeof(IReadOnlyList<EnrollmentResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get all enrollments for a course")]
    public async Task<IActionResult> GetEnrollments(
        int courseId,
        CancellationToken ct)
    {
        // ----------------------------------------------------
        // Check whether the course exists.
        // ----------------------------------------------------

        var course = await courseService.GetByIdAsync(
            courseId,
            ct);

        // Return 404 when the course does not exist.
        if (course is null)
        {
            return NotFound();
        }

        // ----------------------------------------------------
        // Get all enrollments for this course.
        // ----------------------------------------------------

        var enrollments =
            await enrollmentService.GetByCourseAsync(
                courseId,
                ct);

        // Return HTTP 200 OK with the enrollment list.
        return Ok(enrollments);
    }


    // ========================================================
    // GET:
    // /api/courses/{courseId}/enrollments/{id}
    // ========================================================
    //
    // Returns one enrollment from a specific course.
    //
    // ========================================================

    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(
        typeof(EnrollmentResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrollment for a course")]
    public async Task<IActionResult> GetEnrollment(
        int courseId,
        int id,
        CancellationToken ct)
    {
        // Find the enrollment using both course ID and
        // enrollment ID.
        var enrollment =
            await enrollmentService.GetByIdAsync(
                courseId,
                id,
                ct);

        // Return 404 when the enrollment does not exist.
        if (enrollment is null)
        {
            return NotFound();
        }

        // Return HTTP 200 OK with the enrollment.
        return Ok(enrollment);
    }


    // ========================================================
    // POST:
    // /api/courses/{courseId}/enrollments
    // ========================================================
    //
    // Creates an enrollment for a specific course.
    //
    // ========================================================

    [HttpPost]
    [ProducesResponseType(
        typeof(EnrollmentResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Enroll a student in a course")]
    [EndpointDescription(
        "Returns 404 if the course does not exist, " +
        "409 if the course is full or the student is already enrolled.")]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // ----------------------------------------------------
        // Step 1: Check whether the course exists.
        // ----------------------------------------------------

        var course = await courseService.GetByIdAsync(
            courseId,
            ct);

        // Return 404 if the course does not exist.
        if (course is null)
        {
            return NotFound();
        }


        // ----------------------------------------------------
        // Step 2: Check whether the course is full.
        // ----------------------------------------------------

        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",

                Detail =
                    $"Course '{course.Title}' has reached " +
                    $"its maximum capacity of " +
                    $"{course.MaxCapacity}.",

                Status = StatusCodes.Status409Conflict
            });
        }
         

        // ----------------------------------------------------
        // Step 3: Check whether the student is already
        // enrolled in this course.
        // ----------------------------------------------------

        var alreadyEnrolled =
            await enrollmentService.ExistsAsync(
                request.StudentId,
                course.Code,
                ct);

        // Return 409 Conflict when a duplicate enrollment
        // is detected.
        if (alreadyEnrolled)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Already enrolled",

                Detail =
                    $"Student {request.StudentId} is already " +
                    $"enrolled in course '{course.Code}'.",

                Status = StatusCodes.Status409Conflict
            });
        }


        // ----------------------------------------------------
        // Step 4: Create the enrollment.
        // ----------------------------------------------------

        var enrollment =
            await enrollmentService.CreateAsync(
                courseId,
                request,
                ct);


        // ----------------------------------------------------
        // Step 5: Return HTTP 201 Created.
        //
        // CreatedAtAction provides the URL of the newly
        // created enrollment.
        // ----------------------------------------------------

        return CreatedAtAction(
            nameof(GetEnrollment),
            new
            {
                courseId,
                id = enrollment.Id
            },
            enrollment);
    }


    // ========================================================
    // DELETE:
    // /api/courses/{courseId}/enrollments/{id}
    // ========================================================
    //
    // Deletes an enrollment.
    //
    // Example:
    //
    // DELETE /api/courses/1/enrollments/10
    //
    // ========================================================

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete an enrollment")]
    public async Task<IActionResult> Delete(
        int courseId,
        int id,
        CancellationToken ct)
    {
        // ----------------------------------------------------
        // First verify that the enrollment belongs to the
        // requested course.
        // ----------------------------------------------------

        var enrollment =
            await enrollmentService.GetByIdAsync(
                courseId,
                id,
                ct);

        // Return 404 if the enrollment does not exist or
        // does not belong to this course.
        if (enrollment is null)
        {
            return NotFound();
        }


        // ----------------------------------------------------
        // Delete the enrollment.
        //
        // IMPORTANT:
        // DeleteAsync now expects an INT ID, so we do NOT
        // use id.ToString().
        // ----------------------------------------------------

        var deleted =
            await enrollmentService.DeleteAsync(
                id,
                ct);


        // ----------------------------------------------------
        // Return the appropriate HTTP response.
        // ----------------------------------------------------

        return deleted
            ? NoContent()
            : NotFound();
    }
}