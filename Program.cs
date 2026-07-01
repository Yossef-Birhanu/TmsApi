using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Josi_TmsApi.Data;
using Josi_TmsApi.Entities;
<<<<<<< HEAD
using Josi_TmsApi.Services;
=======
>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();

// Authentication & Authorization
builder.Services.AddAuthentication("TrainingScheme")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("TrainingScheme", null);

//Services are registered in the DBContext, so we can use THIS CODE
 // Register TmsDbContext scoped for incoming HTTP requests
<<<<<<< HEAD

 builder.Services.AddProblemDetails();
 builder.Services.AddOpenApi();
=======
>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
builder.Services.AddDbContext<TmsDb1Context>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));

builder.Services.AddDbContext<TmsDb1Context>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
.LogTo(Console.WriteLine, LogLevel.Information) // Log SQLto output window
.EnableSensitiveDataLogging()); // Show parameters in querylogs (dev only)
<<<<<<< HEAD
 
builder.Services.AddControllers();
=======

>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
builder.Services.AddAuthorization();

// Host validation
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var app = builder.Build();

// Middleware order
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ===== ENDPOINTS =====

// secured endpoint
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

// controllers
app.MapControllers();

// worker endpoint
app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

// error test endpoint
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

// TODO1
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Development mode");
}

// TODO2
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// TODO3
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

//Seed test data at startup
using (var scope = app.Services.CreateScope())
{
  var context =scope.ServiceProvider.GetRequiredService<TmsDb1Context>();
  context.Database.Migrate(); // Apply any pending migrations migration history intact
  if (!context.Students.Any())
{
var students = new List<Student>
{
new() { RegistrationNumber = "TMS-2026-0001", Name = "AliceSmith", GPA = 3.8m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0002", Name = "BobJones", GPA = 2.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
new() { RegistrationNumber = "TMS-2026-0004", Name = "DianaPrince", GPA = 3.9m, IsActive = true },
<<<<<<< HEAD
new() { RegistrationNumber = "TMS-2026-0005", Name = "EvanWright", GPA = 2.5m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0006", Name = "Yossef C", GPA = 3.7m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0007", Name = "Yossef B", GPA = 3.8m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0008", Name = "Yosef Bir", GPA = 3.8m, IsActive = true }
=======
new() { RegistrationNumber = "TMS-2026-0005", Name = "EvanWright", GPA = 2.5m, IsActive = true }
>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
};
context.Students.AddRange(students);
var courses = new List<Course>
{
<<<<<<< HEAD
new() { Code = "CS-101", Title = "Introduction to ComputerScience", MaxCapacity = 30 },
new() { Code = "CS-201", Title = "Data Structures and Algorithms", MaxCapacity = 25 },
new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity=40 }
=======
new() { Code = "CS-101", Title = "Introduction to ComputerScience", Capacity = 30 },
new() { Code = "CS-201", Title = "Data Structures and Algorithms", Capacity = 25 },
new() { Code = "MAT-101", Title = "Calculus I", Capacity=40 }
>>>>>>> 1b8179c2f051a626319abdf900df189def4834c2
};
context.Courses.AddRange(courses);
context.SaveChanges();
var enrollments = new List<Enrollment>
{
new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
};
context.Enrollments.AddRange(enrollments);
context.SaveChanges();
}
}


app.Run(); // MUST BE LAST