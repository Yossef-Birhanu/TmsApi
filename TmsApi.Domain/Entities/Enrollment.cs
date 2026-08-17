using System;

namespace TmsApi.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public decimal? Grade { get; set; }
    // Nullable because the student may still be enrolled
    // and may not have received a grade yet.

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
    // New enrollment starts with Pending status.

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Navigation properties back to entities
    public Student Student { get; set; } = null!;

    public Course Course { get; set; } = null!;
}
