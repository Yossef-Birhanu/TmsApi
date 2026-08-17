using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;
public class StudentService(TmsDb1Context context, ILogger<StudentService>logger ) :IStudentService
{
    public async Task<Student?> GetByIdAsync(int id, CancellationToken ct)
    {
        context.Students.AsNoTracking();
        return await context.Students.FirstOrDefaultAsync(c=>c.Id==id,ct);
        throw new NotImplementedException();
    }
    public async Task<Student> CreateAsync(Student student, CancellationToken ct)
    {
      context.Students.Add(student);
      await context.SaveChangesAsync(ct);  
      return student;
      throw new NotImplementedException();
    }
}