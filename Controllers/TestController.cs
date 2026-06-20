using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> Step 1: Building the query object (no database contact)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);

        Console.WriteLine(">>> Step 2: Appending a storing clause...");
        var orderedQuery = query.OrderBy(s => s.Name);

        Console.WriteLine(">>> Step 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList(); // Execution is triggered here

        Console.WriteLine(">>> Step 4: Materialization finished. List populated.\n");
        return Ok(results);
    }

    // Non-translatable helper method
    private static bool IsHonorRoll(decimal gpa) => gpa >= 3.5m;

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> Step 1: Running not-translatable query...");
        try
        {
            var students = context.Students
                .Where(s => IsHonorRoll(s.GPA)) // EF Core does not know how to map this method to SQL
                .ToList();
            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
            return BadRequest(new { Message = ex.Message});
        }
    }
}