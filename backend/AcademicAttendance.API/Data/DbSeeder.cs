using AcademicAttendance.API.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace AcademicAttendance.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Users.AnyAsync()) return;

        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@university.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            IsActive = true
        };

        var teacherUser = new User
        {
            Username = "teacher1",
            Email = "teacher@university.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
            Role = UserRole.Teacher,
            IsActive = true
        };

        context.Users.AddRange(adminUser, teacherUser);
        await context.SaveChangesAsync();

        var teacher = new Teacher
        {
            UserId = teacherUser.Id,
            FullName = "د. أحمد محمد",
            Phone = "07701234567",
            DepartmentId = 1
        };
        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();

        var subjects = new[]
        {
            new Subject { Name = "برمجة 1", Code = "CS101", StageId = 1, DepartmentId = 1, CreditHours = 3 },
            new Subject { Name = "هياكل البيانات", Code = "CS201", StageId = 2, DepartmentId = 1, CreditHours = 3 },
            new Subject { Name = "قواعد البيانات", Code = "CS301", StageId = 3, DepartmentId = 1, CreditHours = 3 }
        };
        context.Subjects.AddRange(subjects);
        await context.SaveChangesAsync();

        context.TeacherSubjects.AddRange(
            new TeacherSubject { TeacherId = teacher.Id, SubjectId = subjects[0].Id },
            new TeacherSubject { TeacherId = teacher.Id, SubjectId = subjects[1].Id }
        );

        context.TeacherStageSections.AddRange(
            new TeacherStageSection { TeacherId = teacher.Id, StageId = 1, SectionId = 1 },
            new TeacherStageSection { TeacherId = teacher.Id, StageId = 2, SectionId = 1 }
        );

        var students = new[]
        {
            new Student { FullName = "أحمد علي", UniversityNumber = "2024001", StageId = 1, SectionId = 1, DepartmentId = 1 },
            new Student { FullName = "علي حسن", UniversityNumber = "2024002", StageId = 1, SectionId = 1, DepartmentId = 1 },
            new Student { FullName = "محمد سعد", UniversityNumber = "2024003", StageId = 1, SectionId = 1, DepartmentId = 1 },
            new Student { FullName = "سارة أحمد", UniversityNumber = "2024004", StageId = 1, SectionId = 2, DepartmentId = 1 },
            new Student { FullName = "فاطمة علي", UniversityNumber = "2023001", StageId = 2, SectionId = 1, DepartmentId = 1 },
            new Student { FullName = "حسين كريم", UniversityNumber = "2023002", StageId = 2, SectionId = 1, DepartmentId = 1 }
        };
        context.Students.AddRange(students);
        await context.SaveChangesAsync();
    }
}
