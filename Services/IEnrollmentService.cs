using Josi_TmsApi.Dtos;

namespace Josi_TmsApi.Services;
public interface IEnrollmentService
{
Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
    int courseId,
    CancellationToken ct);

Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
Task<bool> DeleteAsync(string id);
}