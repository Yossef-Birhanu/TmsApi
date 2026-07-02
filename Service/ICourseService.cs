using Josi_TmsApi.Dtos;

namespace Josi_TmsApi.Services;

public interface ICourseService
{
    // Task<Course?> GetByIdAsync(int id, CancellationToken ct);

    // Task<Course> CreateAsync(Course course, CancellationToken ct);

    // //To Add the duplicate check services 
    // Task<bool> CodeExistsAsync(string code, CancellationToken ct);

    //Update the course details

     Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct);

    Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct);
}