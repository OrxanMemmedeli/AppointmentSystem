using AppointmentSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Data.Configurations;

public class ParentConfiguration : AuditableEntityConfiguration<Parent>
{
    public override void Configure(EntityTypeBuilder<Parent> builder)
    {
        base.Configure(builder);

        builder.ToTable("Parents");

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.FinCode).IsRequired().HasMaxLength(7).IsFixedLength();
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(100);
        builder.Property(e => e.ImagePath).HasMaxLength(500);

        builder.HasIndex(e => new { e.CompanyId, e.FinCode }).IsUnique();

        builder.HasOne(e => e.Company)
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithOne(u => u.Parent)
            .HasForeignKey<Parent>(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}