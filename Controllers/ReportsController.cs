using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{
    [HttpGet("active-honor-count")]
    public async Task<IActionResult> GetActiveHonorCount()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();
        
        return Ok(new { Count = count });
    }

    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopCourses()
    {
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();
        
        return Ok(list);
    }

    [HttpGet("avg-gpa-per-course")]
    public async Task<IActionResult> GetAvgGpaPerCourse()
    {
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();
        
        return Ok(list);
    }

    // // Approach A (Using Subquery):
    // [HttpGet("unenrolled-subquery")]
    // public async Task<IActionResult> GetUnenrolledSubquery()
    // {
    //     var list = await context.Students
    //         .Where(s => !s.Enrollments.Any())
    //         .Select(s => new { s.Name, s.RegistrationNumber })
    //         .ToListAsync();
        
    //     return Ok(list);
    // }

    // Approach B (Using EF Core 10 LeftJoin):
    [HttpGet("unenrolled-leftjoin")]
    public async Task<IActionResult> GetUnenrolledLeftJoin()
    {
        var list = await context.Students
            .LeftJoin(
                context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { s, e })
            .Where(x => x.e == null)
            .Select(x => new { x.s.Name, x.s.RegistrationNumber })
            .ToListAsync();
        
        return Ok(list);
    }
}