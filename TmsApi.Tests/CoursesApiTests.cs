using System.Net;
using System.Net.Http.Json;

namespace TmsApi.Tests;
public class CoursesApiTests: IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public CoursesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    [Fact]
    public async Task GetCourses_ReturnsOkAndPagedJson()
    {
        //act - pin the v2 URL (see versioning callout below)
        var response = await _client.GetAsync("/api/v1/courses?page=1&pageSize=10");
        //assert - check HTTP status 200 OK
        response.EnsureSuccessStatusCode();
        //TMS API contract check : pagedResponse<T> with items array
        var page=await response.Content.ReadFromJsonAsync<PagedCoursesJson>();
        Assert.NotNull(page?.Items);

    }
    [Fact]
    public async Task CreateCourse_InvalidCode_ReturnsValidationError()
    {
        //Act -past invalid payload (empty code )to the v2 controller.
        var response = await _client.PostAsJsonAsync("/api/v1/courses", new { Code = "", Title = "Intro to TMS Security",maxCapacity=30 });
        //Assert -validation failure returns 400 bad request or 422 unprocessable entity
        Assert.True(response.StatusCode is HttpStatusCode.BadRequest || response.StatusCode is HttpStatusCode.UnprocessableEntity);

    }
    private sealed class PagedCoursesJson
    {
        public List<CourseRowJson> Items { get; set; } = default!;
        public int TotalCount{get; set;}
    }

    private sealed class CourseRowJson
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Title {get; set;}="";
        public int MaxCapacity { get; set; }
        public int EnrolledCount { get; set; }
    }
}