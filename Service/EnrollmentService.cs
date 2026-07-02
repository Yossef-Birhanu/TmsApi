<<<<<<< HEAD
// public class EnrollmentService : IEnrollmentService
// {
//     private readonly Dictionary<string, EnrollmentRecord> _store = new();
//     private readonly ILogger<EnrollmentService> _logger;

//     public EnrollmentService(ILogger<EnrollmentService> logger)
//     {
//         _logger = logger;
//     }

//     public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
//     {
//         // Check for duplicate enrollment
//         var existing = _store.Values
//             .FirstOrDefault(e =>
//                 e.StudentId == studentId &&
//                 e.CourseCode == courseCode);

//         if (existing is not null)
//         {
//             _logger.LogWarning(
//                 "Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})",
//                 studentId,
//                 courseCode,
//                 existing.Id);

//             return Task.FromResult(existing);
//         }

//         var id = Guid.NewGuid().ToString("N")[..8];

//         var record = new EnrollmentRecord(          
//             id,
//             studentId,
//             courseCode,
//             DateTime.UtcNow);

//         _store[id] = record;

//         _logger.LogInformation(
//             "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
//             studentId,
//             courseCode,
//             id);

//         return Task.FromResult(record);
//     }

//     public Task<EnrollmentRecord?> GetByIdAsync(string id)
//     {
//         _store.TryGetValue(id, out var record);

//         if (record is null)
//         {
//             _logger.LogWarning(
//                 "Enrollment {EnrollmentId} not found",id);
//         }

//         return Task.FromResult(record);
//     }

//     public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
//     {
//         IReadOnlyList<EnrollmentRecord> all = _store.Values.ToList();

//         _logger.LogInformation(
//             "Retrieved {Count} enrollments",
//             all.Count);

//         return Task.FromResult(all);
//     }

//     public Task<bool> DeleteAsync(string id)
//     {
//         var removed = _store.Remove(id);

//         if (removed)
//         {
//             _logger.LogInformation(
//                 "Deleted enrollment {EnrollmentId}",id);
//         }
//         else
//         {
//             _logger.LogWarning(
//                 "Delete failed, enrollment {EnrollmentId} not found", id);
//         }

//         return Task.FromResult(removed);
//     }
// }

// public interface IEnrollmentService
// {
//     Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
//     Task<EnrollmentRecord?> GetByIdAsync(string id);
//     Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
//     Task<bool> DeleteAsync(string id);
// }

// public class EnrollmentRecord(string id, string studentId, string courseCode, DateTime utcNow)
// {
//     private string id = id;
//     private string studentId = studentId;
//     private string courseCode = courseCode;
//     private DateTime utcNow = utcNow;

//     public string? StudentId { get; set; }
//     public string? CourseCode { get; set; }
//     public object[]? Id { get;  set; }
// }



//Updated IEnrollmentService.cs
using Microsoft.EntityFrameworkCore;
using Josi_TmsApi.Data;
using Josi_TmsApi.Dtos;
using Josi_TmsApi.Entities;
using Tms.Api.Services;

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
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Student {StudentId} enrolled in Course {CourseId}",
            request.StudentId,
            courseId);

        return (await GetByIdAsync(
            courseId,
            enrollment.Id,
            ct))!;
    }
}
=======
public class EnrollmentService : IEnrollmentService
{
    private readonly Dictionary<string, EnrollmentRecord> _store = new();
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ILogger<EnrollmentService> logger)
    {
        _logger = logger;
    }

    public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        // Check for duplicate enrollment
        var existing = _store.Values
            .FirstOrDefault(e =>
                e.StudentId == studentId &&
                e.CourseCode == courseCode);

        if (existing is not null)
        {
            _logger.LogWarning(
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

        _logger.LogInformation(
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
            _logger.LogWarning(
                "Enrollment {EnrollmentId} not found",id);
        }

        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        IReadOnlyList<EnrollmentRecord> all = _store.Values.ToList();

        _logger.LogInformation(
            "Retrieved {Count} enrollments",
            all.Count);

        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);

        if (removed)
        {
            _logger.LogInformation(
                "Deleted enrollment {EnrollmentId}",id);
        }
        else
        {
            _logger.LogWarning(
                "Delete failed, enrollment {EnrollmentId} not found", id);
        }

        return Task.FromResult(removed);
    }
}

public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
    Task<EnrollmentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
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




>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
