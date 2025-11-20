using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        //await context.Database.MigrateAsync();

        // 1. Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new[]
            {
                new Role { Id = Guid.NewGuid(), Name = "SuperAdmin", Code = "SUPERADMIN", IsSystemRole = true, Priority = 100, Description = "Sistem Administratoru" },
                new Role { Id = Guid.NewGuid(), Name = "Manager", Code = "MANAGER", IsSystemRole = true, Priority = 80, Description = "Şirkət Meneceri" },
                new Role { Id = Guid.NewGuid(), Name = "Teacher", Code = "TEACHER", IsSystemRole = true, Priority = 50, Description = "Müəllim" },
                new Role { Id = Guid.NewGuid(), Name = "Parent", Code = "PARENT", IsSystemRole = true, Priority = 30, Description = "Valideyn" }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. UserTypes
        if (!await context.UserTypes.AnyAsync())
        {
            var userTypes = new[]
            {
                new UserType { Id = Guid.NewGuid(), Name = "System Admin", Description = "Sistem Administratoru" },
                new UserType { Id = Guid.NewGuid(), Name = "Manager", Description = "Şirkət Meneceri" },
                new UserType { Id = Guid.NewGuid(), Name = "Teacher", Description = "Müəllim" },
                new UserType { Id = Guid.NewGuid(), Name = "Parent", Description = "Valideyn" }
            };
            await context.UserTypes.AddRangeAsync(userTypes);
            await context.SaveChangesAsync();
        }

        // 3. SuperAdmin User
        if (!await context.Users.AnyAsync(u => u.UserName == "admin"))
        {
            var adminUserId = Guid.NewGuid();
            var superAdminRole = await context.Roles.FirstAsync(r => r.Code == "SUPERADMIN");
            var adminUserType = await context.UserTypes.FirstAsync(ut => ut.Name == "System Admin");

            var adminUser = new User
            {
                Id = adminUserId,
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@appointmentsystem.az",
                UserName = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                PhoneNumber = "+994501234567",
                IsEmailConfirmed = true,
                UserTypeId = adminUserType.Id
            };

            await context.Users.AddAsync(adminUser);
            await context.UserRoles.AddAsync(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = adminUserId,
                RoleId = superAdminRole.Id,
                AssignedDate = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // 4. Sample Company
        if (!await context.Companies.AnyAsync())
        {
            var companyId = Guid.NewGuid();
            var company = new Company
            {
                Id = companyId,
                Name = "Demo Təhsil Mərkəzi",
                Code = "DEMO001",
                Address = "Nəsimi rayonu, Bakı, Azərbaycan",
                Phone = "+994123456789",
                Email = "info@demomekteb.az",
                Description = "Demo məqsədilə yaradılmış təhsil mərkəzi",
                DefaultMeetingDuration = 30,
                DefaultBreakDuration = 10,
                DefaultStartTime = new TimeSpan(9, 0, 0),
                DefaultEndTime = new TimeSpan(17, 0, 0)
            };

            await context.Companies.AddAsync(company);
            await context.SaveChangesAsync();

            // 5. Sample Class
            var classId = Guid.NewGuid();
            var schoolClass = new SchoolClass
            {
                Id = classId,
                Name = "10-A",
                Level = 10,
                Section = "A",
                CompanyId = companyId
            };

            await context.Classes.AddAsync(schoolClass);
            await context.SaveChangesAsync();

            // 6. Sample Subject
            var subjectId = Guid.NewGuid();
            var subject = new Subject
            {
                Id = subjectId,
                Name = "Riyaziyyat",
                Code = "MATH",
                Description = "Ümumi riyaziyyat fənni"
            };

            await context.Subjects.AddAsync(subject);
            await context.CompanySubjects.AddAsync(new CompanySubject
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                SubjectId = subjectId
            });
            await context.SaveChangesAsync();
        }

        Console.WriteLine("✅ Seed data successfully initialized!");
    }
}
