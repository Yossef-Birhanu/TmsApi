using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Tms.Api.Authorization;
using TmsApi.Domain.Entities;

namespace TmsApi.Api.Authorization;

public sealed class CourseInstructorHandler
    : AuthorizationHandler<CourseInstructorRequirement, Course>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseInstructorRequirement requirement,
        Course resource)
    {
        // Get the authenticated user's ID from the JWT.
        var userId = context.User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // Get user's roles.
        var isInstructor = context.User.IsInRole("Instructor");
        var isAdmin = context.User.IsInRole("Admin");

        // ---------------------------------------------------------
        // ADMIN
        // ---------------------------------------------------------
        // Admins can manage any course.
        if (isAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // ---------------------------------------------------------
        // INSTRUCTOR
        // ---------------------------------------------------------
        // Instructor can manage only courses assigned to them.
        if (isInstructor &&
            !string.IsNullOrWhiteSpace(userId) &&
            resource.InstructorId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}