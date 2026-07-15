using Josi_TmsApi.Data;
using Josi_TmsApi.Dtos;
using Josi_TmsApi.Entities;
using Microsoft.EntityFrameworkCore;
namespace Josi_TmsApi.Services;
public class EnrollmentService(
    TmsDb1Context context,
    ILogger<EnrollmentService> logger) : IEnrollmentService
    {
    public Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .FirstOrDefaultAsync(ct);
    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // Create a new enrollment
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };
    // Save to database
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        return new EnrollmentResponseDto(
            enrollment.Id,
            enrollment.CourseId,
            enrollment.StudentId,
            enrollment.EnrolledAt);
    }

  

    private readonly Dictionary<string, EnrollmentRecord> _store = new();
// private readonly ILogger<EnrollmentService> _logger;

    // public EnrollmentService(ILogger<EnrollmentService> logger)
    // {
    //     _logger = logger;
    // }

    public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        // Check for duplicate enrollment
        var existing = _store.Values
            .FirstOrDefault(e =>
                e.StudentId == studentId &&
                e.CourseCode == courseCode);

        if (existing is not null)
        {
            logger.LogWarning(
                "Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})",
                studentId,
                courseCode,
                existing.Id);

            return Task.FromResult(existing);
        }

        var id = Guid.NewGuid().ToString("N")[..8];

        var record = new EnrollmentRecord(          
            id,
            studentId,
            courseCode,
            DateTime.UtcNow);

        _store[id] = record;

        logger.LogInformation(
            "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
            studentId,
            courseCode,
            id);

        return Task.FromResult(record);
    }

    public Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);

        if (record is null)
        {
            logger.LogWarning(
                "Enrollment {EnrollmentId} not found",id);
        }

        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        IReadOnlyList<EnrollmentRecord> all = _store.Values.ToList();

        logger.LogInformation(
            "Retrieved {Count} enrollments",
            all.Count);

        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);

        if (removed)
        {
            logger.LogInformation(
                "Deleted enrollment {EnrollmentId}",id);
        }
        else
        {
            logger.LogWarning(
                "Delete failed, enrollment {EnrollmentId} not found", id);
        }

        return Task.FromResult(removed);
    }

    public Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
    CancellationToken ct) =>
    context.Enrollments
        .AsNoTracking()
        .Where(e => e.CourseId == courseId)
        .Select(e => new EnrollmentResponseDto(
            e.Id,
            e.CourseId,
            e.StudentId,
            e.EnrolledAt))
        .ToListAsync(ct)
        .ContinueWith(
            t => (IReadOnlyList<EnrollmentResponseDto>)t.Result,
            ct);

}


public class EnrollmentRecord(string id, string studentId, string courseCode, DateTime utcNow)
{
    private string id = id;
    private string studentId = studentId;
    private string courseCode = courseCode;
    private DateTime utcNow = utcNow;

    public string? StudentId { get; set; }
    public string? CourseCode { get; set; }
    public object[]? Id { get;  set; }
}




