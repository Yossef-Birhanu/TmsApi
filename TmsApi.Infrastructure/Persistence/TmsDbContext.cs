using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
namespace TmsApi.Infrastructure.Persistence;
public class TmsDb1Context(DbContextOptions<TmsDb1Context> options) : DbContext(options)
{
public DbSet<Student> Students => Set<Student>();
public DbSet<Course> Courses => Set<Course>();
public DbSet<Enrollment> Enrollments => Set<Enrollment>();

public DbSet<Assessment> Assessments => Set<Assessment>();
public DbSet<Certificate> Certificates => Set<Certificate>();
}

