using Josi_TmsApi.Entities;
namespace Josi_TmsApi.Services;
public interface IStudentService
{
    Task<Student?> GetByIdAsync(int id, CancellationToken ct);
    Task<Student> CreateAsync(Student student, CancellationToken ct);
    
}