using Asp.Versioning;
using Josi_TmsApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Josi_TmsApi.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/Courses")]
[ApiVersion("2.0")]

public class CoursesController(TmsDb1Context context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult>GetCourses(
        [FromQuery] int page=1,
        [FromQuery] int pageSize=20,
        CancellationToken ct =default
    )
    {
        page=Math.Max(1,page);
        pageSize=Math.Clamp(pageSize,1,50);
        var baseQuery= context.Courses.AsNoTracking();
        var totalCount= await baseQuery.CountAsync(ct);

        var rows =  await baseQuery
        .OrderBy(c=>c.Title)
        .Skip((page-1)*pageSize)
        .Take(pageSize)
        .Select(c => new
        {
            c.Id,
            c.Title,
            c.Code,
            c.MaxCapacity,
            EnrollmentCount=c.Enrollments.Count
        }).ToListAsync(ct);
        var totalPages=(int)Math.Ceiling(totalCount/(double)pageSize);
        var hasNext=page<totalPages;
        var hasPrevious =page>1;
        return Ok(new
        {
            data=rows,
            meta = new
            {
                totalCount,
                page,
                pageSize,
                totalPages,
                hasNext,
                hasPrevious
            },
            links = new
            {
                self=$"/api/V2/Courses?page={page}&pageSize={pageSize}",
                next=hasNext?$"api/V2/Courses?page={page+1}&pageSize={pageSize}":(string?)null,
                prev=hasPrevious?$"api/V2/Courses?page={page-1}&pageSize={pageSize}":(string?)null,
                enroll="/api/V2/enrollments"
            }
        });
    }
}