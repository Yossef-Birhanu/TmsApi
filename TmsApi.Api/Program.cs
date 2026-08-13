using System.Threading.Channels;
using System.Threading.RateLimiting;

using Asp.Versioning;

using FluentValidation;
using MediatR;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

using Scalar.AspNetCore;

using TmsApi.Api.ExceptionHandlers;
using TmsApi.Api.Hubs;
using TmsApi.Api.Middlewares;
using TmsApi.Api.Notifications;
using TmsApi.Api.RateLimiting;
using TmsApi.Api.Worker;

using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Notifications;
using TmsApi.Application.Transcripts;
using TmsApi.Domain.Entities;

using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Infrastructure.Workers;


public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ======================================================
        // SERVICES
        // ======================================================


        // ==================================================
        // Configure CORS
        // ==================================================

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAngular", policy =>
                policy
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });



        // ---------- Database ----------
        builder.Services.AddDbContext<TmsDb1Context>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("TmsDatabase"))
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
        );


        // ---------- Application Services ----------
        builder.Services.AddSingleton<EnrollmentWorker>();

        builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
        builder.Services.AddScoped<ICourseService, CourseService>();
        builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
        builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();
        builder.Services.AddSingleton<ITranscriptNotificationService, SignalRTranscriptNotificationService>();
        //-------------BOUNDED CHANNEL------------
        builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(
            new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));
    

          //----------SignalIR--------------
          builder.Services.AddSignalR();

        // ---------- MediatR ----------
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(EnrollStudentHandler).Assembly));


        // ---------- Fluent Validation ----------
        builder.Services.AddValidatorsFromAssembly(
            typeof(EnrollStudentValidator).Assembly);


        // ---------- Pipeline Behaviors ----------
        builder.Services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));

        builder.Services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));


        // ---------- Controllers ----------
        builder.Services.AddControllers();
        builder.Services.AddHostedService<TranscriptWorker>();

        // ---------- Exception Handling ----------
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();


        // ---------- Health Check ----------
        builder.Services.AddHealthChecks();


        // ---------- Authentication ----------
        builder.Services.AddAuthentication("TrainingScheme")
            .AddScheme<AuthenticationSchemeOptions,
                TrainingAuthHandler>(
                    "TrainingScheme",
                    null);


        // ---------- Authorization ----------
        builder.Services.AddAuthorization();


        // ---------- Hybrid Cache ----------
        builder.Services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions =
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromMinutes(2)
                };
        });


        // ---------- Redis Cache ----------
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                builder.Configuration.GetConnectionString("Redis");

            options.InstanceName = "tms:";
        });

         // ---------- OpenAPI ----------
        builder.Services.AddOpenApi("v1",options =>
        {
            options.ShouldInclude =description =>
                description.GroupName == "v1";
        });


        builder.Services.AddOpenApi("v2",options =>
        {
            options.ShouldInclude =description =>
                description.GroupName == "v2";
        });

        // ---------- API Versioning ----------
     builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);

            options.AssumeDefaultVersionWhenUnspecified = true;

            options.ReportApiVersions = true;


            options.ApiVersionReader =
            
                    new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";

            options.SubstituteApiVersionInUrl = true;
        });



        // ---------- Rate Limiting ----------
        builder.Services.AddRateLimiter(options =>
        {

            options.GlobalLimiter =
                PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext =>
                {

                    var (partitionKey, tier) =
                        ApiKeyResolver.Resolve(httpContext);


                    return tier switch
                    {

                        ApiKeyTier.Paid =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            $"paid:{partitionKey}",
                            _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 200,
                                TokensPerPeriod = 100,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            }),


                        ApiKeyTier.Free =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            $"free:{partitionKey}",
                            _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 30,
                                TokensPerPeriod = 10,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            }),


                        _ =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            $"anon:{partitionKey}",
                            _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 10,
                                TokensPerPeriod = 5,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            })
                    };
                });



            options.AddTokenBucketLimiter(
                "search",
                opt =>
                {
                    opt.TokenLimit = 10;
                    opt.TokensPerPeriod = 5;
                    opt.ReplenishmentPeriod =
                        TimeSpan.FromSeconds(10);
                    opt.QueueLimit = 2;
                    opt.QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst;
                    opt.AutoReplenishment = true;
                });



            options.AddConcurrencyLimiter(
                "transcripts",
                opt =>
                {
                    opt.PermitLimit = 2;
                    opt.QueueLimit = 5;
                    opt.QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst;
                });

        });


        // ---------- Host Validation ----------
        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });



        // ======================================================
        // BUILD APPLICATION
        // ======================================================

        var app = builder.Build();



        // ======================================================
        // MIDDLEWARE PIPELINE
        // ======================================================

        app.UseCors("AllowAngular");
        app.UseRouting();

        app.UseRateLimiter();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseExceptionHandler();

        app.UseStatusCodePages();
        app.MapHub<TmsHub>("/hubs/tms");

        app.UseMiddleware<V1DeprecationMiddleware>();



        // ======================================================
        // ENDPOINTS
        // ======================================================


        app.MapControllers();


        app.MapHealthChecks("/health/live")
            .DisableRateLimiting();


        app.MapHealthChecks("/health/ready")
            .DisableRateLimiting();



        app.MapGet(
        "/api/assessments/results",
        () =>
        Results.Ok(new
        {
            courseCode = "CS-101",
            studentId = "S-001",
            letterGrade = "A"
        }))
        .RequireAuthorization();



        app.MapGet("/api/enrollments/worker-smoke",(EnrollmentWorker worker) =>
        {
            worker.ProcessBatch();

            return Results.Ok("processed");
        });



        app.MapGet( "/api/error",() =>
        {
            throw new TmsDatabaseException(
                "Simulated database failure");
        });



        // ======================================================
        // SCALAR + OPENAPI
        // ======================================================


        app.MapScalarApiReference(options =>
        {
            options.WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp,ScalarClient.HttpClient);
            options.AddDocument("v1", "API Version 1.0")
            .AddDocument("v2", "API Version 2.0");
        });



        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            Console.WriteLine("Development mode");


            using var scope =
                app.Services.CreateScope();


            var context =
                scope.ServiceProvider
                .GetRequiredService<TmsDb1Context>();


            await DataSeeder.SeedAsync(context);
        }



        // ======================================================
        // DATABASE MIGRATION + SEEDING
        // ======================================================

        //Seed test data at startup
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TmsDb1Context>();
            context.Database.EnsureCreated();

            // using (var scope = app.Services.CreateScope())
            // {
            //     var context = scope.ServiceProvider.GetRequiredService<TmsDb1Context>();

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
    }
}