using AppointmentSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Data.Configurations;

public class SubjectConfiguration : AuditableEntityConfiguration<Subject>
{
    public override void Configure(EntityTypeBuilder<Subject> builder)
    {
        base.Configure(builder);

        builder.ToTable("Subjects");

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Code).HasMaxLength(50);
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasIndex(e => e.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
    }
}