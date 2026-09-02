namespace TmsApi.Application.DTOs;

public class UpdateCourseRequest
{
    public string Title { get; set; } = string.Empty;

    public int MaxCapacity { get; set; }
    public string Code { get; set; }
    public object Description { get; internal set; }
}