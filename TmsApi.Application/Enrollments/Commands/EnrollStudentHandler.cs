// using MediatR;
// using TmsApi.Application.Common;
// using TmsApi.Application.Interfaces;
// using TmsApi.Domain.Entities;

// namespace TmsApi.Application.Enrollments.Commands;

// public class EnrollStudentHandler(
//     IEnrollmentService enrollmentService,
//     ICourseService courseService)
//     : IRequestHandler<EnrollStudentCommand, Result<EnrollmentCreated, EnrollmentError>>
// {
//     public async Task<Result<EnrollmentCreated, EnrollmentError>> Handle(
//         EnrollStudentCommand command,
//         CancellationToken ct)
//     {
//         var course = await courseService.GetByCodeAsync(command.CourseCode, ct);

//         if (course is null)
//         {
//             return Result<EnrollmentCreated, EnrollmentError>.Failure(
//                 EnrollmentError.CourseNotFound(command.CourseCode));
//         }

//         if (course.Enrollments.Count >= course.MaxCapacity)
//         {
//             return Result<EnrollmentCreated, EnrollmentError>.Failure(
//                 EnrollmentError.CourseFull(course.Title, course.MaxCapacity));
//         }

//         if (await enrollmentService.ExistsAsync(command.StudentId, command.CourseCode, ct))
//         {
//             return Result<EnrollmentCreated, EnrollmentError>.Failure(
//                 EnrollmentError.AlreadyEnrolled(command.StudentId, command.CourseCode));
//         }

//         var enrollment = new Enrollment
//         {
//             StudentId = command.StudentId,
//             CourseId = course.Id,
//             EnrolledAt = DateTime.UtcNow
//         };

//         await enrollmentService.AddAsync(enrollment, ct);

//         return Result<EnrollmentCreated, EnrollmentError>.Success(
//             new EnrollmentCreated(
//                 enrollment.Id,
//                 enrollment.StudentId,
//                 course.Code));
//     }
// }

// public class EnrollmentCreated
// {
//     private int id;
//     private int studentId;
//     private string code;

//     public EnrollmentCreated(int id, int studentId, string code)
//     {
//         this.id = id;
//         this.studentId = studentId;
//         this.code = code;
//     }

//     public object StudentId { get; set; }
// }


using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Enrollments.Commands;

public class EnrollStudentHandler(
    IEnrollmentService enrollmentService,
    ICourseService courseService)
    : IRequestHandler<EnrollStudentCommand, Result<EnrollmentCreated, EnrollmentError>>
{
    public async Task<Result<EnrollmentCreated, EnrollmentError>> Handle(
        EnrollStudentCommand command,
        CancellationToken ct)
    {
        // Find the course using its course code.
        var course = await courseService.GetByCodeAsync(
            command.CourseCode,
            ct);

        // Course does not exist.
        if (course is null)
        {
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.CourseNotFound(command.CourseCode));
        }

        // Check whether the course is already full.
        if (course.Enrollments.Count >= course.MaxCapacity)
        {
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.CourseFull(
                    course.Title,
                    course.MaxCapacity));
        }

        // Prevent the same student from enrolling twice.
        if (await enrollmentService.ExistsAsync(
            command.StudentId,
            command.CourseCode,
            ct))
        {
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.AlreadyEnrolled(
                    command.StudentId,
                    command.CourseCode));
        }

        // Create a new enrollment.
        var enrollment = new Enrollment
        {
            StudentId = command.StudentId,
            CourseId = course.Id,

            // New enrollments start as Pending.
            Status = EnrollmentStatus.Pending,

            EnrolledAt = DateTime.UtcNow
        };

        // Save the enrollment through the service.
        await enrollmentService.AddAsync(enrollment, ct);

        // Return the created enrollment information.
        return Result<EnrollmentCreated, EnrollmentError>.Success(
            new EnrollmentCreated(
                enrollment.Id,
                enrollment.StudentId,
                course.Code));
    }
}

public class EnrollmentCreated
{
    public EnrollmentCreated(
        int id,
        int studentId,
        string code)
    {
        Id = id;
        StudentId = studentId;
        Code = code;
    }

    public int Id { get; set; }

    public int StudentId { get; set; }

    public string Code { get; set; } = string.Empty;
}