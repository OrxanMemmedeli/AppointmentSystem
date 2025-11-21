using AppointmentSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Data.Configurations;

public class TeacherConfiguration : AuditableEntityConfiguration<Teacher>
{
    public override void Configure(EntityTypeBuilder<Teacher> builder)
    {
        base.Configure(builder);

        builder.ToTable("Teachers");

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.Property(e => e.ImagePath).HasMaxLength(500);
        builder.Property(e => e.Specialization).HasMaxLength(200);
        builder.Property(e => e.Biography).HasMaxLength(2000);

        builder.HasIndex(e => new { e.CompanyId, e.Email }).IsUnique();

        builder.HasOne(e => e.Company)
            .WithMany(c => c.Teachers)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithOne(u => u.Teacher)
            .HasForeignKey<Teacher>(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}