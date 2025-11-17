using AppointmentSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Data.Configurations;

public class CompanyConfiguration : AuditableEntityConfiguration<Company>
{
    public override void Configure(EntityTypeBuilder<Company> builder)
    {
        base.Configure(builder);

        builder.ToTable("Companies");

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.Website).HasMaxLength(200);
        builder.Property(e => e.LogoPath).HasMaxLength(500);
        builder.Property(e => e.BackgroundImagePath).HasMaxLength(500);
        builder.Property(e => e.MapUrl).HasMaxLength(1000);
        builder.Property(e => e.MapCoordinates).HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.WorkingDays).HasMaxLength(50);
        builder.Property(e => e.DefaultMeetingDuration).HasDefaultValue(30);
        builder.Property(e => e.DefaultBreakDuration).HasDefaultValue(10);

        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.Name);
    }
}