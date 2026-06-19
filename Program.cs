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
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

using Scalar.AspNetCore;
using TmsApi.Entities;

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

// builder.Services.AddOptions<PaymentOptions>()
//     .BindConfiguration("Payments")
//     .ValidateDataAnnotations()
//     .ValidateOnStart();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(); // Required before MapOpenApi() will work

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Register TmsDbContext scoped for incoming HTTP requests
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
        .LogTo(Console.WriteLine, LogLevel.Information) // Log SQL to output window
        .EnableSensitiveDataLogging()); // Show parameters in query logs (dev only)

var app = builder.Build();

builder.Services.AddProblemDetails();

// TODO 1: Register routing in the pipeline where it belongs for your app.
app.UseRouting();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler("/error");

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// TODO 2: Register authentication and authorization in the pipeline where your template and facilitator expect them for a protected minimal API route.
app.UseAuthentication();
app.UseAuthorization();

// Program.cs — middleware section

if (app.Environment.IsDevelopment())
{
    // Development only: expose OpenAPI document and interactive explorer
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

// TODO 3: Map GET /api/assessments/results with the same response body as the starter, but require authorization for that route.
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}))
.RequireAuthorization();

app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

app.MapControllers();

// Seed test data at startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate(); // Applies any pending migrations; keeps migration history intact

    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith", GPA = 3.8m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
            new() { RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince", GPA = 3.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright", GPA = 2.5m, IsActive = true },
        };
        context.Students.AddRange(students);

        var courses = new List<Course>
        {
            new() { Code = "CS-101", Title = "Introduction to Computer Science", Capacity = 30 },
            new() { Code = "CS-201", Title = "Data Structures and Algorithms", Capacity = 25 },
            new() { Code = "MAT-101", Title = "Calculus I", Capacity = 40 }
        };
        context.Courses.AddRange(courses);
        context.SaveChanges();

        var enrollments = new List<Enrollment>
        {
            new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
            new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
            new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
            new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m },
        };
        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }
}

app.Run();

// ==== Exercise 1B: Custom Request Logging Middleware ====

// Check RequestLoggingMiddlewares.cs for "class RequestLoggingMiddleware" and "readonly ILogger<RequestLoggingMiddleware>"

// Updated Program.cs -> Add app.UseMiddleware<RequestLoggingMiddleware>();
// Updated Program.cs -> Add app.UseExceptionHandler("/error");
// Updated Program.cs -> Add app.UseHttpsRedirection();


// ==== Exercise 2: The Memory Leak (Captive Dependencies) ====

// First, make the failure visible
// Updated Program.cs -> Add builder.Services.AddSingleton<EnrollmentWorker>();
// Updated Program.cs -> Add builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
// Check ./Workers/EnrollmentWorker.cs for "class EnrollmentWorker(IServiceScopeFactory scopeFactory)"


// ==== Exercise 3: The Silent Crash (Options Pattern) ====

// Check ./Configurations/Payment
// Updated Program.cs -> Add builder.Services.AddOptions<PaymentOptions>();


// ==== Exercise 4: The Unreachable Logs (Structured Logging) ====

// Updated ./Services/EnrollmentService.cs methods EnrollAsync, GetByIdAsync, DeleteAsync


// ==== Exercise 5: The Enrollment API (Controllers with Real CRUD) ====

// Check ./Controllers/EnrollmentController.cs for the Enrollment APIs
// Part A: Get Endpoints
// Part B: POST with 201 + Location
// Part C: DELETE with 204/404


// ==== Exercise 6: The Consistent Fault (Standardized Error Handling) ====

    // TODO 1: Check if the app is running in Development mode.
    // Stuck? if (app.Environment.IsDevelopment()) { ... }
    // TODO 2: In Development only expose the OpenAPI document and an interactive API explorer.
    // Use the built-in MapOpenApi() and MapScalarApiReference().
    // Stuck? app.MapOpenApi(); app.MapScalarApiReference();
    // TODO 3: In Production use the exception handler middleware so stack traces
    // are never shown to external users.
    // Stuck? app.UseExceptionHandler();
    // TODO 4: Run in both environments and verify:
    // - In Development: can you browse /scalar/v1 and see your endpoints?
    // - In Production: does a thrown exception return ProblemDetails JSON, not a stack trace?


// ==== Exercise 7: The Environment Toggle (Dev vs Prod) ====

    // TODO 1: Check if the app is running in Development mode.
    // Stuck? if (app.Environment.IsDevelopment()) { ... }
    // TODO 2: In Development only expose the OpenAPI document and an interactive API explorer.
    // Use the built-in MapOpenApi() and MapScalarApiReference().
    // Stuck? app.MapOpenApi(); app.MapScalarApiReference();
    // TODO 3: In Production use the exception handler middleware so stack traces
    // are never shown to external users.
    // Stuck? app.UseExceptionHandler();
    // TODO 4: Run in both environments and verify:
    // - In Development: can you browse /scalar/v1 and see your endpoints?
    // - In Production: does a thrown exception return ProblemDetails JSON, not a stack trace?


// ==== Exercise 1: Configure TmsDbContext and Apply the First Migration ====

// == Step 1: Define Your Database Entities ==
// Create folder named Entities
// Create Student.cs, Course.cs, Enrollment.cs, Assessment.cs, Certificate.cs

// == Step 2: Implement TmsDbContext ==
// Create folder named Data and add TmsDbContext.cs

// == Step 3: Register the DbContext in Program.cs ==
// Add DbContext registration using Npgsql

// == Step 4: Configure Connection String ==
// Add ConnectionStrings block at the root level of appsettings.Development.json

// == Step 5: Generate the First Migration ==
// dotnet ef migrations add InitialCreate

// == Step 6: Inspect the Generated Migration File ==

// == Step 7: Apply the Migration ==
// dotnet ef database update

// == Step 8: Verify the Schema in PostgreSQL ==
// psql -U postgres -d TmsDb -c "\dt"


// ==== Exercise 2: The LINQ Engine Logging, Deferred Execution, and Translation Limits ====

// == Step 1: Enable Console SQL Logging ==
// Update AddDbContext registration in Program.cs

// == Step 2: Write an Auto-Seeder ==
// Ensure your database contains records to query, add this temporary seeding block inside Program.cs

// == Step 3: Run the Deferred Execution Experiment ==
// Create a temporary API controller

// == Step 4: Run the SQL Translation Failure Experiment ==
// Add helper C# method and a new endpoint to TestController class

// == Step 5: Solve the Registrar's Business Queries ==
// Write endpoints to solve the four registrar requests:
//  How many active students have GPA >= 3.0?
//  Which courses have the most enrollments, sorted descending?
//  What is the average GPA per course?
//  Which students have zero enrollments?

// == Extended Exercise (Stretch): Wire Assessment and Certificate into the Database ==
// Task: Register Assessment and Certificate entities
//       Generate a new migration
//       Inspect it before applying
//       Apply and verify