using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class UpdateCourseHandler(
    ICourseService service,
    ICachedCourseService cachedService)
    : IRequestHandler<UpdateCourseCommand, bool>
{
    public async Task<bool> Handle(
        UpdateCourseCommand command,
        CancellationToken ct)
    {
        var request = new UpdateCourseRequest
        {
            Code = command.Code,
            Title = command.Title,
            Description = command.Description
        };

        var updated = await service.UpdateAsync(
            command.Id,
            request,
            ct);

        if (!updated)
            return false;

        // Remove stale cache entries
        await cachedService.InvalidateCourseCacheAsync(ct);

        return true;
    }
}