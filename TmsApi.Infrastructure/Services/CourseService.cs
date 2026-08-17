using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;

// ============================================================
// COURSE SERVICE
// ============================================================
//
// This service contains the database operations related to
// Course entities.
//
// TmsDb1Context:
//   Used to communicate with the PostgreSQL database.
//
// ILogger<CourseService>:
//   Used to write information, warning, and error messages.
//
// ICourseService:
//   Interface that CourseService implements.
// ============================================================

public class CourseService(
    TmsDb1Context context,
    ILogger<CourseService> logger) : ICourseService
{
    // ============================================================
    // GET COURSE BY ID
    // ============================================================
    //
    // Searches for a course using its ID.
    //
    // AsNoTracking():
    //   Improves read-only queries because EF Core does not track
    //   the returned entity.
    //
    // Returns:
    //   Course if found.
    //   null if no course exists with the specified ID.
    // ============================================================

    // public async Task<Course?> GetByIdAsync(
    //     int id,
    //     CancellationToken ct)
    // {
    //     return await context.Courses
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(c => c.Id == id, ct);
    // }
public async Task<CourseResponseDto?> GetByIdAsync(
    int id,
    CancellationToken ct)
{
    return await context.Courses
        .AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => new CourseResponseDto(
            c.Id,
            c.Code,
            c.Title,
            c.MaxCapacity,
            c.Enrollments.Count
        ))
        .FirstOrDefaultAsync(ct);
}

    // ============================================================
    // CREATE COURSE
    // ============================================================
    //
    // Adds a new course to the database and saves the changes.
    // ============================================================

    // public async Task<Course> CreateAsync(
    //     Course course,
    //     CancellationToken ct)
    // {
    //     context.Courses.Add(course);

    //     // Save the new course to the database.
    //     await context.SaveChangesAsync(ct);

    //     return course;
    // }
public async Task<CourseResponseDto> CreateAsync(
    CreateCourseRequest request,
    CancellationToken ct)
{
    var course = new Course
    {
        Code = request.Code,
        Title = request.Title,
        MaxCapacity = request.MaxCapacity
    };

    context.Courses.Add(course);

    await context.SaveChangesAsync(ct);

    return new CourseResponseDto(
        course.Id,
        course.Code,
        course.Title,
        course.MaxCapacity,
        0
    );
}

    // ============================================================
    // CHECK WHETHER COURSE CODE EXISTS
    // ============================================================
    //
    // Returns true if the specified course code already exists.
    //
    // This can be used before creating a course to prevent
    // duplicate course codes.
    // ============================================================

    public Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct) =>
        context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Code == code, ct);


    // ============================================================
    // GET COURSES WITH PAGINATION, SEARCHING AND SORTING
    // ============================================================
    //
    // Supports:
    //   1. Searching by course title or code
    //   2. Sorting by Code, Title or MaxCapacity
    //   3. Ascending or descending order
    //   4. Pagination
    // ============================================================

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        // --------------------------------------------------------
        // Step 1: Start with a read-only query.
        // --------------------------------------------------------

        IQueryable<Course> query = context.Courses
            .AsNoTracking();


        // --------------------------------------------------------
        // Step 2: Apply search filter.
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(
                    c.Title,
                    $"%{request.Search}%") ||

                EF.Functions.ILike(
                    c.Code,
                    $"%{request.Search}%"));
        }


        // --------------------------------------------------------
        // Step 3: Count records BEFORE pagination.
        //
        // This gives us the total number of matching courses.
        // --------------------------------------------------------

        var totalCount = await query.CountAsync(ct);


        // --------------------------------------------------------
        // Step 4: Apply sorting.
        // --------------------------------------------------------

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


        // --------------------------------------------------------
        // Step 5: Apply pagination and convert Course entities
        //         into CourseResponseDto objects.
        // --------------------------------------------------------

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


        // --------------------------------------------------------
        // Step 6: Return the paginated response.
        // --------------------------------------------------------

        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }


    // ============================================================
    // GET ALL COURSES
    // ============================================================
    //
    // Returns all courses together with their enrollments.
    // ============================================================

    // public async Task<List<Course>> GetAllAsync(
    //     CancellationToken ct)
    // {
    //     return await context.Courses
    //         .Include(c => c.Enrollments)
    //         .AsNoTracking()
    //         .ToListAsync(ct);
    // }
public async Task<IEnumerable<Course>> GetAllAsync(
    CancellationToken ct)
{
    return await context.Courses
        .Include(c => c.Enrollments)
        .AsNoTracking()
        .ToListAsync(ct);
}

    // ============================================================
    // GET COURSE BY CODE
    // ============================================================
    //
    // Searches for one course using its course code.
    // ============================================================

    public async Task<Course?> GetByCodeAsync(
        string courseCode,
        CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Code == courseCode,
                ct);
    }


    // ============================================================
    // UPDATE COURSE
    // ============================================================
    //
    // Finds the course by ID and updates its properties.
    //
    // NOTE:
    // The exact properties depend on your UpdateCourseCommand.
    // ============================================================

    public async Task<bool> UpdateAsync(
        UpdateCourseCommand command,
        CancellationToken ct)
    {
        var course = await context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == command.Id,
                ct);

        if (course is null)
        {
            return false;
        }

        // Update the course properties.
        course.Code = command.Code;
        course.Title = command.Title;
        course.MaxCapacity = command.MaxCapacity;

        await context.SaveChangesAsync(ct);

        return true;
    }
}