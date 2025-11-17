using AppointmentSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Data.Configurations;

public class StudentConfiguration : AuditableEntityConfiguration<Student>
{
    public override void Configure(EntityTypeBuilder<Student> builder)
    {
        base.Configure(builder);

        builder.ToTable("Students");

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.FinCode).IsRequired().HasMaxLength(7).IsFixedLength();
        builder.Property(e => e.ImagePath).HasMaxLength(500);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasIndex(e => new { e.CompanyId, e.FinCode }).IsUnique();

        builder.HasOne(e => e.Company)
            .WithMany(c => c.Students)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Class)
            .WithMany(c => c.Students)
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}