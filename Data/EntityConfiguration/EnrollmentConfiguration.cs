using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();  // One enrollment per student-course pair
        
        builder.Property(e => e.Grade)
            .HasPrecision(3, 2);
        
        // Student → Enrollment
        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);  // Delete enrollments when student deleted
        
        // Course → Enrollment
        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);  // Protect course from deletion
    }

    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}