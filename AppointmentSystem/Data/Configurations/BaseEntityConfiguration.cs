using AppointmentSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Data.Configurations;

/// <summary>
/// Bütün entity configuration-ları üçün baza sinif
/// </summary>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Indexes
        builder.HasIndex(e => e.IsDeleted);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.CreatedDate);

        // Properties
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.CreatedDate).IsRequired();
        builder.Property(e => e.ModifiedDate).IsRequired(false);
    }
}

/// <summary>
/// Auditable entity-lər üçün baza configuration
/// </summary>
public abstract class AuditableEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity>
    where TEntity : AuditableEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        // Audit properties
        builder.Property(e => e.CreatedById).IsRequired(false);
        builder.Property(e => e.ModifiedById).IsRequired(false);
    }
}
