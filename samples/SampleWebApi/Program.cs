using Handlr.Abstractions.Common;
using Handlr.Abstractions.Pipelines;
using Handlr.Abstractions.Results;
using Microsoft.AspNetCore.Mvc;
using SampleWebApi.Behaviors;
using SampleWebApi.Commands;
using SampleWebApi.Queries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// 🚀 Add Handlr CQRS Framework
builder.Services.AddHandlr();

// 🔧 Register Pipeline Behaviors (Global Registration)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));

// 📊 Add additional services
builder.Services.AddMemoryCache();
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Swagger UI removed for simplicity - can be added with Swashbuckle.AspNetCore package
}

app.UseHttpsRedirection();

// 🎯 COMMAND ENDPOINTS (Actions that change state)

// POST /api/users - Create a new user
app.MapPost("/api/users", async ([FromServices] IHandlrDispatcher handlr, [FromBody] CreateUserRequest request) =>
{
    var command = new CreateUserCommand
    {
        Name = request.Name,
        Email = request.Email,
        Age = request.Age
    };

    var result = await handlr.SendAsync<Result<int>>(command);

    return result.IsSuccess
        ? Results.Created($"/api/users/{result.Value}", new { Id = result.Value, Message = "User created successfully" })
        : Results.BadRequest(new { Error = result.FirstError?.Message ?? "Bad request" });
})
.WithName("CreateUser")
.WithSummary("Create a new user")
.WithDescription("Creates a new user and returns the generated user ID");

// PUT /api/users/{id}/status - Update user status
app.MapPut("/api/users/{id}/status", async ([FromServices] IHandlrDispatcher handlr, int id, [FromBody] UpdateStatusRequest request) =>
{
    var command = new UpdateUserStatusCommand
    {
        UserId = id,
        Status = request.Status
    };

    var result = await handlr.SendAsync<Result>(command);

    return result.IsSuccess
        ? Results.Ok(new { Message = "Status updated successfully" })
        : Results.BadRequest(new { Error = result.FirstError?.Message ?? "Bad request" });
})
.WithName("UpdateUserStatus")
.WithSummary("Update user status")
.WithDescription("Updates the status of an existing user");

// POST /api/reports - Generate a report
app.MapPost("/api/reports", async ([FromServices] IHandlrDispatcher handlr, [FromBody] GenerateReportRequest request) =>
{
    var command = new GenerateReportCommand
    {
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        ReportType = request.ReportType
    };

    var result = await handlr.SendAsync<Result<string>>(command);

    return result.IsSuccess
        ? Results.Ok(new { ReportData = result.Value, Message = "Report generated successfully" })
        : Results.BadRequest(new { Error = result.FirstError?.Message ?? "Bad request" });
})
.WithName("GenerateReport")
.WithSummary("Generate a report")
.WithDescription("Generates a report based on the specified criteria");

// 🔍 QUERY ENDPOINTS (Read operations)

// GET /api/users/{id} - Get user by ID
app.MapGet("/api/users/{id}", async ([FromServices] IHandlrDispatcher handlr, int id) =>
{
    var query = new GetUserByIdQuery { UserId = id };
    var result = await handlr.SendAsync(query);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.NotFound(new { Error = result.FirstError?.Message ?? "User not found" });
})
.WithName("GetUserById")
.WithSummary("Get user by ID")
.WithDescription("Retrieves a user by their unique identifier");

// GET /api/users - Get users with filtering
app.MapGet("/api/users", async ([FromServices] IHandlrDispatcher handlr,
    [FromQuery] string nameFilter = null,
    [FromQuery] int minAge = 0,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10) =>
{
    var query = new GetUsersQuery
    {
        NameFilter = nameFilter,
        MinAge = minAge,
        Page = page,
        PageSize = pageSize
    };

    var result = await handlr.SendAsync(query);

    return result.IsSuccess
        ? Results.Ok(new
        {
            Users = result.Value,
            Page = page,
            PageSize = pageSize,
            TotalCount = result.Value?.Count ?? 0
        })
        : Results.BadRequest(new { Error = result.FirstError?.Message ?? "Bad request" });
})
.WithName("GetUsers")
.WithSummary("Get users with filtering")
.WithDescription("Retrieves a paginated list of users with optional filtering");

// GET /api/health - Health check endpoint
app.MapGet("/api/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Framework = "Handlr CQRS",
    Version = "1.0.0"
}))
.WithName("HealthCheck")
.WithSummary("Health check")
.WithDescription("Returns the health status of the API");

// 📖 DOCUMENTATION ENDPOINT
app.MapGet("/", () => Results.Redirect("/swagger"))
.ExcludeFromDescription();

app.Run();

// 📋 REQUEST/RESPONSE MODELS
record CreateUserRequest(string Name, string Email, int Age);
record UpdateStatusRequest(string Status);
record GenerateReportRequest(DateTime StartDate, DateTime EndDate, string ReportType);
