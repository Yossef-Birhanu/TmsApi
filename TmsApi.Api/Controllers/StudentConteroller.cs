using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Api.Controllers;
[ApiController]
[Route("api/Students")]
public class StudentController(IStudentService StudentService) : ControllerBase
{
    [HttpGet("{id:int}", Name =nameof(GetStudentById))]
public async Task<IActionResult> GetStudentById(int id, CancellationToken ct)
    {
        var student=await StudentService.GetByIdAsync(id, ct);
        if(student is null)
        {
            return NotFound();
        }
        return Ok(student);
        throw new NotImplementedException();

    }
[HttpPost]
public async Task<IActionResult> CreateStudent(Student student, CancellationToken ct)
    {
        var createdStudent= await StudentService.CreateAsync(student,ct);
        return CreatedAtAction(nameof(GetStudentById),new{ id=createdStudent.Id},createdStudent);
    throw new NotImplementedException();
    }
}