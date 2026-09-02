// ============================================================
// SYSTEM NAMESPACES
// ============================================================
using System.Text;
using System.Threading.Channels;
using System.Threading.RateLimiting;
// ============================================================
// THIRD-PARTY / FRAMEWORK NAMESPACES
// ============================================================
using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

// ============================================================
// TMS API NAMESPACES
// ============================================================
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

// ============================================================
// PROGRAM
// ============================================================
public partial class Program
{
    public static async Task Main(string[] args)
    {
        // ====================================================
        // 1. CREATE WEB APPLICATION BUILDER
        // ====================================================

        var builder = WebApplication.CreateBuilder(args);

        // ====================================================
        // 2. READ CONFIGURATION
        // ====================================================
        //
        // Read Angular client origins from:
        //
        // appsettings.json
        // appsettings.Development.json
        //
        // Example:
        //
        // "AllowedOrigins": ["http://localhost:4200"]
        //
        // ====================================================

        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins") .Get<string[]>()?? new[]
            {
                "http://localhost:4200"
            };

        // ====================================================
        // 3. DATABASE
        // ====================================================
        //
        // Production/development database:
        // PostgreSQL using Npgsql.
        //
        // IMPORTANT:
        // The integration tests can replace this with
        // EF Core InMemory.
        //
        // Therefore, later we check:
        //
        // context.Database.IsRelational()
        //
        // before calling Migrate().
        //
        // ====================================================

        builder.Services.AddDbContext<TmsDb1Context>(options =>{
            options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"));

            // Display EF Core SQL/database logs.
            options.LogTo(Console.WriteLine,LogLevel.Information);

            // Development only.
            // Do not normally enable this in production.
            options.EnableSensitiveDataLogging();
        });
        // ====================================================
        // 4. CORS
        // ====================================================
        //
        // Angular application:
        //
        // http://localhost:4200
        //
        // IMPORTANT:
        // We register only ONE CORS policy.
        //
        // Policy name:
        // TmsClient
        //
        // ====================================================

        builder.Services.AddCors(options =>{
            options.AddPolicy("TmsClient",
                policy =>{
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetPreflightMaxAge(
                            TimeSpan.FromMinutes(10));
                });
        });

        // ====================================================
        // 5. CONTROLLERS
        // ====================================================

        builder.Services.AddControllers();

        // ====================================================
        // 6. EXCEPTION HANDLING
        // ====================================================
        //
        // Global exception handler converts exceptions into
        // proper HTTP ProblemDetails responses.
        //
        // ====================================================

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddProblemDetails();

        // ====================================================
        // 7. HEALTH CHECKS
        // ====================================================

        builder.Services.AddHealthChecks();

        // ====================================================
        // 8. SIGNALR
        // ====================================================
        //
        // Used for real-time communication between the API
        // and Angular client.
        //
        // IMPORTANT:
        // AddSignalR() appears only ONCE.
        //
        // ====================================================

        builder.Services.AddSignalR();

        // ====================================================
        // 9. MEDIATR
        // ====================================================
        //
        // Automatically discovers MediatR handlers from the
        // assembly containing EnrollStudentHandler.
        //
        // ====================================================

        builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly);
            });


        // ====================================================
        // 10. FLUENT VALIDATION
        // ====================================================

        builder.Services.AddValidatorsFromAssembly( typeof(EnrollStudentValidator).Assembly);

        // ====================================================
        // 11. MEDIATR PIPELINE BEHAVIORS
        // ====================================================

        // Logging behavior.
        builder.Services.AddTransient( typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // Validation behavior.
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>),typeof(ValidationBehavior<,>));

        // ====================================================
        // 12. APPLICATION SERVICES
        // ====================================================

        // Enrollment service.
        builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

        // Course service.
        builder.Services.AddScoped< ICourseService, CourseService>();

        // Cached course service.
        builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();

        // Transcript status store.
        builder.Services.AddSingleton< ITranscriptStatusStore,InMemoryTranscriptStatusStore>();

        // Transcript SignalR notification service.
        builder.Services.AddSingleton<ITranscriptNotificationService,SignalRTranscriptNotificationService>();

        // Token service.
        builder.Services.AddScoped<TokenService>();

        // ====================================================
        // 13. AUTHORIZATION HANDLER
        // ====================================================
        //
        // Handles the CanEditCourse authorization requirement.
        //
        // ====================================================

        builder.Services.AddSingleton< IAuthorizationHandler,CourseInstructorHandler>();
        // ====================================================
        // 14. BOUNDED CHANNEL
        // ====================================================
        //
        // Maximum queue size = 100.
        //
        // When the queue is full, writers wait instead of
        // creating unlimited memory usage.
        //
        // ====================================================

        builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(new BoundedChannelOptions(100)
                {
                    FullMode =
                        BoundedChannelFullMode.Wait
                }));

        // ====================================================
        // 15. BACKGROUND WORKER
        // ====================================================

        builder.Services.AddSingleton<EnrollmentWorker>();

        builder.Services.AddHostedService<TranscriptWorker>();

        // ====================================================
        // 16. IDENTITY
        // ====================================================
        //
        // ASP.NET Core Identity configuration.
        //
        // ====================================================

        builder.Services.AddIdentityCore<TmsUser>(options =>
            {
                // ------------------------------------------------
                // PASSWORD POLICY
                // ------------------------------------------------

                // Minimum password length.
                options.Password.RequiredLength = 12;

                // Must contain uppercase character.
                options.Password.RequireUppercase = true;

                // Must contain number.
                options.Password.RequireDigit = true;

                // Must contain special character.
                options.Password.RequireNonAlphanumeric = true;


                // ------------------------------------------------
                // ACCOUNT LOCKOUT
                // ------------------------------------------------

                // Lock account after 5 failed attempts.
                options.Lockout.MaxFailedAccessAttempts = 5;

                // Lock account for 15 minutes.
                options.Lockout.DefaultLockoutTimeSpan =
                 TimeSpan.FromMinutes(15);

                // Enable lockout for new users.
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole>().AddEntityFrameworkStores<TmsDb1Context>();

        // ====================================================
        // 17. JWT AUTHENTICATION
        // ====================================================
        //
        // The API uses JWT Bearer authentication.
        //
        // ====================================================

        builder.Services.AddAuthentication(options =>
            {
                // Default authentication scheme.
                options.DefaultAuthenticateScheme =JwtBearerDefaults.AuthenticationScheme;

                // Default challenge scheme.
                options.DefaultChallengeScheme =JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
                {
                    // Read JWT configuration.
                    var jwtKey =builder.Configuration["Jwt:Key"];

                    var jwtIssuer = builder.Configuration["Jwt:Issuer"];

                    var jwtAudience =builder.Configuration["Jwt:Audience"];

                    // ------------------------------------------------
                    // VALIDATE JWT CONFIGURATION
                    // ------------------------------------------------

                    if (string.IsNullOrWhiteSpace(jwtKey))
                    {
                        throw new InvalidOperationException("JWT Key is missing. " +"Check Jwt:Key in appsettings.json.");
                    }

                    if (string.IsNullOrWhiteSpace(jwtIssuer))
                    {
                        throw new InvalidOperationException("JWT Issuer is missing. " + "Check Jwt:Issuer in appsettings.json.");
                    }

                    if (string.IsNullOrWhiteSpace(jwtAudience))
                    {
                        throw new InvalidOperationException( "JWT Audience is missing. " +"Check Jwt:Audience in appsettings.json.");
                    }

                    // ------------------------------------------------
                    // TOKEN VALIDATION
                    // ------------------------------------------------

                    options.TokenValidationParameters =new TokenValidationParameters
                        {
                            // Validate token issuer.
                            ValidateIssuer = true,

                            // Validate token audience.
                            ValidateAudience = true,

                            // Validate token expiration.
                            ValidateLifetime = true,

                            // Validate signing key.
                            ValidateIssuerSigningKey = true,

                            // Expected issuer.
                            ValidIssuer = jwtIssuer,

                            // Expected audience.
                            ValidAudience = jwtAudience,

                            // Signing key.
                            IssuerSigningKey =new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                        };
                });

        // ====================================================
        // 18. AUTHORIZATION
        // ====================================================

        builder.Services.AddAuthorizationBuilder() .AddPolicy("CanEditCourse",policy =>
                {
                    policy.Requirements.Add(new CourseInstructorRequirement());
                });
        // ====================================================
        // 19. ANTIFORGERY / XSRF
        // ====================================================
        //
        // Angular should use:
        //
        // X-XSRF-TOKEN
        //
        // ====================================================

        builder.Services.AddAntiforgery( options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
            });


        // ====================================================
        // 20. HYBRID CACHE
        // ====================================================
        //
        // Local cache:
        // 2 minutes
        //
        // Distributed cache:
        // 10 minutes
        //
        // ====================================================

        builder.Services.AddHybridCache(options =>
            {
                options.DefaultEntryOptions =new HybridCacheEntryOptions
                    {
                        Expiration =TimeSpan.FromMinutes(10),

                        LocalCacheExpiration = TimeSpan.FromMinutes(2)
                    };
            });

        // ====================================================
        // 21. REDIS CACHE
        // ====================================================

        builder.Services.AddStackExchangeRedisCache( options =>
            {
                options.Configuration = builder.Configuration .GetConnectionString("Redis");
                 options.InstanceName = "tms:";
            });

        // ====================================================
        // 22. API VERSIONING
        // ====================================================
        //
        // Default version = 1.0
        //
        // Version can be specified in the URL:
        //
        // /api/v1/courses
        // /api/v2/courses
        //
        // ====================================================

        builder.Services .AddApiVersioning(options =>
                {
                    // Default API version.
                    options.DefaultApiVersion = new ApiVersion(1, 0);

                    // If no version is specified,
                    // use version 1.0.
                    options.AssumeDefaultVersionWhenUnspecified =true;

                    // Return supported API versions
                    // in response headers.
                    options.ReportApiVersions = true;

                    // Read version from URL.
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                }).AddApiExplorer(options =>{
                    // Generate groups:
                    // v1
                    // v2
                    options.GroupNameFormat = "'v'VVV";

                    // Replace version in route.
                    options.SubstituteApiVersionInUrl = true;
                });

        // ====================================================
        // 23. OPENAPI
        // ====================================================
        //
        // Separate OpenAPI documents for v1 and v2.
        //
        // ====================================================

        builder.Services.AddOpenApi("v1",options =>
            {
                options.ShouldInclude =description =>description.GroupName == "v1";
            });

        builder.Services.AddOpenApi("v2",options =>
        {
                options.ShouldInclude =description => description.GroupName == "v2";
            });
        // ====================================================
        // 24. RATE LIMITING
        // ====================================================
        //
        // Global:
        //
        // Paid:
        // 200 tokens
        //
        // Free:
        // 30 tokens
        //
        // Anonymous:
        // 10 tokens
        //
        // ====================================================

        builder.Services.AddRateLimiter( options => {
                // ------------------------------------------------
                // GLOBAL RATE LIMITER
                // ------------------------------------------------

                options.GlobalLimiter =PartitionedRateLimiter.Create<HttpContext,string>( httpContext =>
                        {
                            // Resolve API key and user tier.
                            var (partitionKey, tier) =ApiKeyResolver.Resolve( httpContext);
                            // ------------------------------------------------
                            // PAID USERS
                            // ------------------------------------------------

                         if (tier ==ApiKeyTier.Paid)
                            {
                                return RateLimitPartition.GetTokenBucketLimiter( $"paid:{partitionKey}",_ =>new TokenBucketRateLimiterOptions
                                                {
                                                    TokenLimit = 200,

                                                    TokensPerPeriod = 100,

                                                    ReplenishmentPeriod = TimeSpan.FromSeconds( 10),

                                                    QueueLimit = 0,

                                                    AutoReplenishment =true
                                                });
                            }
                            // ------------------------------------------------
                            // FREE USERS
                            // ------------------------------------------------

                            if (tier ==ApiKeyTier.Free)
                            {
                                return RateLimitPartition .GetTokenBucketLimiter( $"free:{partitionKey}",_ =>new TokenBucketRateLimiterOptions
                                                {
                                                    TokenLimit = 30,

                                                    TokensPerPeriod = 10,

                                                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),

                                                    QueueLimit = 0,

                                                    AutoReplenishment = true
                                                });
                            }

                            // ------------------------------------------------
                            // ANONYMOUS USERS
                            // ------------------------------------------------

                            return RateLimitPartition.GetTokenBucketLimiter( $"anon:{partitionKey}", _ =>new TokenBucketRateLimiterOptions
                                            {
                                                TokenLimit = 10,

                                                TokensPerPeriod = 5,

                                                ReplenishmentPeriod =TimeSpan.FromSeconds(10),

                                                QueueLimit = 0,

                                                AutoReplenishment =true
                                            });
                        });


                // ------------------------------------------------
                // SEARCH RATE LIMITER
                // ------------------------------------------------

                options.AddTokenBucketLimiter("search",opt =>
                    {
                        opt.TokenLimit = 10;

                        opt.TokensPerPeriod = 5;

                        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);

                        opt.QueueLimit = 2;

                        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

                        opt.AutoReplenishment = true;
                    });
                // ------------------------------------------------
                // TRANSCRIPT CONCURRENCY LIMITER
                // ------------------------------------------------

                options.AddConcurrencyLimiter("transcripts", opt =>
                    {
                        opt.PermitLimit = 2;

                        opt.QueueLimit = 5;

                        opt.QueueProcessingOrder =QueueProcessingOrder.OldestFirst;
                    });
            });

        // ====================================================
        // 25. AUTHENTICATION RATE LIMITER
        // ====================================================

        builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("AuthLimiter",opt =>
                    {
                        // Maximum 5 attempts.
                        opt.PermitLimit = 5;

                        // Per one minute.
                        opt.Window = TimeSpan.FromMinutes(1);

                        // Do not queue requests.
                        opt.QueueLimit = 0;
                    });
            });


        // ====================================================
        // 26. HOST VALIDATION
        // ====================================================
        //
        // Detect dependency injection problems during
        // application startup.
        //
        // ====================================================

        builder.Host.UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;

                options.ValidateOnBuild = true;
            });

        // ====================================================
        // 27. BUILD APPLICATION
        // ====================================================

        var app = builder.Build();

        // ====================================================
        // 28. DATABASE MIGRATION + SEEDING
        // ====================================================
        //
        // VERY IMPORTANT FOR YOUR TEST ERROR.
        //
        // PostgreSQL:
        //
        //     IsRelational() = true
        //     Migrate() executes.
        //
        // EF Core InMemory:
        //
        //     IsRelational() = false
        //     Migrate() is skipped.
        //
        // This prevents:
        //
        // "Relational-specific methods can only be used
        // when the context is using a relational database
        // provider."
        //
        // ====================================================

        using (var scope = app.Services.CreateScope()){
            var context =scope.ServiceProvider.GetRequiredService<TmsDb1Context>();

            // ------------------------------------------------
            // DATABASE MIGRATION
            // ------------------------------------------------
                // Apply pending EF Core migrations.
                // context.Database.Migrate();
            
            // ------------------------------------------------
            // APPLICATION DATA SEEDER
            // ------------------------------------------------

            await DataSeeder.SeedAsync(context);

            // ------------------------------------------------
            // STUDENT / COURSE / ENROLLMENT SEEDING
            // ------------------------------------------------
            //
            // Only create these records if no students exist.
            //
            // ------------------------------------------------

            if (!context.Students.Any())
            {
                // ============================================
                // STUDENTS
                // ============================================

                var students = new List<Student>
                {
                    new(){RegistrationNumber = "TMS-2026-0001",Name = "Alice Smith",GPA = 3.8m,IsActive = true},

                    new(){RegistrationNumber = "TMS-2026-0002",Name = "Bob Jones",GPA = 2.9m,IsActive = true},

                    new(){ RegistrationNumber ="TMS-2026-0003",Name = "Charlie Brown", GPA = 3.4m,IsActive = false},

                    new(){RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince", GPA = 3.9m,IsActive = true},

                    new(){RegistrationNumber = "TMS-2026-0005",Name = "Evan Wright",GPA = 2.5m,IsActive = true},

                    new(){ RegistrationNumber = "TMS-2026-0006",Name = "Yossef C",GPA = 3.7m,IsActive = true},

                    new(){RegistrationNumber ="TMS-2026-0007",Name = "Yossef B", GPA = 3.8m,IsActive = true },

                    new() {RegistrationNumber ="TMS-2026-0008",Name = "Yosef Bir", GPA = 3.8m, IsActive = true}
                };


                // Add students to database.
                context.Students.AddRange(students);

                await context.SaveChangesAsync();

                // ============================================
                // COURSES
                // ============================================

                var courses = new List<Course>
                {
                    new() {Code = "CS-101",Title ="Introduction to Computer Science",MaxCapacity = 30 },

                    new(){Code = "CS-201",Title ="Data Structures and Algorithms", MaxCapacity = 25},

                    new(){ Code = "MAT-101",Title = "Calculus I", MaxCapacity = 40}
                };


                // Add courses.
                context.Courses.AddRange(courses);

                await context.SaveChangesAsync();

                // ============================================
                // ENROLLMENTS
                // ============================================

                var enrollments =new List<Enrollment>
                    {
                        new() { StudentId =students[0].Id, CourseId =courses[0].Id, Grade = 4.0m},

                        new() {StudentId =students[0].Id, CourseId =courses[1].Id, Grade = 3.6m},

                        new(){ StudentId =students[1].Id,CourseId =courses[0].Id,Grade = 2.8m},

                        new(){StudentId =students[3].Id,CourseId =courses[1].Id, Grade = 3.9m}
                    };
                // Add enrollments.
                context.Enrollments.AddRange( enrollments);

                await context.SaveChangesAsync();
            }
        }

        // ====================================================
        // 29. DEVELOPMENT OPENAPI
        // ====================================================

        if (app.Environment.IsDevelopment())
        {
            // Map OpenAPI endpoints.
            app.MapOpenApi();

            Console.WriteLine("Development mode");
        }

        // ====================================================
        // 30. EXCEPTION HANDLING MIDDLEWARE
        // ====================================================
        //
        // Keep exception handling early in the pipeline.
        //
        // ====================================================

        app.UseExceptionHandler();

        // ====================================================
        // 31. STATUS CODE PAGES
        // ====================================================
        //
        // Registered only ONCE.
        //
        // ====================================================

        app.UseStatusCodePages();

        // ====================================================
        // 32. CORS MIDDLEWARE
        // ====================================================
        //
        // IMPORTANT:
        // The registered policy is "TmsClient".
        //
        // Do NOT use:
        //
        // app.UseCors("AngularClient");
        //
        // because that policy does not exist.
        //
        // ====================================================

        app.UseCors("TmsClient");

        // ====================================================
        // 33. ROUTING
        // ====================================================

        app.UseRouting();


        // ====================================================
        // 34. RATE LIMITING MIDDLEWARE
        // ====================================================

        app.UseRateLimiter();


        // ====================================================
        // 35. AUTHENTICATION MIDDLEWARE
        // ====================================================

        app.UseAuthentication();


        // ====================================================
        // 36. AUTHORIZATION MIDDLEWARE
        // ====================================================

        app.UseAuthorization();


        // ====================================================
        // 37. XSRF / ANTIFORGERY COOKIE
        // ====================================================
        //
        // Generates the XSRF-TOKEN cookie for authenticated
        // requests.
        //
        // Angular can read this cookie because:
        //
        // HttpOnly = false
        //
        // ====================================================

        app.Use(async (context, next) =>
            {
                // Check whether the user is authenticated
                // or has the tms_auth cookie.
                if (context.User.Identity?.IsAuthenticated ==true ||context.Request.Cookies.ContainsKey( "tms_auth"))
                {
                    // Get antiforgery service.
                    var antiforgery =context.RequestServices .GetRequiredService<IAntiforgery>();

                    // Generate tokens.
                    var tokens =antiforgery.GetAndStoreTokens(context);

                    // Store token in XSRF-TOKEN cookie.
                    context.Response.Cookies.Append("XSRF-TOKEN",tokens.RequestToken!, new CookieOptions
                        {
                            // Angular must be able to read it.
                            HttpOnly = false,

                            // HTTPS outside development.
                            Secure =!builder.Environment.IsDevelopment(),

                            // Protect against cross-site requests.
                            SameSite =SameSiteMode.Strict
                        });
                }

                await next();
            });


        // ====================================================
        // 38. SECURITY HEADERS
        // ====================================================

        app.Use(async (context, next) =>
            {
                // Prevent MIME type sniffing.
                context.Response.Headers.Append( "X-Content-Type-Options", "nosniff");

                // Control referrer information.
                context.Response.Headers.Append( "Referrer-Policy","strict-origin-when-cross-origin");

                // Content Security Policy.
                context.Response.Headers.Append(
                    "Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' " +
                        "'unsafe-inline' 'unsafe-eval'; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' data: blob:; " +
                    "font-src 'self' data:; " +
                    "connect-src 'self' " +"http://localhost:5094;");
                await next();
            });


        // ====================================================
        // 39. API V1 DEPRECATION MIDDLEWARE
        // ====================================================

        app.UseMiddleware< V1DeprecationMiddleware>();

        // ====================================================
        // 40. API CONTROLLERS
        // ====================================================

        app.MapControllers();

        // ====================================================
        // 41. SIGNALR HUB
        // ====================================================
        //
        // Angular connects to:
        //
        // http://localhost:5094/hubs/tms
        //
        // IMPORTANT:
        // Hub is mapped only ONCE.
        //
        // ====================================================

        app.MapHub<TmsHub>( "/hubs/tms").RequireCors("TmsClient");

        // ====================================================
        // 42. LIVE HEALTH CHECK
        // ====================================================

        app.MapHealthChecks("/health/live").DisableRateLimiting();

        // ====================================================
        // 43. READY HEALTH CHECK
        // ====================================================

        app.MapHealthChecks("/health/ready").DisableRateLimiting();

        // ====================================================
        // 44. ASSESSMENT RESULTS
        // ====================================================
        //
        // Protected endpoint.
        //
        // Requires authentication.
        //
        // ====================================================

        app.MapGet("/api/assessments/results",() =>
                {
                    return Results.Ok( new
                        {
                            courseCode = "CS-101",

                            studentId = "S-001",

                            letterGrade = "A"
                        });
                }) .RequireAuthorization();


        // ====================================================
        // 45. ENROLLMENT WORKER SMOKE TEST
        // ====================================================

        app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
            {
                // Manually process a worker batch.
                worker.ProcessBatch();

                return Results.Ok( "processed");
            });


        // ====================================================
        // 46. ERROR TEST ENDPOINT
        // ====================================================
        //
        // Used to test GlobalExceptionHandler.
        //
        // ====================================================

        app.MapGet( "/api/error",() =>
            {
                throw new TmsDatabaseException( "Simulated database failure");
            });


        // ====================================================
        // 47. SCALAR API DOCUMENTATION
        // ====================================================
        //
        // Scalar provides a web-based API documentation UI.
        //
        // ====================================================

        app.MapScalarApiReference(options =>
            {
                // Page title.
                options.WithTitle("TMS API Reference")

                    // Scalar theme.
                    .WithTheme(ScalarTheme.DeepSpace)

                    // Default HTTP client.
                    .WithDefaultHttpClient(ScalarTarget.CSharp,ScalarClient.HttpClient);


                // API version 1.
                options.AddDocument("v1", "API Version 1.0");

                // API version 2.
                options.AddDocument( "v2", "API Version 2.0");
            });

        // ====================================================
        // 48. CRYPTO PASSWORD HASHING DEMO
        // ====================================================
        //
        // Demonstrates that hashing the same password twice
        // produces different hashes because of random salts.
        //
        // IMPORTANT:
        // Do NOT print real password hashes in production.
        //
        // ====================================================

        var cryptoService =new CryptoDemoService();

        // Hash the same password twice.
        var hash1 =cryptoService.HashUserPassword( "Password123!");

        var hash2 =cryptoService.HashUserPassword( "Password123!");

        // The hashes should be different because
        // different random salts are generated.
        Console.WriteLine( $"Hash 1: {hash1}");

        Console.WriteLine( $"Hash 2: {hash2}");

        // Verify password against first hash.
        var match1 =cryptoService.VerifyUserPassword("Password123!",hash1);

        // Verify password against second hash.
        var match2 =cryptoService.VerifyUserPassword( "Password123!", hash2);

        Console.WriteLine( $"Hash 1 verification: {match1}");

        Console.WriteLine($"Hash 2 verification: {match2}");

        // ====================================================
        // 49. START APPLICATION
        // ====================================================
        //
        // app.Run() MUST BE THE LAST STATEMENT.
        //
        // ====================================================

        app.Run();
    }
}

