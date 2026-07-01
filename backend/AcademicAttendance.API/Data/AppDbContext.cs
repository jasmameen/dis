using AcademicAttendance.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicAttendance.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Stage> Stages => Set<Stage>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
    public DbSet<TeacherStageSection> TeacherStageSections => Set<TeacherStageSection>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<AttendanceDetail> AttendanceDetails => Set<AttendanceDetail>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Student>(e =>
        {
            e.HasIndex(s => s.UniversityNumber).IsUnique();
        });

        modelBuilder.Entity<Subject>(e =>
        {
            e.HasIndex(s => s.Code).IsUnique();
        });

        modelBuilder.Entity<Teacher>(e =>
        {
            e.HasOne(t => t.User).WithOne(u => u.Teacher).HasForeignKey<Teacher>(t => t.UserId);
        });

        modelBuilder.Entity<TeacherSubject>(e =>
        {
            e.HasKey(ts => new { ts.TeacherId, ts.SubjectId });
        });

        modelBuilder.Entity<Attendance>(e =>
        {
            e.HasIndex(a => new { a.SubjectId, a.StageId, a.SectionId, a.Date }).IsUnique();
        });

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "علوم الحاسوب", NameEn = "Computer Science", Code = "CS" },
            new Department { Id = 2, Name = "هندسة", NameEn = "Engineering", Code = "ENG" }
        );

        modelBuilder.Entity<Stage>().HasData(
            new Stage { Id = 1, Name = "المرحلة الأولى", NameEn = "First Stage", Order = 1 },
            new Stage { Id = 2, Name = "المرحلة الثانية", NameEn = "Second Stage", Order = 2 },
            new Stage { Id = 3, Name = "المرحلة الثالثة", NameEn = "Third Stage", Order = 3 },
            new Stage { Id = 4, Name = "المرحلة الرابعة", NameEn = "Fourth Stage", Order = 4 }
        );

        modelBuilder.Entity<Section>().HasData(
            new Section { Id = 1, Name = "A" },
            new Section { Id = 2, Name = "B" },
            new Section { Id = 3, Name = "C" },
            new Section { Id = 4, Name = "D" }
        );
    }
}
