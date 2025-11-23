using AppointmentSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Data.Configurations;

public class StudentParentConfiguration : AuditableEntityConfiguration<StudentParent>
{
    public override void Configure(EntityTypeBuilder<StudentParent> builder)
    {
        base.Configure(builder);

        builder.ToTable("StudentParents");

        builder.HasIndex(e => new { e.StudentId, e.ParentId, e.ParentTypeId }).IsUnique();

        builder.HasOne(e => e.Student)
            .WithMany(s => s.StudentParents)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Parent)
            .WithMany(p => p.StudentParents)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ParentType)
            .WithMany(pt => pt.StudentParents)
            .HasForeignKey(e => e.ParentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TeacherSubjectConfiguration : AuditableEntityConfiguration<TeacherSubject>
{
    public override void Configure(EntityTypeBuilder<TeacherSubject> builder)
    {
        base.Configure(builder);

        builder.ToTable("TeacherSubjects");

        builder.HasIndex(e => new { e.TeacherId, e.SubjectId }).IsUnique();

        builder.HasOne(e => e.Teacher)
            .WithMany(t => t.TeacherSubjects)
            .HasForeignKey(e => e.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Subject)
            .WithMany(s => s.TeacherSubjects)
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TeacherClassConfiguration : AuditableEntityConfiguration<TeacherClass>
{
    public override void Configure(EntityTypeBuilder<TeacherClass> builder)
    {
        base.Configure(builder);

        builder.ToTable("TeacherClasses");

        builder.HasIndex(e => new { e.TeacherId, e.ClassId, e.SubjectId }).IsUnique();

        builder.HasOne(e => e.Teacher)
            .WithMany(t => t.TeacherClasses)
            .HasForeignKey(e => e.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Class)
            .WithMany(c => c.TeacherClasses)
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Subject)
            .WithMany()
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class CompanyUserConfiguration : AuditableEntityConfiguration<CompanyUser>
{
    public override void Configure(EntityTypeBuilder<CompanyUser> builder)
    {
        base.Configure(builder);

        builder.ToTable("CompanyUsers");

        builder.HasIndex(e => new { e.CompanyId, e.UserId }).IsUnique();

        builder.HasOne(e => e.Company)
            .WithMany(c => c.CompanyUsers)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany(u => u.CompanyUsers)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CompanySettingConfiguration : AuditableEntityConfiguration<CompanySetting>
{
    public override void Configure(EntityTypeBuilder<CompanySetting> builder)
    {
        base.Configure(builder);

        builder.ToTable("CompanySettings");

        builder.Property(e => e.WorkingDays).HasMaxLength(50);
        builder.Property(e => e.ExcludedTimeSlots).HasMaxLength(1000);
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasIndex(e => new { e.CompanyId, e.EffectiveDate });

        builder.HasOne(e => e.Company)
            .WithMany(c => c.CompanySettings)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ParentTypeConfiguration : AuditableEntityConfiguration<ParentType>
{
    public override void Configure(EntityTypeBuilder<ParentType> builder)
    {
        base.Configure(builder);

        builder.ToTable("ParentTypes");

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasIndex(e => e.Type).IsUnique();

        // Relationships
        builder.HasMany(x => x.StudentParents)
            .WithOne(x => x.ParentType)
            .HasForeignKey(x => x.ParentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CompanySubjectConfiguration : AuditableEntityConfiguration<CompanySubject>
{
    public override void Configure(EntityTypeBuilder<CompanySubject> builder)
    {
        base.Configure(builder);

        builder.ToTable("CompanySubjects");

        builder.HasIndex(e => new { e.CompanyId, e.SubjectId }).IsUnique();

        builder.HasOne(e => e.Company)
            .WithMany(c => c.CompanySubjects)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Subject)
            .WithMany(s => s.CompanySubjects)
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}