using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Authentication & Authorization
builder.Services.AddAuthentication("TrainingScheme")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("TrainingScheme", null);

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

app.Run(); // MUST BE LAST