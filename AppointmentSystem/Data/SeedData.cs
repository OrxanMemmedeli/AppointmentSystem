using AppointmentSystem.Models.Entities;
using AppointmentSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Data;

/// <summary>
/// Seed Data - İlkin məlumatlar
/// Test və development üçün tam işləyən sistem
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        try
        {
            Console.WriteLine("🔄 Seed data başladı...");

            // 1. Roles
            await SeedRolesAsync(context);

            // 2. UserTypes
            await SeedUserTypesAsync(context);

            // 3. SuperAdmin User
            await SeedSuperAdminAsync(context);

            // 4. Sample Company
            var companyId = await SeedCompanyAsync(context);

            if (companyId.HasValue)
            {
                // 5. ParentTypes (Students-dən əvvəl)
                await SeedParentTypesAsync(context);

                // 6. Sample Classes
                var classIds = await SeedClassesAsync(context, companyId.Value);

                // 7. Sample Subjects
                var subjectIds = await SeedSubjectsAsync(context, companyId.Value);

                // 8. Sample Manager (Company Admin)
                await SeedManagerAsync(context, companyId.Value);

                // 9. Sample Teachers
                var teacherIds = await SeedTeachersAsync(context, companyId.Value, subjectIds, classIds);

                // 10. Sample Parents
                var parentIds = await SeedParentsAsync(context, companyId.Value);

                // 11. Sample Students
                await SeedStudentsAsync(context, companyId.Value, classIds, parentIds);

                // 12. Menu Structure
                await SeedMenusAsync(context);
            }

            Console.WriteLine("✅ Seed data uğurla tamamlandı!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Seed data xətası: {ex.Message}");
            throw;
        }
    }

    #region 1. Roles

    private static async Task SeedRolesAsync(AppDbContext context)
    {
        if (await context.Roles.AnyAsync())
        {
            Console.WriteLine("⏭️ Roles artıq mövcuddur");
            return;
        }

        var roles = new[]
        {
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "SuperAdmin",
                Code = "SUPERADMIN",
                Description = "Sistem Administratoru - Bütün hüquqlara malikdir",
                IsSystemRole = true,
                Priority = 100,
                IsActive = true
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Manager",
                Code = "MANAGER",
                Description = "Şirkət Meneceri - Şirkət daxili tam idarəetmə",
                IsSystemRole = true,
                Priority = 80,
                IsActive = true
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Teacher",
                Code = "TEACHER",
                Description = "Müəllim - Görüş və sinif idarəetməsi",
                IsSystemRole = true,
                Priority = 50,
                IsActive = true
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Parent",
                Code = "PARENT",
                Description = "Valideyn - Görüş təyin etmə və izləmə",
                IsSystemRole = true,
                Priority = 30,
                IsActive = true
            }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {roles.Length} role əlavə edildi");
    }

    #endregion

    #region 2. UserTypes

    private static async Task SeedUserTypesAsync(AppDbContext context)
    {
        if (await context.UserTypes.AnyAsync())
        {
            Console.WriteLine("⏭️ UserTypes artıq mövcuddur");
            return;
        }

        var userTypes = new[]
        {
            new UserType { Id = Guid.NewGuid(), Name = "System Admin", Description = "Sistem Administratoru", IsActive = true },
            new UserType { Id = Guid.NewGuid(), Name = "Manager", Description = "Şirkət Meneceri", IsActive = true },
            new UserType { Id = Guid.NewGuid(), Name = "Teacher", Description = "Müəllim", IsActive = true },
            new UserType { Id = Guid.NewGuid(), Name = "Parent", Description = "Valideyn", IsActive = true }
        };

        await context.UserTypes.AddRangeAsync(userTypes);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {userTypes.Length} user type əlavə edildi");
    }

    #endregion

    #region 3. SuperAdmin

    private static async Task SeedSuperAdminAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.UserName == "admin"))
        {
            Console.WriteLine("⏭️ SuperAdmin artıq mövcuddur");
            return;
        }

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
            UserTypeId = adminUserType.Id,
            IsActive = true
        };

        await context.Users.AddAsync(adminUser);
        await context.UserRoles.AddAsync(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = adminUserId,
            RoleId = superAdminRole.Id,
            AssignedDate = DateTime.Now,
            IsActive = true
        });

        await context.SaveChangesAsync();
        Console.WriteLine("✅ SuperAdmin yaradıldı (admin / Admin123!)");
    }

    #endregion

    #region 4. Sample Company

    private static async Task<Guid?> SeedCompanyAsync(AppDbContext context)
    {
        if (await context.Companies.AnyAsync())
        {
            Console.WriteLine("⏭️ Company artıq mövcuddur");
            return await context.Companies.Select(c => c.Id).FirstAsync();
        }

        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Name = "Demo Təhsil Mərkəzi",
            Code = "DEMO001",
            Address = "Nəsimi rayonu, Ü.Hacıbəyov 25, Bakı, Azərbaycan",
            PhoneNumber = "+994123456789",
            Email = "info@demomekteb.az",
            Website = "https://demomekteb.az",
            Description = "Demo məqsədilə yaradılmış tam funksional təhsil mərkəzi",
            DefaultMeetingDuration = 30,
            DefaultBreakDuration = 10,
            DefaultStartTime = new TimeSpan(9, 0, 0),
            DefaultEndTime = new TimeSpan(17, 0, 0),
            IsActive = true,
            CreatedDate = DateTime.Now
        };

        await context.Companies.AddAsync(company);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Company yaradıldı: {company.Name}");

        return companyId;
    }

    #endregion

    #region 5. ParentTypes

    private static async Task SeedParentTypesAsync(AppDbContext context)
    {
        if (await context.ParentTypes.AnyAsync())
        {
            Console.WriteLine("⏭️ ParentTypes artıq mövcuddur");
            return;
        }

        var parentTypes = new[]
        {
            new ParentType
            {
                Id = Guid.NewGuid(),
                Name = "Ata",
                Type = ParentRelationType.Father,
                Description = "Atalar",
                IsActive = true
            },
            new ParentType
            {
                Id = Guid.NewGuid(),
                Name = "Ana",
                Type = ParentRelationType.Mother,
                Description = "Analar",
                IsActive = true
            },
            new ParentType
            {
                Id = Guid.NewGuid(),
                Name = "Qəyyum",
                Type = ParentRelationType.Other, // ✅ Guardian yoxdur, Other istifadə et
                Description = "Qanuni nümayəndələr",
                IsActive = true
            }
        };

        await context.ParentTypes.AddRangeAsync(parentTypes);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {parentTypes.Length} valideyn tipi əlavə edildi");
    }

    #endregion

    #region 6. Sample Classes

    private static async Task<List<Guid>> SeedClassesAsync(AppDbContext context, Guid companyId)
    {
        if (await context.Classes.AnyAsync(c => c.CompanyId == companyId))
        {
            Console.WriteLine("⏭️ Classes artıq mövcuddur");
            return await context.Classes.Where(c => c.CompanyId == companyId).Select(c => c.Id).ToListAsync();
        }

        var classes = new[]
        {
            new SchoolClass
            {
                Id = Guid.NewGuid(),
                Name = "9-A",
                Level = 9,
                Section = "A",
                Description = "9-cu sinif A şöbəsi",
                CompanyId = companyId,
                IsActive = true
            },
            new SchoolClass
            {
                Id = Guid.NewGuid(),
                Name = "9-B",
                Level = 9,
                Section = "B",
                Description = "9-cu sinif B şöbəsi",
                CompanyId = companyId,
                IsActive = true
            },
            new SchoolClass
            {
                Id = Guid.NewGuid(),
                Name = "10-A",
                Level = 10,
                Section = "A",
                Description = "10-cu sinif A şöbəsi",
                CompanyId = companyId,
                IsActive = true
            },
            new SchoolClass
            {
                Id = Guid.NewGuid(),
                Name = "10-B",
                Level = 10,
                Section = "B",
                Description = "10-cu sinif B şöbəsi",
                CompanyId = companyId,
                IsActive = true
            },
            new SchoolClass
            {
                Id = Guid.NewGuid(),
                Name = "11-A",
                Level = 11,
                Section = "A",
                Description = "11-ci sinif A şöbəsi",
                CompanyId = companyId,
                IsActive = true
            }
        };

        await context.Classes.AddRangeAsync(classes);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {classes.Length} sinif əlavə edildi");

        return classes.Select(c => c.Id).ToList();
    }

    #endregion

    #region 7. Sample Subjects

    private static async Task<List<Guid>> SeedSubjectsAsync(AppDbContext context, Guid companyId)
    {
        if (await context.Subjects.AnyAsync())
        {
            Console.WriteLine("⏭️ Subjects artıq mövcuddur");
            return await context.CompanySubjects
                .Where(cs => cs.CompanyId == companyId)
                .Select(cs => cs.SubjectId)
                .ToListAsync();
        }

        var subjects = new[]
        {
            new Subject { Id = Guid.NewGuid(), Name = "Riyaziyyat", Code = "MATH", Description = "Ümumi riyaziyyat", IsActive = true },
            new Subject { Id = Guid.NewGuid(), Name = "Fizika", Code = "PHYS", Description = "Ümumi fizika", IsActive = true },
            new Subject { Id = Guid.NewGuid(), Name = "Kimya", Code = "CHEM", Description = "Ümumi kimya", IsActive = true },
            new Subject { Id = Guid.NewGuid(), Name = "Biologiya", Code = "BIO", Description = "Ümumi biologiya", IsActive = true },
            new Subject { Id = Guid.NewGuid(), Name = "Tarix", Code = "HIST", Description = "Azərbaycan və dünya tarixi", IsActive = true },
            new Subject { Id = Guid.NewGuid(), Name = "Coğrafiya", Code = "GEO", Description = "Ümumi coğrafiya", IsActive = true },
            new Subject { Id = Guid.NewGuid(), Name = "İngilis dili", Code = "ENG", Description = "İngilis dili", IsActive = true }
        };

        await context.Subjects.AddRangeAsync(subjects);
        await context.SaveChangesAsync();

        // Company-Subject əlaqəsi
        var companySubjects = subjects.Select(s => new CompanySubject
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            SubjectId = s.Id,
            IsActive = true
        }).ToList();

        await context.CompanySubjects.AddRangeAsync(companySubjects);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ {subjects.Length} fənn əlavə edildi");
        return subjects.Select(s => s.Id).ToList();
    }

    #endregion

    #region 8. Sample Manager

    private static async Task SeedManagerAsync(AppDbContext context, Guid companyId)
    {
        if (await context.Users.AnyAsync(u => u.UserName == "manager"))
        {
            Console.WriteLine("⏭️ Manager artıq mövcuddur");
            return;
        }

        var managerUserId = Guid.NewGuid();
        var managerRole = await context.Roles.FirstAsync(r => r.Code == "MANAGER");
        var managerUserType = await context.UserTypes.FirstAsync(ut => ut.Name == "Manager");

        var managerUser = new User
        {
            Id = managerUserId,
            FirstName = "Elçin",
            LastName = "Məmmədov",
            Email = "manager@demomekteb.az",
            UserName = "manager",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"),
            PhoneNumber = "+994551234567",
            IsEmailConfirmed = true,
            UserTypeId = managerUserType.Id,
            IsActive = true
        };

        await context.Users.AddAsync(managerUser);

        // CompanyUser (Manager-Company əlaqəsi)
        await context.CompanyUsers.AddAsync(new CompanyUser
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            UserId = managerUserId,
            IsActive = true
        });

        // UserRole
        await context.UserRoles.AddAsync(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = managerUserId,
            RoleId = managerRole.Id,
            AssignedDate = DateTime.Now,
            IsActive = true
        });

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Manager yaradıldı (manager / Manager123!)");
    }

    #endregion

    #region 9. Sample Teachers

    private static async Task<List<Guid>> SeedTeachersAsync(
        AppDbContext context,
        Guid companyId,
        List<Guid> subjectIds,
        List<Guid> classIds)
    {
        if (await context.Teachers.AnyAsync(t => t.CompanyId == companyId))
        {
            Console.WriteLine("⏭️ Teachers artıq mövcuddur");
            return await context.Teachers.Where(t => t.CompanyId == companyId).Select(t => t.Id).ToListAsync();
        }

        var teacherRole = await context.Roles.FirstAsync(r => r.Code == "TEACHER");
        var teacherUserType = await context.UserTypes.FirstAsync(ut => ut.Name == "Teacher");

        var teachers = new[]
        {
            new
            {
                User = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Aysel",
                    LastName = "Əliyeva",
                    Email = "aysel.aliyeva@demomekteb.az",
                    UserName = "aysel.aliyeva",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher123!"),
                    PhoneNumber = "+994501111111",
                    UserTypeId = teacherUserType.Id,
                    IsActive = true
                },
                SubjectIndex = 0, // Riyaziyyat
                IsClassLeader = true,
                ClassIndex = 2 // 10-A
            },
            new
            {
                User = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Rəşad",
                    LastName = "Həsənov",
                    Email = "reshad.hasanov@demomekteb.az",
                    UserName = "reshad.hasanov",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher123!"),
                    PhoneNumber = "+994502222222",
                    UserTypeId = teacherUserType.Id,
                    IsActive = true
                },
                SubjectIndex = 1, // Fizika
                IsClassLeader = false,
                ClassIndex = 0
            },
            new
            {
                User = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Günel",
                    LastName = "Məmmədova",
                    Email = "gunel.mammadova@demomekteb.az",
                    UserName = "gunel.mammadova",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher123!"),
                    PhoneNumber = "+994503333333",
                    UserTypeId = teacherUserType.Id,
                    IsActive = true
                },
                SubjectIndex = 6, // İngilis dili
                IsClassLeader = true,
                ClassIndex = 4 // 11-A
            }
        };

        var teacherIds = new List<Guid>();

        foreach (var teacherData in teachers)
        {
            await context.Users.AddAsync(teacherData.User);

            var teacherId = Guid.NewGuid();
            var teacher = new Teacher
            {
                Id = teacherId,
                UserId = teacherData.User.Id,
                CompanyId = companyId,
                FirstName = teacherData.User.FirstName,
                LastName = teacherData.User.LastName,
                Email = teacherData.User.Email,
                PhoneNumber = teacherData.User.PhoneNumber,
                Specialization = "Tam ixtisas",
                IsActive = true
            };

            await context.Teachers.AddAsync(teacher);

            // UserRole
            await context.UserRoles.AddAsync(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = teacherData.User.Id,
                RoleId = teacherRole.Id,
                AssignedDate = DateTime.Now,
                IsActive = true
            });

            // TeacherSubject
            await context.TeacherSubjects.AddAsync(new TeacherSubject
            {
                Id = Guid.NewGuid(),
                TeacherId = teacherId,
                SubjectId = subjectIds[teacherData.SubjectIndex],
                IsActive = true
            });

            // TeacherClass
            if (teacherData.ClassIndex < classIds.Count)
            {
                await context.TeacherClasses.AddAsync(new TeacherClass
                {
                    Id = Guid.NewGuid(),
                    TeacherId = teacherId,
                    ClassId = classIds[teacherData.ClassIndex],
                    SubjectId = subjectIds[teacherData.SubjectIndex],
                    IsClassLeader = teacherData.IsClassLeader,
                    IsActive = true
                });
            }

            teacherIds.Add(teacherId);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {teachers.Length} müəllim əlavə edildi (şifrə: Teacher123!)");

        return teacherIds;
    }

    #endregion

    #region 10. Sample Parents

    private static async Task<List<Guid>> SeedParentsAsync(AppDbContext context, Guid companyId)
    {
        if (await context.Parents.AnyAsync(p => p.CompanyId == companyId))
        {
            Console.WriteLine("⏭️ Parents artıq mövcuddur");
            return await context.Parents.Where(p => p.CompanyId == companyId).Select(p => p.Id).ToListAsync();
        }

        var parentRole = await context.Roles.FirstAsync(r => r.Code == "PARENT");
        var parentUserType = await context.UserTypes.FirstAsync(ut => ut.Name == "Parent");

        var parents = new[]
        {
            new
            {
                User = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Kamran",
                    LastName = "Əhmədov",
                    Email = "kamran.ahmadov@example.com",
                    UserName = "kamran.ahmadov",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Parent123!"),
                    PhoneNumber = "+994504444444",
                    UserTypeId = parentUserType.Id,
                    IsActive = true
                },
                FinCode = "2AB3CD4"
            },
            new
            {
                User = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Leyla",
                    LastName = "Qasımova",
                    Email = "leyla.qasimova@example.com",
                    UserName = "leyla.qasimova",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Parent123!"),
                    PhoneNumber = "+994505555555",
                    UserTypeId = parentUserType.Id,
                    IsActive = true
                },
                FinCode = "3EF4GH5"
            },
            new
            {
                User = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Orxan",
                    LastName = "Rzayev",
                    Email = "orxan.rzayev@example.com",
                    UserName = "orxan.rzayev",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Parent123!"),
                    PhoneNumber = "+994506666666",
                    UserTypeId = parentUserType.Id,
                    IsActive = true
                },
                FinCode = "4IJ5KL6"
            }
        };

        var parentIds = new List<Guid>();

        foreach (var parentData in parents)
        {
            await context.Users.AddAsync(parentData.User);

            var parentId = Guid.NewGuid();
            var parent = new Parent
            {
                Id = parentId,
                UserId = parentData.User.Id,
                CompanyId = companyId,
                FirstName = parentData.User.FirstName,
                LastName = parentData.User.LastName,
                Email = parentData.User.Email,
                PhoneNumber = parentData.User.PhoneNumber,
                FinCode = parentData.FinCode,
                IsActive = true
            };

            await context.Parents.AddAsync(parent);

            // UserRole
            await context.UserRoles.AddAsync(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = parentData.User.Id,
                RoleId = parentRole.Id,
                AssignedDate = DateTime.Now,
                IsActive = true
            });

            parentIds.Add(parentId);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {parents.Length} valideyn əlavə edildi (FIN ilə giriş)");

        return parentIds;
    }

    #endregion

    #region 11. Sample Students

    private static async Task SeedStudentsAsync(
        AppDbContext context,
        Guid companyId,
        List<Guid> classIds,
        List<Guid> parentIds)
    {
        if (await context.Students.AnyAsync(s => s.CompanyId == companyId))
        {
            Console.WriteLine("⏭️ Students artıq mövcuddur");
            return;
        }

        var parentTypeId = await context.ParentTypes.Select(pt => pt.Id).FirstOrDefaultAsync();

        var students = new[]
        {
            new { FirstName = "Nigar", LastName = "Əhmədova", FinCode = "5MN6OP7", ClassIndex = 2, ParentIndex = 0 },
            new { FirstName = "Emil", LastName = "Qasımov", FinCode = "6QR7ST8", ClassIndex = 2, ParentIndex = 1 },
            new { FirstName = "Ayla", LastName = "Rzayeva", FinCode = "7UV8WX9", ClassIndex = 4, ParentIndex = 2 },
            new { FirstName = "Rəşid", LastName = "Əhmədov", FinCode = "8YZ9AB1", ClassIndex = 0, ParentIndex = 0 }
        };

        foreach (var studentData in students)
        {
            var studentId = Guid.NewGuid();
            var student = new Student
            {
                Id = studentId,
                CompanyId = companyId,
                ClassId = classIds[studentData.ClassIndex],
                FirstName = studentData.FirstName,
                LastName = studentData.LastName,
                FinCode = studentData.FinCode,
                DateOfBirth = DateTime.Now.AddYears(-16),
                IsActive = true
            };

            await context.Students.AddAsync(student);

            // StudentParent
            if (parentTypeId != Guid.Empty && studentData.ParentIndex < parentIds.Count)
            {
                await context.StudentParents.AddAsync(new StudentParent
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    ParentId = parentIds[studentData.ParentIndex],
                    ParentTypeId = parentTypeId,
                    IsActive = true
                });
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {students.Length} şagird əlavə edildi");
    }

    #endregion

    #region 12. Menus

    private static async Task SeedMenusAsync(AppDbContext context)
    {
        if (await context.Menus.AnyAsync())
        {
            Console.WriteLine("⏭️ Menus artıq mövcuddur");
            return;
        }

        var menus = new List<Menu>();

        // ===== ADMIN AREA =====
        var adminDashboard = new Menu
        {
            Id = Guid.NewGuid(),
            Name = "Dashboard",
            Code = "ADMIN_DASHBOARD",
            Url = "/Admin/Dashboard/Index",
            IconSVG = "bi bi-speedometer2",
            Type = MenuType.Link,
            OrderIndex = 1,
            IsVisible = true,
            IsActive = true
        };
        menus.Add(adminDashboard);

        // Şirkətlər
        var companies = new Menu
        {
            Id = Guid.NewGuid(),
            Name = "Companies",
            Code = "ADMIN_COMPANIES",
            IconSVG = "bi bi-building",
            Type = MenuType.Group,
            OrderIndex = 2,
            IsVisible = true,
            IsActive = true
        };
        menus.Add(companies);

        menus.Add(new Menu
        {
            Id = Guid.NewGuid(),
            ParentId = companies.Id,
            Name = "CompanyList",
            Code = "ADMIN_COMPANY_LIST",
            Url = "/Admin/Company/Index",
            OrderIndex = 1,
            Type = MenuType.Link,
            IsVisible = true,
            IsActive = true
        });
        menus.Add(new Menu
        {
            Id = Guid.NewGuid(),
            ParentId = companies.Id,
            Name = "CompanyCreate",
            Code = "ADMIN_COMPANY_CREATE",
            Url = "/Admin/Company/Create",
            OrderIndex = 2,
            Type = MenuType.Link,
            IsVisible = true,
            IsActive = true
        });

        // Müəllimlər
        var teachers = new Menu
        {
            Id = Guid.NewGuid(),
            Name = "Teachers",
            Code = "ADMIN_TEACHERS",
            IconSVG = "bi bi-person-video3",
            Type = MenuType.Group,
            OrderIndex = 3,
            IsVisible = true,
            IsActive = true
        };
        menus.Add(teachers);

        menus.Add(new Menu { Id = Guid.NewGuid(), ParentId = teachers.Id, Name = "TeacherList", Code = "ADMIN_TEACHER_LIST", Url = "/Admin/Teacher/Index", OrderIndex = 1, Type = MenuType.Link, IsVisible = true, IsActive = true });
        menus.Add(new Menu { Id = Guid.NewGuid(), ParentId = teachers.Id, Name = "TeacherCreate", Code = "ADMIN_TEACHER_CREATE", Url = "/Admin/Teacher/Create", OrderIndex = 2, Type = MenuType.Link, IsVisible = true, IsActive = true });

        // Valideynlər
        var parents = new Menu { Id = Guid.NewGuid(), Name = "Parents", Code = "ADMIN_PARENTS", IconSVG = "bi bi-people", Type = MenuType.Group, OrderIndex = 4, IsVisible = true, IsActive = true };
        menus.Add(parents);
        menus.Add(new Menu { Id = Guid.NewGuid(), ParentId = parents.Id, Name = "ParentList", Code = "ADMIN_PARENT_LIST", Url = "/Admin/Parent/Index", OrderIndex = 1, Type = MenuType.Link, IsVisible = true, IsActive = true });
        menus.Add(new Menu { Id = Guid.NewGuid(), ParentId = parents.Id, Name = "ParentCreate", Code = "ADMIN_PARENT_CREATE", Url = "/Admin/Parent/Create", OrderIndex = 2, Type = MenuType.Link, IsVisible = true, IsActive = true });

        // Şagirdlər
        var students = new Menu { Id = Guid.NewGuid(), Name = "Students", Code = "ADMIN_STUDENTS", IconSVG = "bi bi-mortarboard", Type = MenuType.Group, OrderIndex = 5, IsVisible = true, IsActive = true };
        menus.Add(students);
        menus.Add(new Menu { Id = Guid.NewGuid(), ParentId = students.Id, Name = "StudentList", Code = "ADMIN_STUDENT_LIST", Url = "/Admin/Student/Index", OrderIndex = 1, Type = MenuType.Link, IsVisible = true, IsActive = true });
        menus.Add(new Menu { Id = Guid.NewGuid(), ParentId = students.Id, Name = "StudentCreate", Code = "ADMIN_STUDENT_CREATE", Url = "/Admin/Student/Create", OrderIndex = 2, Type = MenuType.Link, IsVisible = true, IsActive = true });

        // Görüşlər
        menus.Add(new Menu { Id = Guid.NewGuid(), Name = "Meetings", Code = "ADMIN_MEETINGS", Url = "/Admin/Meeting/Index", IconSVG = "bi bi-calendar-event", Type = MenuType.Link, OrderIndex = 6, IsVisible = true, IsActive = true });

        // Tənzimləmələr
        var settings = new Menu { Id = Guid.NewGuid(), Name = "Settings", Code = "ADMIN_SETTINGS", IconSVG = "bi bi-gear", Type = MenuType.Group, OrderIndex = 10, IsVisible = true, IsActive = true };
        menus.Add(settings);
        menus.Add(new Menu { Id = Guid.NewGuid(), ParentId = settings.Id, Name = "Roles", Code = "ADMIN_ROLES", Url = "/Admin/Role/Index", OrderIndex = 1, Type = MenuType.Link, IsVisible = true, IsActive = true });
        menus.Add(new Menu { Id = Guid.NewGuid(), ParentId = settings.Id, Name = "Permissions", Code = "ADMIN_PERMISSIONS", Url = "/Admin/Permission/Index", OrderIndex = 2, Type = MenuType.Link, IsVisible = true, IsActive = true });
        menus.Add(new Menu { Id = Guid.NewGuid(), ParentId = settings.Id, Name = "Menus", Code = "ADMIN_MENUS", Url = "/Admin/Menu/Index", OrderIndex = 3, Type = MenuType.Link, IsVisible = true, IsActive = true });

        await context.Menus.AddRangeAsync(menus);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {menus.Count} menu əlavə edildi");
    }

    #endregion
}