using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
using Josi_TmsApi.Data;

namespace TmsApi.Controllers;

=======
using System;
using System.Linq;
using Josi_TmsApi.Data;
namespace TmsApi.Controllers;
>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
[ApiController]
[Route("api/test")]
public class TestController(TmsDb1Context context) : ControllerBase
{
<<<<<<< HEAD
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (no database contact)...");

        var query = context.Students.Where(s => s.GPA >= 3.0m);

        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");

        var orderedQuery = query.OrderBy(s => s.Name);

        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");

        var results = orderedQuery.ToList(); // SQL executes here

        Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");

        return Ok(results);
    }

    // Non-translatable helper method
    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");

        try
        {
            var students = context.Students
                .Where(s => IsHonorRoll(s.GPA))
                .ToList();

            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");

            return BadRequest(new
            {
                Message = ex.Message
            });
        }
    }

    // Exercise 3 - Pagination
    [HttpGet("students")]
    public async Task<IActionResult> GetStudentsPageAsync(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        var students = await context.Students
            .OrderBy(s => s.Name)                // Required before Skip/Take
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(students);
    }

    // Exercise 3 - Top 5 Courses by Enrollment
    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopCoursesAsync(
        CancellationToken cancellationToken = default)
    {
        var topCourses = await context.Enrollments
            .GroupBy(e => new
            {
                e.CourseId,
                e.Course.Title
            })
            .Select(g => new
            {
                CourseTitle = g.Key.Title,
                EnrollmentCount = g.Count()
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return Ok(topCourses);
    }
}

=======
[HttpGet("deferred")]
public IActionResult TestDeferred()
{
Console.WriteLine("\n>>> STEP 1: Building the query object (nodatabase contact)...");
var query = context.Students.Where(s => s.GPA >= 3.0m);
Console.WriteLine(">>> STEP 2: Appending a sorting clause...");var orderedQuery = query.OrderBy(s => s.Name);
Console.WriteLine(">>> STEP 3: Materializing query into a C#List...");
var results = orderedQuery.ToList(); // Execution is triggeredhere
Console.WriteLine(">>> STEP 4: Materialization finished. Listpopulated.\n");
return Ok(results);
}

// Non-translatable helper method
private static bool IsHonorRoll(decimal GPA)
{
return GPA >= 3.5m;
}
[HttpGet("translation-fail")]
public IActionResult TestTranslationFail()
{
Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
try
{
var students = context.Students
.Where(s => IsHonorRoll(s.GPA)) // EF Core does not know how to map this method to SQL
.ToList();
return Ok(students);
}
catch (Exception ex)
{
Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");return BadRequest(new { Message = ex.Message });
}
}
}





>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
