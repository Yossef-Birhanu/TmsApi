
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;
public interface IStudentService
{
    Task<Student?> GetByIdAsync(int id, CancellationToken ct);
    Task<Student> CreateAsync(Student student, CancellationToken ct);
    
}