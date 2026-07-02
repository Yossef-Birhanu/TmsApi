using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Josi_TmsApi.Entities;

namespace Josi_TmsApi.Data.Configurations;

<<<<<<< HEAD
// public class CourseConfiguration : IEntityTypeConfiguration<Course>
// {
//     public void Configure(EntityTypeBuilder<Course> builder)
//     {
//         builder.ToTable("Courses");

//         builder.HasKey(c => c.Id);

//         builder.Property(c => c.Code)
//             .IsRequired()
//             .HasMaxLength(20);

//         builder.Property(c => c.Title)
//             .IsRequired()
//             .HasMaxLength(200);

//         builder.Property(c => c.Capacity)
//             .IsRequired();
//     }
// }

=======
>>>>>>> 70789a3ac36423b19b24234c54947951ff4298e8
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
<<<<<<< HEAD
=======
        builder.ToTable("Courses");

>>>>>>> 70789a3ac36423b19b24234c54947951ff4298e8
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
<<<<<<< HEAD
            .HasMaxLength(10);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId);
=======
            .HasMaxLength(20);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Capacity)
            .IsRequired();
>>>>>>> 70789a3ac36423b19b24234c54947951ff4298e8
    }
}