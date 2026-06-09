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
using Microsoft.AspNetCore.Authentication.JwtBearer;
var builder = WebApplication.CreateBuilder(args);

// Services: add authentication / authorization service
builder.Services
    .AddAuthentication("Tranining")
    .AddScheme<AuthenticationSchemeOptions,
    TrainingAuthHandler>("Training", null);

builder.Services.AddAuthorization();

var app = builder.Build();

// TODO 1: Register routing in the pipeline where it belongs for your app.
app.UseRouting();

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