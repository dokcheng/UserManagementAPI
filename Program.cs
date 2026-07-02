using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi;
using UserManagementAPI.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ==========================================
// MIDDLEWARE PIPELINE CONFIGURATION (Step 5)
// Order matters! Error Handling -> Authentication -> Logging
// ==========================================
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

app.UseHttpsRedirection();

// ==========================================
// In-Memory Data Store & Validation
// ==========================================
var users = new List<User>
{
    new User { Id = 1, Name = "Alice Smith", Email = "alice.smith@techhive.com", Department = "HR" },
    new User { Id = 2, Name = "Bob Jones", Email = "bob.jones@techhive.com", Department = "IT" }
};

int nextUserId = 3; 

static bool IsValid(User user, out List<string> errors)
{
    var context = new ValidationContext(user);
    var results = new List<ValidationResult>();
    bool isValid = Validator.TryValidateObject(user, context, results, true);
    
    errors = results.Select(r => r.ErrorMessage ?? "Validation error").ToList();
    return isValid;
}

// ==========================================
// API Endpoints
// ==========================================
var api = app.MapGroup("/api/users").WithTags("User Management");

api.MapGet("/", () => Results.Ok(users));

api.MapGet("/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is not null ? Results.Ok(user) : Results.NotFound(new { Message = $"User with ID {id} not found." });
});

api.MapPost("/", (User newUser) =>
{
    if (!IsValid(newUser, out var errors))
    {
        return Results.BadRequest(new { Message = "Invalid user data", Errors = errors });
    }

    newUser.Id = nextUserId++;
    users.Add(newUser);
    return Results.Created($"/api/users/{newUser.Id}", newUser);
});

api.MapPut("/{id:int}", (int id, User updatedUser) =>
{
    if (!IsValid(updatedUser, out var errors))
    {
        return Results.BadRequest(new { Message = "Invalid user data", Errors = errors });
    }

    var existingUser = users.FirstOrDefault(u => u.Id == id);
    if (existingUser is null) return Results.NotFound(new { Message = $"User with ID {id} not found." });

    existingUser.Name = updatedUser.Name;
    existingUser.Email = updatedUser.Email;
    existingUser.Department = updatedUser.Department;

    return Results.NoContent();
});

api.MapDelete("/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound(new { Message = $"User with ID {id} not found." });

    users.Remove(user);
    return Results.NoContent();
});

// Endpoint specifically designed to test the Error Handling Middleware
api.MapGet("/trigger-error", () => 
{
    throw new Exception("This is a simulated unexpected error!");
});

app.Run();

// ==========================================
// CUSTOM MIDDLEWARE CLASSES
// ==========================================

// Step 3: Error Handling Middleware
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception)
        {
            // Catch unhandled exceptions and return a consistent JSON response
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Internal server error." });
        }
    }
}

// Step 4: Authentication Middleware
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for Swagger UI so documentation remains accessible
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        // Validate token (using a hardcoded mock token for this activity)
        if (!context.Request.Headers.TryGetValue("Authorization", out var extractedToken) || 
            extractedToken != "Bearer techhive-secret-token")
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized. Invalid or missing token." });
            return;
        }

        await _next(context);
    }
}

// Step 2: Logging Middleware
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Let the request pass through the pipeline first so we can log the final status code
        await _next(context);

        // Log: HTTP Method, Request Path, Response Status Code
        _logger.LogInformation(
            "TechHive Audit Log - Method: {Method}, Path: {Path}, Status: {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode);
    }
}