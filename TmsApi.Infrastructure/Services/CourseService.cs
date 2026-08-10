
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;
public class CourseService(TmsDb1Context context, ILogger<CourseService>logger): ICourseService
                              // <summary>
                              //context is the database context used to communicate with the database.
                              //logger is used to write log messages (errors, warnings, information).
                              //: ICourseService    Means CourseService implements the ICourseService interface.

{
    public async Task<Course?> GetByIdAsync(int id, CancellationToken ct)

//Task<Course?> Returns a Course. The ? means it may return null if no course is found.
//GetByIdAsync Method name. "Async" indicates asynchronous execution. (int id, CancellationToken ct)
// id is the course ID to search for.
//ct allows the operation to be cancelled.


    {
       context.Courses.AsNoTracking();
         return await context.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
         throw new NotImplementedException();


}
public async Task<Course> CreateAsync(Course course, CancellationToken ct)
    {
        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);
//await
// Waits until the database finishes executing the query.


        return course;
        throw new NotImplementedException();
    }

    Task<CourseResponseDto?> ICourseService.GetByIdAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<CourseResponseDto> CreateAsync(CreateCourseRequest Request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

   

    //Exercise 3:  Extend CourseService.cs to add a method to check if a course code already exists in the database.
    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);

         //Exercise 4: Implement the GetCoursesAsync method in CourseService.cs to support pagination, searching, and sorting.
    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
    {
       // Step 1: Start with a no-tracking query
    IQueryable<Course> query = context.Courses
        .AsNoTracking();

    // Step 2: Apply search filter if provided
    if (!string.IsNullOrWhiteSpace(request.Search))
    {
        query = query.Where(c =>
            EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
            EF.Functions.ILike(c.Code, $"%{request.Search}%"));
    }

    // Step 3: Count BEFORE pagination
    var totalCount = await query.CountAsync(ct);

    // Step 4: Apply OrderBy
    IOrderedQueryable<Course> sortedQuery; 
     switch (request.Orderby)
    {
        case "Code":
            sortedQuery = request.Descending
                ? query.OrderByDescending(c => c.Code)
                : query.OrderBy(c => c.Code);
            break;

        case "MaxCapacity":
            sortedQuery = request.Descending
                ? query.OrderByDescending(c => c.MaxCapacity)
                : query.OrderBy(c => c.MaxCapacity);
            break;

        case "Title":
        default:
            sortedQuery = request.Descending
                ? query.OrderByDescending(c => c.Title)
                : query.OrderBy(c => c.Title);
            break;
    }

     // Step 5: Apply paging and project to DTO
    var items = await sortedQuery
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Select(c => new CourseResponseDto(
            c.Id,
            c.Code,
            c.Title,
            c.MaxCapacity,
            c.Enrollments.Count
        ))
        .ToListAsync(ct);

    // Step 6: Return paged response
    return new PagedResponse<CourseResponseDto>
    {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize
    };
    throw new NotImplementedException();
    }

public async Task<List<Course>> GetAllAsync(CancellationToken ct)
{
    return await context.Courses
        .Include(c => c.Enrollments)
        .ToListAsync(ct);
}
    public Task<Course?> GetByCodeAsync(string courseCode, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(UpdateCourseCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    Task<IEnumerable<Course>> ICourseService.GetAllAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
