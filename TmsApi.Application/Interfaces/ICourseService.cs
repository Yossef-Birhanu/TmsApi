// using TmsApi.Application.Courses.Commands;
// using TmsApi.Application.DTOs;
// using TmsApi.Domain.Entities;

// namespace TmsApi.Application.Interfaces;
// public interface ICourseService
// {
// // Task<Course?> GetByIdAsync(int id, CancellationToken ct);
// // Task<Course> CreateAsync(Course course, CancellationToken ct);
// ////Excercise 2: Update the ICourseService interface to use DTOs instead of the Course entity. 
// Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
// Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);


// Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
//  //Exercise 3: Extend IcourseService to add a method to check if a course code already exists in the database.
//       Task<bool> CodeExistsAsync(string code, CancellationToken ct);
   
//     Task<Course?> GetByCodeAsync(string courseCode, CancellationToken ct);
//     // Task<bool> UpdateAsync(UpdateCourseCommand command, CancellationToken ct);

//    Task<bool> UpdateAsync( UpdateCourseCommand command,CancellationToken ct);
//        Task<IEnumerable<Course>> GetAllAsync(CancellationToken ct);
//     Task<bool> UpdateAsync(int id, UpdateCourseRequest request, CancellationToken ct);
// }


using TmsApi.Application.Courses.Commands;
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct);

    Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct);

    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct);

    Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct);

    Task<Course?> GetByCodeAsync(
        string courseCode,
        CancellationToken ct);

    Task<IEnumerable<Course>> GetAllAsync(
        CancellationToken ct);

    Task<bool> UpdateAsync(
        int id,
        UpdateCourseRequest request,
        CancellationToken ct);
}