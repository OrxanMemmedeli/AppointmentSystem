using AppointmentSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Data.Configurations;

public class SchoolClassConfiguration : AuditableEntityConfiguration<SchoolClass>
{
    public override void Configure(EntityTypeBuilder<SchoolClass> builder)
    {
        base.Configure(builder);

        builder.ToTable("Classes");

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Section).HasMaxLength(10);
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();

        builder.HasOne(e => e.Company)
            .WithMany(c => c.Classes)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}