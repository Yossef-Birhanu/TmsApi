using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Josi_TmsApi.Data;
using Josi_TmsApi.Entities;
using Josi_TmsApi.Services;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddProblemDetails();
//builder.Services.AddOpenApi();
builder.Services.AddControllers();
// Authentication & Authorization
builder.Services.AddAuthentication("TrainingScheme")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("TrainingScheme", null);

//Services are registered in the DBContext, so we can use THIS CODE
 // Register TmsDbContext scoped for incoming HTTP requests
builder.Services.AddDbContext<TmsDb1Context>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));

builder.Services.AddDbContext<TmsDb1Context>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
.LogTo(Console.WriteLine, LogLevel.Information) // Log SQLto output window
.EnableSensitiveDataLogging()); // Show parameters in querylogs (dev only)
builder.Services.AddOpenApi(documentName:"v1", configureOptions: options =>
{
    options.ShouldInclude=description=>description.GroupName=="v1";
});
builder.Services.AddOpenApi(documentName:"v2", configureOptions: options =>
{
    options.ShouldInclude=description=>description.GroupName=="v2";
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1,0);
    options.AssumeDefaultVersionWhenUnspecified= true;
    options.ReportApiVersions= true;
    options.ApiVersionReader=new UrlSegmentApiVersionReader();
    
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat="'v'vvv";
    options.SubstituteApiVersionInUrl=true;
});

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
app.UseExceptionHandler();
app.UseStatusCodePages();

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

//update your scalar config
app.MapScalarApiReference(options =>
{
    options.WithTitle("TMS API Reference").WithTheme(ScalarTheme.DeepSpace).WithDefaultHttpClient(ScalarTarget.CSharp,ScalarClient.HttpClient);
    //Tell scalar to pull both documents into its sidebar dropdown
    options
          .AddDocument("v1","API Version 1.0")
          .AddDocument("v2","API Version 2.0");
});

// TODO2
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
   // app.MapScalarApiReference();
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDb1Context>();
    await DataSeeder.SeedAsync(context);
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
  context.Database.EnsureCreated();
  context.Database.Migrate(); // Apply any pending migrations migration history intact
  if (!context.Students.Any())
{
var students = new List<Student>
{
new() { RegistrationNumber = "TMS-2026-0001", Name = "AliceSmith", GPA = 3.8m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0002", Name = "BobJones", GPA = 2.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
new() { RegistrationNumber = "TMS-2026-0004", Name = "DianaPrince", GPA = 3.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0005", Name = "EvanWright", GPA = 2.5m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0006", Name = "Yossef C", GPA = 3.7m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0007", Name = "Yossef B", GPA = 3.8m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0008", Name = "Yosef Bir", GPA = 3.8m, IsActive = true }
};
context.Students.AddRange(students);
var courses = new List<Course>
{
new() { Code = "CS-101", Title = "Introduction to ComputerScience", MaxCapacity = 30 },
new() { Code = "CS-201", Title = "Data Structures and Algorithms", MaxCapacity = 25 },
new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity=40 }
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