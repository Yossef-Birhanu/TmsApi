using Microsoft.EntityFrameworkCore;
using Josi_TmsApi.Data;
using Josi_TmsApi.Entities;
using Josi_TmsApi.Dtos;

namespace Josi_TmsApi.Services;

// public class CourseService(
//     TmsDb1Context context,
//     ILogger<CourseService> logger) : ICourseService
// {
//     public async Task<Course?> GetByIdAsync(int id, CancellationToken ct)
//     {
//         return await context.Courses
//             .AsNoTracking()
//             .FirstOrDefaultAsync(c => c.Id == id, ct);
//     }

//               //GetByIdAsync

//                        // Uses AsNoTracking() because this is a read-only operation.
//                           // Searches by Id.
//                            // Returns null if the course doesn't exist.

//     public async Task<Course> CreateAsync(Course course, CancellationToken ct)
//     {
//         context.Courses.Add(course);

//         await context.SaveChangesAsync(ct);

//         logger.LogInformation(
//             "Course {Code} created with ID {Id}",
//             course.Code,
//             course.Id);

//         return course;
//     }
//            //     CreateAsync

//               // Adds the new course.
//               // Saves changes to the database.
//               // Logs the creation.
//               // Returns the saved entity.


//     // To  Addthe duplicate-code check to the service
//     public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
// context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);
// }

//Updated the course details
public class CourseService(
    TmsDb1Context context,
    ILogger<CourseService> logger) : ICourseService
{
    public Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct) =>
        context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);

    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        context.Courses.Add(course);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created course {CourseId} ({Code})",
            course.Id,
            course.Code);

        return (await GetByIdAsync(course.Id, ct))!;
    }
}