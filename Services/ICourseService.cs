using Josi_TmsApi.Data;
using Josi_TmsApi.Dtos;
using Josi_TmsApi.Entities;

namespace Josi_TmsApi.Services;
public interface ICourseService
{
// Task<Course?> GetByIdAsync(int id, CancellationToken ct);
// Task<Course> CreateAsync(Course course, CancellationToken ct);
////Excercise 2: Update the ICourseService interface to use DTOs instead of the Course entity. 
Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);


Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
 //Exercise 3: Extend IcourseService to add a method to check if a course code already exists in the database.
      Task<bool> CodeExistsAsync(string code, CancellationToken ct);
}