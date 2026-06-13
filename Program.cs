// ==== Exercise 1 - The Blind Server (Middleware Ordering) ====

// // == Predict before you change code ==
// // Starter pipeline (do nos assume this order is correct)
// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.UseRouting();

// app.MapGet("/api/assessment/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// }));

// app.UseAuthentication();
// app.UseAuthorization();

// app.Run();

// == Task: Secure the pipeline ==
using Microsoft.AspNetCore.Authentication;
var builder = WebApplication.CreateBuilder(args);

// Services: add authentication / authorization service
builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training",
        options => { }
    );

builder.Services.AddAuthorization();

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var app = builder.Build();

// TODO 1: Register routing in the pipeline where it belongs for your app.
app.UseRouting();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler("/error");

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// TODO 2: Register authentication and authorization in the pipeline where your template and facilitator expect them for a protected minimal API route.
app.UseAuthentication();
app.UseAuthorization();

// TODO 3: Map GET /api/assessments/results with the same response body as the starter, but require authorization for that route.
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}))
.RequireAuthorization();

app.Run();

// ==== Exercise 1B: Custom Request Logging Middleware ====

// Check RequestLoggingMiddlewares.cs for "class RequestLoggingMiddleware" and "readonly ILogger<RequestLoggingMiddleware>"

// Updated Program.cs -> Add app.UseMiddleware<RequestLogginMiddleware>();
// Updated Program.cs -> Add app.UseExceptionHandler("/error");
// Updated Program.cs -> Add app.UseHttpsRedirection();


// ==== Exercise 2: The Memory Leak (Captive Dependencies) ====

// First, make the failure visible
// Updated Program.cs -> Add builder.Services.AddSingleton<EnrollmentWorker>();
// Updated Program.cs -> Add builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
// Check ./Workers/EnrollmentWorker.cs for "class EnrollmentWorker(IServiceScopeFactory scopeFactory)"

