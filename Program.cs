
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
// ... your existing service registrations ...
builder.Services.AddOpenApi(); // Required before MapOpenApi() will work

builder.Services.AddControllers();

//Addhost validation so the container catches illegal lifetime wiring early:

builder.Host.UseDefaultServiceProvider(options =>
{
options.ValidateScopes = true;
options.ValidateOnBuild = true;
});

// 1. REGISTER SCHEMES AND SERVICES IN THE DI CONTAINER FIRST (Fixes the crash)
builder.Services.AddAuthentication("TrainingScheme")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("TrainingScheme", null);

builder.Services.AddAuthorization(); // REQUIRED: Populates the underlying engine

var app = builder.Build();

// 2. CONFIGURE THE MIDDLEWARE PIPELINE IN THE CORRECT ORDER
app.UseRouting();

app.UseAuthentication(); // Must come BEFORE Authorization
app.UseAuthorization();  // Verified safely now because services are registered above

// 3. MAP SECURED ENDPOINTS AFTER SECURITY MIDDLEWARE
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization(); // Enforces the custom header validation gate securely


 app.MapControllers();

app.Run();


app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
worker.ProcessBatch();
return Results.Ok("processed");
});

app.MapGet("/api/error", () =>
{
throw new TmsDatabaseException(" Simulated database failure for ProblemDetails testing");
});