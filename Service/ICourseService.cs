//using Josi_TmsApi.Entities;
using Josi_TmsApi.Dtos;
using Tms.Api.Dtos;
namespace Josi_TmsApi.Services;

public interface ICourseService
{
    // Task<Course?>GetCourseAsync(int id, CancellationToken ct);
    // Task<Course> CreateAsync(Course course, CancellationToken ct);
    // Task<Course?> GetByIdAsync(int id, CancellationToken ct);

    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request,CancellationToken ct);
    
    //To add the duplicate -code check to the service
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);

}
