using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Josi_TmsApi.Data;

namespace Josi_TmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly TmsDb1Context _context;

        public ReportsController(TmsDb1Context context)
        {
            _context = context;
        }

        // 1. Active students with GPA >= 3.0
        [HttpGet("active-students-count")]
        public async Task<IActionResult> ActiveStudentsCount()
        {
            var count = await _context.Students
                .Where(s => s.IsActive && s.GPA >= 3.0m)
                .CountAsync();

            return Ok(count);
        }

        // 2. Courses with most enrollments
        [HttpGet("courses-by-enrollment")]
        public async Task<IActionResult> CoursesByEnrollment()
        {
            var list = await _context.Courses
                .Select(c => new
                {
                    c.Title,EnrollmentCount = c.Enrollments.Count
                })
                .OrderByDescending(x => x.EnrollmentCount)
                .ToListAsync();

            return Ok(list);
        }

        // 3. Average GPA per course
        [HttpGet("average-gpa-per-course")]
        public async Task<IActionResult> AverageGpaPerCourse()
        {
            var list = await _context.Enrollments
                .GroupBy(e => e.Course.Title)
                .Select(g => new
                {
                    Course = g.Key,
                    AverageGPA = g.Average(e => e.Student.GPA)
                })
                .ToListAsync();

            return Ok(list);
        }

        // 4. Students with zero enrollments
        [HttpGet("students-without-enrollments")]
        public async Task<IActionResult> StudentsWithoutEnrollments()
        {
            var list = await _context.Students
                .Where(s => !s.Enrollments.Any())
                .Select(s => s.Name)
                .ToListAsync();

            return Ok(list);
        }
    }
}