using Microsoft.EntityFrameworkCore;
using Josi_TmsApi.Entities;
namespace Josi_TmsApi.Data;
public class TmsDb1Context(DbContextOptions<TmsDb1Context> options) : DbContext(options)
{
public DbSet<Student> Students => Set<Student>();
public DbSet<Course> Courses => Set<Course>();
public DbSet<Enrollment> Enrollments => Set<Enrollment>();

public DbSet<Assessment> Assessments => Set<Assessment>();
public DbSet<Certificate> Certificates => Set<Certificate>();
<<<<<<< HEAD
}

=======
}
>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
