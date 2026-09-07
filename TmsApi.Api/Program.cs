using System.Text;
using System.Threading.Channels;
using System.Threading.RateLimiting;

using Asp.Versioning;

using FluentValidation;
using MediatR;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Caching.Hybrid;

using Microsoft.IdentityModel.Tokens;

using Scalar.AspNetCore;

using Tms.Api.Authorization;
using TmsApi.Api.Authorization;
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
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Infrastructure.Workers;


public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ============================================================
        // SERVICES
        // ============================================================


        // ============================================================
        // CONFIGURATION
        // ============================================================

        var configuration = builder.Configuration;


        // ============================================================
        // CORS
        // ============================================================

        var allowedOrigins =
            configuration
                .GetSection("AllowedOrigins")
                .Get<string[]>()
            ?? new[]
            {
                "http://localhost:4200"
            };


        builder.Services.AddCors(options =>
        {
            options.AddPolicy("TmsClient", policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetPreflightMaxAge(
                        TimeSpan.FromMinutes(10));
            });
        });


        // ============================================================
        // DATABASE
        // ============================================================

        builder.Services.AddDbContext<TmsDb1Context>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString(
                    "TmsDatabase"));

            options.LogTo(
                Console.WriteLine,
                LogLevel.Information);

            options.EnableSensitiveDataLogging();
        });


        // ============================================================
        // APPLICATION SERVICES
        // ============================================================

        builder.Services.AddScoped<IEnrollmentService,
            EnrollmentService>();

        builder.Services.AddScoped<ICourseService,
            CourseService>();

        builder.Services.AddScoped<ICachedCourseService,
            CachedCourseService>();

        builder.Services.AddScoped<TokenService>();


        // ============================================================
        // ENROLLMENT WORKER
        // ============================================================

        builder.Services.AddSingleton<EnrollmentWorker>();


        // ============================================================
        // TRANSCRIPT SERVICES
        // ============================================================

        builder.Services.AddSingleton<ITranscriptStatusStore,
            InMemoryTranscriptStatusStore>();

        builder.Services.AddSingleton<ITranscriptNotificationService,
            SignalRTranscriptNotificationService>();


        // ============================================================
        // AUTHORIZATION HANDLER
        // ============================================================

        builder.Services.AddSingleton<
            IAuthorizationHandler,
            CourseInstructorHandler>();


        // ============================================================
        // BOUNDED CHANNEL
        // ============================================================

        builder.Services.AddSingleton(
            Channel.CreateBounded<TranscriptRequest>(
                new BoundedChannelOptions(100)
                {
                    FullMode =
                        BoundedChannelFullMode.Wait
                }));


        // ============================================================
        // HOSTED SERVICES
        // ============================================================

        builder.Services.AddHostedService<TranscriptWorker>();


        // ============================================================
        // SIGNALR
        // ============================================================

        builder.Services.AddSignalR();


        // ============================================================
        // MEDIATR
        // ============================================================

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                typeof(EnrollStudentHandler).Assembly);
        });


        // ============================================================
        // FLUENT VALIDATION
        // ============================================================

        builder.Services.AddValidatorsFromAssembly(
            typeof(EnrollStudentValidator).Assembly);


        // ============================================================
        // MEDIATR PIPELINE BEHAVIORS
        // ============================================================

        builder.Services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));

        builder.Services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));


        // ============================================================
        // ANTIFORGERY
        // ============================================================

        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
        });


        // ============================================================
        // CONTROLLERS
        // ============================================================

        builder.Services.AddControllers();


        // ============================================================
        // EXCEPTION HANDLING
        // ============================================================

        builder.Services.AddExceptionHandler<
            GlobalExceptionHandler>();

        builder.Services.AddProblemDetails();


        // ============================================================
        // HEALTH CHECKS
        // ============================================================

        builder.Services.AddHealthChecks();


        // ============================================================
        // IDENTITY
        // ============================================================

        builder.Services
            .AddIdentityCore<TmsUser>(options =>
            {
                // ----------------------------------------------------
                // Password Policy
                // ----------------------------------------------------

                options.Password.RequiredLength = 12;

                options.Password.RequireUppercase = true;

                options.Password.RequireDigit = true;

                options.Password.RequireNonAlphanumeric = true;


                // ----------------------------------------------------
                // Lockout Policy
                // ----------------------------------------------------

                options.Lockout.MaxFailedAccessAttempts = 5;

                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);

                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TmsDb1Context>();


        // ============================================================
        // JWT AUTHENTICATION
        // ============================================================

        var jwtKey =
            configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException(
                "Jwt:Key is missing from configuration.");
        }


        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,

                        ValidateAudience = true,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,

                        ValidIssuer =
                            configuration["Jwt:Issuer"],

                        ValidAudience =
                            configuration["Jwt:Audience"],

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtKey))
                    };
            });


        // ============================================================
        // AUTHORIZATION
        // ============================================================

        builder.Services.AddAuthorizationBuilder()

            .AddPolicy(
                "CanEditCourse",
                policy =>
                {
                    policy.Requirements.Add(
                        new CourseInstructorRequirement());
                });


        // ============================================================
        // HYBRID CACHE
        // ============================================================

        builder.Services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions =
                new HybridCacheEntryOptions
                {
                    Expiration =
                        TimeSpan.FromMinutes(10),

                    LocalCacheExpiration =
                        TimeSpan.FromMinutes(2)
                };
        });


        // ============================================================
        // REDIS CACHE
        // ============================================================

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                configuration.GetConnectionString(
                    "Redis");

            options.InstanceName = "tms:";
        });


        // ============================================================
        // OPENAPI
        // ============================================================

        builder.Services.AddOpenApi(
            "v1",
            options =>
            {
                options.ShouldInclude =
                    description =>
                        description.GroupName == "v1";
            });


        builder.Services.AddOpenApi(
            "v2",
            options =>
            {
                options.ShouldInclude =
                    description =>
                        description.GroupName == "v2";
            });


        // ============================================================
        // API VERSIONING
        // ============================================================

        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion =
                    new ApiVersion(1, 0);

                options.AssumeDefaultVersionWhenUnspecified =
                    true;

                options.ReportApiVersions = true;

                options.ApiVersionReader =
                    new UrlSegmentApiVersionReader();
            })

            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";

                options.SubstituteApiVersionInUrl = true;
            });


        // ============================================================
        // RATE LIMITING
        // ============================================================

        builder.Services.AddRateLimiter(options =>
        {

            // ========================================================
            // GLOBAL RATE LIMITER
            // ========================================================

            options.GlobalLimiter =
                PartitionedRateLimiter.Create<
                    HttpContext,
                    string>(
                    httpContext =>
                    {
                        var (
                            partitionKey,
                            tier) =
                            ApiKeyResolver.Resolve(
                                httpContext);


                        return tier switch
                        {

                            // ========================================
                            // PAID
                            // ========================================

                            ApiKeyTier.Paid =>
                                RateLimitPartition
                                    .GetTokenBucketLimiter(
                                        $"paid:{partitionKey}",
                                        _ =>
                                            new TokenBucketRateLimiterOptions
                                            {
                                                TokenLimit = 200,

                                                TokensPerPeriod = 100,

                                                ReplenishmentPeriod =
                                                    TimeSpan.FromSeconds(10),

                                                QueueLimit = 0,

                                                AutoReplenishment = true
                                            }),


                            // ========================================
                            // FREE
                            // ========================================

                            ApiKeyTier.Free =>
                                RateLimitPartition
                                    .GetTokenBucketLimiter(
                                        $"free:{partitionKey}",
                                        _ =>
                                            new TokenBucketRateLimiterOptions
                                            {
                                                TokenLimit = 30,

                                                TokensPerPeriod = 10,

                                                ReplenishmentPeriod =
                                                    TimeSpan.FromSeconds(10),

                                                QueueLimit = 0,

                                                AutoReplenishment = true
                                            }),


                            // ========================================
                            // ANONYMOUS
                            // ========================================

                            _ =>
                                RateLimitPartition
                                    .GetTokenBucketLimiter(
                                        $"anon:{partitionKey}",
                                        _ =>
                                            new TokenBucketRateLimiterOptions
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


            // ========================================================
            // SEARCH RATE LIMITER
            // ========================================================

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


            // ========================================================
            // TRANSCRIPT CONCURRENCY LIMITER
            // ========================================================

            options.AddConcurrencyLimiter(
                "transcripts",
                opt =>
                {
                    opt.PermitLimit = 2;

                    opt.QueueLimit = 5;

                    opt.QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst;
                });


            // ========================================================
            // AUTHENTICATION RATE LIMITER
            // ========================================================

            options.AddFixedWindowLimiter(
                "AuthLimiter",
                opt =>
                {
                    opt.PermitLimit = 5;

                    opt.Window =
                        TimeSpan.FromMinutes(1);

                    opt.QueueLimit = 0;
                });
        });


        // ============================================================
        // HOST VALIDATION
        // ============================================================

        builder.Host.UseDefaultServiceProvider(
            options =>
            {
                options.ValidateScopes = true;

                options.ValidateOnBuild = true;
            });


        // ============================================================
        // BUILD APPLICATION
        // ============================================================

        var app = builder.Build();


        // ============================================================
        // DATABASE MIGRATION + SEEDING
        // ============================================================

        using (var scope =
            app.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<TmsDb1Context>();


            // --------------------------------------------------------
            // Run migrations only for relational databases
            // --------------------------------------------------------

            if (context.Database.IsRelational())
            {
                // context.Database.Migrate();
            }


            // --------------------------------------------------------
            // Main application seeder
            // --------------------------------------------------------

            await DataSeeder.SeedAsync(context);


            // --------------------------------------------------------
            // Additional test data
            // --------------------------------------------------------

            if (!context.Students.Any())
            {
                var students =
                    new List<Student>
                    {
                        new()
                        {
                            RegistrationNumber =
                                "TMS-2026-0001",

                            Name = "Alice Smith",

                            GPA = 3.8m,

                            IsActive = true
                        },

                        new()
                        {
                            RegistrationNumber =
                                "TMS-2026-0002",

                            Name = "Bob Jones",

                            GPA = 2.9m,

                            IsActive = true
                        },

                        new()
                        {
                            RegistrationNumber =
                                "TMS-2026-0003",

                            Name = "Charlie Brown",

                            GPA = 3.4m,

                            IsActive = false
                        },

                        new()
                        {
                            RegistrationNumber =
                                "TMS-2026-0004",

                            Name = "Diana Prince",

                            GPA = 3.9m,

                            IsActive = true
                        },

                        new()
                        {
                            RegistrationNumber =
                                "TMS-2026-0005",

                            Name = "Evan Wright",

                            GPA = 2.5m,

                            IsActive = true
                        },

                        new()
                        {
                            RegistrationNumber =
                                "TMS-2026-0006",

                            Name = "Yossef C",

                            GPA = 3.7m,

                            IsActive = true
                        },

                        new()
                        {
                            RegistrationNumber =
                                "TMS-2026-0007",

                            Name = "Yossef B",

                            GPA = 3.8m,

                            IsActive = true
                        },

                        new()
                        {
                            RegistrationNumber =
                                "TMS-2026-0008",

                            Name = "Yosef Bir",

                            GPA = 3.8m,

                            IsActive = true
                        }
                    };


                context.Students.AddRange(students);


                // ----------------------------------------------------
                // Courses
                // ----------------------------------------------------

                var courses =
                    new List<Course>
                    {
                        new()
                        {
                            Code = "CS-101",

                            Title =
                                "Introduction to Computer Science",

                            MaxCapacity = 30
                        },

                        new()
                        {
                            Code = "CS-201",

                            Title =
                                "Data Structures and Algorithms",

                            MaxCapacity = 25
                        },

                        new()
                        {
                            Code = "MAT-101",

                            Title =
                                "Calculus I",

                            MaxCapacity = 40
                        }
                    };


                context.Courses.AddRange(courses);


                // ----------------------------------------------------
                // Save students and courses first
                // ----------------------------------------------------

                await context.SaveChangesAsync();


                // ----------------------------------------------------
                // Enrollments
                // ----------------------------------------------------

                var enrollments =
                    new List<Enrollment>
                    {
                        new()
                        {
                            StudentId =
                                students[0].Id,

                            CourseId =
                                courses[0].Id,

                            Grade = 4.0m
                        },

                        new()
                        {
                            StudentId =
                                students[0].Id,

                            CourseId =
                                courses[1].Id,

                            Grade = 3.6m
                        },

                        new()
                        {
                            StudentId =
                                students[1].Id,

                            CourseId =
                                courses[0].Id,

                            Grade = 2.8m
                        },

                        new()
                        {
                            StudentId =
                                students[3].Id,

                            CourseId =
                                courses[1].Id,

                            Grade = 3.9m
                        }
                    };


                context.Enrollments.AddRange(
                    enrollments);


                await context.SaveChangesAsync();
            }
        }


        // ============================================================
        // PASSWORD HASHING DEMO
        // ============================================================

        var cryptoService =
            new CryptoDemoService();


        string hash1 =
            cryptoService.HashUserPassword(
                "Password123!");


        string hash2 =
            cryptoService.HashUserPassword(
                "Password123!");


        Console.WriteLine(
            $"Hash 1: {hash1}");

        Console.WriteLine(
            $"Hash 2: {hash2}");


        bool match1 =
            cryptoService.VerifyUserPassword(
                "Password123!",
                hash1);


        bool match2 =
            cryptoService.VerifyUserPassword(
                "Password123!",
                hash2);


        Console.WriteLine(
            $"Hash 1 verification: {match1}");

        Console.WriteLine(
            $"Hash 2 verification: {match2}");


        // ============================================================
        // HTTP PIPELINE
        // ============================================================

        app.UseExceptionHandler();


        app.UseStatusCodePages();


        // ============================================================
        // CORS
        // ============================================================

        app.UseCors("TmsClient");


        // ============================================================
        // ROUTING
        // ============================================================

        app.UseRouting();


        // ============================================================
        // RATE LIMITING
        // ============================================================

        app.UseRateLimiter();


        // ============================================================
        // AUTHENTICATION
        // ============================================================

        app.UseAuthentication();


        // ============================================================
        // AUTHORIZATION
        // ============================================================

        app.UseAuthorization();


        // ============================================================
        // XSRF / ANTIFORGERY COOKIE
        // ============================================================

        app.Use(async (context, next) =>
        {
            if (
                context.User.Identity?.IsAuthenticated == true
                ||
                context.Request.Cookies.ContainsKey(
                    "tms_auth"))
            {
                var antiforgery =
                    context.RequestServices
                        .GetRequiredService<IAntiforgery>();


                var tokens =
                    antiforgery.GetAndStoreTokens(
                        context);


                if (!string.IsNullOrEmpty(
                    tokens.RequestToken))
                {
                    context.Response.Cookies.Append(
                        "XSRF-TOKEN",
                        tokens.RequestToken,
                        new CookieOptions
                        {
                            HttpOnly = false,

                            Secure =
                                !builder.Environment
                                    .IsDevelopment(),

                            SameSite =
                                SameSiteMode.Strict
                        });
                }
            }


            await next();
        });


        // ============================================================
        // SECURITY HEADERS
        // ============================================================

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append(
                "X-Content-Type-Options",
                "nosniff");


            context.Response.Headers.Append(
                "Referrer-Policy",
                "strict-origin-when-cross-origin");


            context.Response.Headers.Append(
                "Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: blob:; " +
                "font-src 'self' data:; " +
                "connect-src 'self' http://localhost:5094 ws://localhost:5094;");


            await next();
        });


        // ============================================================
        // API VERSION DEPRECATION
        // ============================================================

        app.UseMiddleware<V1DeprecationMiddleware>();


        // ============================================================
        // SIGNALR HUB
        // ============================================================

        app.MapHub<TmsHub>(
            "/hubs/tms")
            .RequireCors("TmsClient");


        // ============================================================
        // CONTROLLERS
        // ============================================================

        app.MapControllers();


        // ============================================================
        // HEALTH CHECKS
        // ============================================================

        app.MapHealthChecks(
            "/health/live")
            .DisableRateLimiting();


        app.MapHealthChecks(
            "/health/ready")
            .DisableRateLimiting();


        // ============================================================
        // ASSESSMENT RESULTS
        // ============================================================

        app.MapGet(
            "/api/assessments/results",
            () =>
            {
                return Results.Ok(
                    new
                    {
                        courseCode = "CS-101",

                        studentId = "S-001",

                        letterGrade = "A"
                    });
            })
            .RequireAuthorization();


        // ============================================================
        // ENROLLMENT WORKER SMOKE TEST
        // ============================================================

        app.MapGet(
            "/api/enrollments/worker-smoke",
            (EnrollmentWorker worker) =>
            {
                worker.ProcessBatch();

                return Results.Ok(
                    "processed");
            });


        // ============================================================
        // ERROR TEST ENDPOINT
        // ============================================================

        app.MapGet(
            "/api/error",
            () =>
            {
                throw new TmsDatabaseException(
                    "Simulated database failure");
            });


        // ============================================================
        // SCALAR API DOCUMENTATION
        // ============================================================

        app.MapScalarApiReference(
            options =>
            {
                options
                    .WithTitle(
                        "TMS API Reference")

                    .WithTheme(
                        ScalarTheme.DeepSpace)

                    .WithDefaultHttpClient(
                        ScalarTarget.CSharp,
                        ScalarClient.HttpClient);


                options
                    .AddDocument(
                        "v1",
                        "API Version 1.0")

                    .AddDocument(
                        "v2",
                        "API Version 2.0");
            });


        // ============================================================
        // OPENAPI
        // ============================================================

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            Console.WriteLine(
                "Development mode");
        }


        // ============================================================
        // START APPLICATION
        // ============================================================

        app.Run();
    }
}