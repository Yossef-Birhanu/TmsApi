using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identity;
namespace TmsApi.Infrastructure.Persistence;
public class TmsDb1Context:IdentityDbContext<TmsUser>
{
    public TmsDb1Context(DbContextOptions<TmsDb1Context> options) : base(options)
    {
        
    }
    
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<Assessment> Assessments => Set<Assessment>();
        public DbSet<Certificate> Certificates => Set<Certificate>();
        public DbSet<RefreshToken>RefreshTokens{get; set;}
    

   
}