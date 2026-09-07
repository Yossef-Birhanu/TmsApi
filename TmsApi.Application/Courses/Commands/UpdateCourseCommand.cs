using MediatR;

namespace TmsApi.Application.Courses.Commands;

public record UpdateCourseCommand(
    int Id,
    string Code,
    string Title,
    string Description
) : IRequest<bool>;