using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

/// <summary>
/// Defines the business operations available for student enrollments.
/// The implementation of this interface should be placed in the
/// Infrastructure or Application layer according to the project architecture.
/// </summary>
public interface IEnrollmentService
{
    // ============================================================
    // GET ENROLLMENT BY ID
    // ============================================================
    // Gets a single enrollment using the course ID and enrollment ID.
    //
    // Returns:
    // - EnrollmentResponseDto when the enrollment exists.
    // - null when the enrollment is not found.
    // ============================================================
    Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct);


    // ============================================================
    // CREATE ENROLLMENT
    // ============================================================
    // Creates a new enrollment for a student in a specific course.
    //
    // courseId:
    //     The ID of the course.
    //
    // request:
    //     Contains the student enrollment information.
    //
    // ct:
    //     Used to cancel the asynchronous operation if necessary.
    // ============================================================
    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct);


    // ============================================================
    // ENROLL STUDENT
    // ============================================================
    // Enrolls a student using the student's ID and course code.
    //
    // The returned Enrollment represents the newly created
    // enrollment record.
    // ============================================================
    Task<Enrollment> EnrollAsync(
        int studentId,
        string courseCode);


    // ============================================================
    // GET ENROLLMENTS BY COURSE
    // ============================================================
    // Returns all enrollments that belong to a specific course.
    //
    // This is useful for displaying the enrollment list for
    // one course.
    // ============================================================
    Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct);


    // ============================================================
    // GET ALL ENROLLMENTS
    // ============================================================
    // Returns all enrollment records in the system.
    //
    // CancellationToken is optional so existing callers can call
    // GetAllAsync() without explicitly providing a token.
    // ============================================================
    Task<IReadOnlyList<Enrollment>> GetAllAsync(
        CancellationToken ct = default);


    // ============================================================
    // DELETE ENROLLMENT
    // ============================================================
    // Deletes an enrollment using its ID.
    //
    // Returns:
    // - true  -> enrollment was successfully deleted.
    // - false -> enrollment was not found or could not be deleted.
    // ============================================================
    Task<bool> DeleteAsync(
        int id,
        CancellationToken ct = default);


    // ============================================================
    // CHECK IF ENROLLMENT EXISTS
    // ============================================================
    // Checks whether a student is already enrolled in a course.
    //
    // This helps prevent duplicate enrollments.
    //
    // Returns:
    // - true  -> enrollment already exists.
    // - false -> student is not enrolled in the course.
    // ============================================================
    Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken ct);


    // ============================================================
    // ADD ENROLLMENT
    // ============================================================
    // Adds an Enrollment entity to the database/context.
    //
    // This method is useful when the Enrollment entity has already
    // been created and only needs to be added to persistence.
    // ============================================================
    Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct);


    // ============================================================
    // GET ENROLLMENTS BY STUDENT
    // ============================================================
    // Returns all enrollments belonging to a specific student.
    //
    // This can be used to display the courses in which a student
    // is currently enrolled.
    // ============================================================
    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct);
}