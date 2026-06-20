using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(s => s.GPA)
            .HasPrecision(3, 2);  // DECIMAL(3,2) — e.g., 3.85
        
        builder.HasIndex(s => s.RegistrationNumber)
            .IsUnique();  // Natural key uniqueness
    }
}