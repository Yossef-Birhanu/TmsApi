using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;

/// <summary>
/// Provides the database implementation for enrollment operations.
///
/// The interface IEnrollmentService is defined in the Application layer.
/// This class implements that interface and uses Entity Framework Core
/// to communicate with the database.
/// </summary>
public class EnrollmentService(
    TmsDb1Context context,
    ILogger<EnrollmentService> logger) : IEnrollmentService
{
    // ============================================================
    // GET ENROLLMENT BY ID
    // ============================================================
    // Gets a single enrollment using:
    //     1. Course ID
    //     2. Enrollment ID
    //
    // Returns null if the enrollment does not exist.
    // ============================================================

    public Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct)
    {
        return context.Enrollments
            .AsNoTracking()
            .Where(e =>
                e.Id == id &&
                e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .FirstOrDefaultAsync(ct);
    }


    // ============================================================
    // CREATE ENROLLMENT
    // ============================================================
    // Creates a new enrollment for a student in a course.
    // The new enrollment is saved to the database.
    // ============================================================

    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // --------------------------------------------------------
        // Check whether the student is already enrolled.
        // --------------------------------------------------------

        var alreadyExists = await context.Enrollments
            .AnyAsync(
                e =>
                    e.CourseId == courseId &&
                    e.StudentId == request.StudentId,
                ct);

        // --------------------------------------------------------
        // Stop the operation if a duplicate enrollment exists.
        // --------------------------------------------------------

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "The student is already enrolled in this course.");
        }

        // --------------------------------------------------------
        // Create a new Enrollment entity.
        // --------------------------------------------------------

        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        // --------------------------------------------------------
        // Add the enrollment to the Entity Framework context.
        // --------------------------------------------------------

        context.Enrollments.Add(enrollment);

        // --------------------------------------------------------
        // Save the enrollment to the database.
        // --------------------------------------------------------

        await context.SaveChangesAsync(ct);

        // --------------------------------------------------------
        // Log the successful enrollment.
        // --------------------------------------------------------

        logger.LogInformation(
            "Student {StudentId} enrolled in course {CourseId}. Enrollment ID: {EnrollmentId}",
            enrollment.StudentId,
            enrollment.CourseId,
            enrollment.Id);

        // --------------------------------------------------------
        // Return the created enrollment as a DTO.
        // --------------------------------------------------------

        return new EnrollmentResponseDto(
            enrollment.Id,
            enrollment.CourseId,
            enrollment.StudentId,
            enrollment.EnrolledAt);
    }


    // ============================================================
    // ENROLL STUDENT USING STUDENT ID AND COURSE CODE
    // ============================================================
    // Finds a course using its course code and enrolls the student.
    //
    // studentId is already an integer, so there is NO need to use
    // int.TryParse().
    // ============================================================

    public async Task<Enrollment> EnrollAsync(
        int studentId,
        string courseCode)
    {
        // --------------------------------------------------------
        // Find the course using its course code.
        // --------------------------------------------------------

        var course = await context.Courses
            .FirstOrDefaultAsync(c => c.Code == courseCode);

        // --------------------------------------------------------
        // Stop if the course does not exist.
        // --------------------------------------------------------

        if (course is null)
        {
            throw new InvalidOperationException(
                $"Course with code '{courseCode}' was not found.");
        }

        // --------------------------------------------------------
        // Check whether the student is already enrolled.
        // --------------------------------------------------------

        var existingEnrollment = await context.Enrollments
            .FirstOrDefaultAsync(
                e =>
                    e.StudentId == studentId &&
                    e.CourseId == course.Id);

        // --------------------------------------------------------
        // Return the existing enrollment instead of creating
        // a duplicate enrollment.
        // --------------------------------------------------------

        if (existingEnrollment is not null)
        {
            logger.LogWarning(
                "Duplicate enrollment attempt. Student {StudentId} is already enrolled in course {CourseCode}.",
                studentId,
                courseCode);

            return existingEnrollment;
        }

        // --------------------------------------------------------
        // Create a new enrollment.
        // --------------------------------------------------------

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow
        };

        // --------------------------------------------------------
        // Add the enrollment to the database context.
        // --------------------------------------------------------

        context.Enrollments.Add(enrollment);

        // --------------------------------------------------------
        // Save the enrollment to the database.
        // --------------------------------------------------------

        await context.SaveChangesAsync();

        // --------------------------------------------------------
        // Log the successful enrollment.
        // --------------------------------------------------------

        logger.LogInformation(
            "Student {StudentId} enrolled in course {CourseCode}. Enrollment ID: {EnrollmentId}",
            studentId,
            courseCode,
            enrollment.Id);

        return enrollment;
    }


    // ============================================================
    // GET ALL ENROLLMENTS
    // ============================================================
    // Retrieves all enrollments from the database.
    // ============================================================

    public async Task<IReadOnlyList<Enrollment>> GetAllAsync(
        CancellationToken ct = default)
    {
        // --------------------------------------------------------
        // AsNoTracking improves performance for read-only queries.
        // --------------------------------------------------------

        var enrollments = await context.Enrollments
            .AsNoTracking()
            .ToListAsync(ct);

        // --------------------------------------------------------
        // Write information to the application log.
        // --------------------------------------------------------

        logger.LogInformation(
            "Retrieved {Count} enrollment records.",
            enrollments.Count);

        return enrollments;
    }


    // ============================================================
    // DELETE ENROLLMENT
    // ============================================================
    // Deletes an enrollment using its ID.
    //
    // Returns:
    //     true  = enrollment was deleted
    //     false = enrollment was not found
    // ============================================================

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        // --------------------------------------------------------
        // Find the enrollment by ID.
        // --------------------------------------------------------

        var enrollment = await context.Enrollments
            .FirstOrDefaultAsync(
                e => e.Id == id,
                ct);

        // --------------------------------------------------------
        // Return false if the enrollment does not exist.
        // --------------------------------------------------------

        if (enrollment is null)
        {
            logger.LogWarning(
                "Enrollment {EnrollmentId} was not found.",
                id);

            return false;
        }

        // --------------------------------------------------------
        // Remove the enrollment from the database context.
        // --------------------------------------------------------

        context.Enrollments.Remove(enrollment);

        // --------------------------------------------------------
        // Save the deletion.
        // --------------------------------------------------------

        await context.SaveChangesAsync(ct);

        // --------------------------------------------------------
        // Log the successful deletion.
        // --------------------------------------------------------

        logger.LogInformation(
            "Enrollment {EnrollmentId} was deleted.",
            id);

        return true;
    }


    // ============================================================
    // GET ENROLLMENTS BY COURSE
    // ============================================================
    // Retrieves all enrollments belonging to a specific course.
    // ============================================================

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .ToListAsync(ct);
    }


    // ============================================================
    // CHECK WHETHER ENROLLMENT EXISTS
    // ============================================================
    // Checks whether a student is already enrolled in a course.
    //
    // The course is identified using its course code.
    // ============================================================

    public async Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken ct)
    {
        // --------------------------------------------------------
        // Find the course ID using the course code.
        // --------------------------------------------------------

        var courseId = await context.Courses
            .Where(c => c.Code == courseCode)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(ct);

        // --------------------------------------------------------
        // If the course does not exist, there cannot be an
        // enrollment for it.
        // --------------------------------------------------------

        if (courseId is null)
        {
            return false;
        }

        // --------------------------------------------------------
        // Check whether the student is enrolled in the course.
        // --------------------------------------------------------

        return await context.Enrollments
            .AnyAsync(
                e =>
                    e.StudentId == studentId &&
                    e.CourseId == courseId.Value,
                ct);
    }


    // ============================================================
    // ADD ENROLLMENT
    // ============================================================
    // Adds an existing Enrollment entity to the database.
    // ============================================================

    public async Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct)
    {
        // Add the entity to Entity Framework.
        context.Enrollments.Add(enrollment);

        // Save the entity to the database.
        await context.SaveChangesAsync(ct);

        // Log the operation.
        logger.LogInformation(
            "Enrollment {EnrollmentId} was added.",
            enrollment.Id);
    }


    // ============================================================
    // GET ENROLLMENTS BY STUDENT
    // ============================================================
    // Retrieves all enrollments belonging to a particular student.
    // ============================================================

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .ToListAsync(ct);
    }
    public async Task<bool> ApproveAsync(
    int id,
    CancellationToken ct)
{
    var enrollment = await context.Enrollments
        .FirstOrDefaultAsync(e => e.Id == id, ct);

    if (enrollment is null)
    {
        return false;
    }

    enrollment.Status = EnrollmentStatus.Approved;

    await context.SaveChangesAsync(ct);

    return true;
}
}