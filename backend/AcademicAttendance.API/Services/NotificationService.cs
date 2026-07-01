using AcademicAttendance.API.Data;
using AcademicAttendance.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicAttendance.API.Services;

public interface INotificationService
{
    Task<List<Models.Notification>> GetForUserAsync(int userId);
    Task MarkAsReadAsync(int id);
    Task CheckHighAbsenceAsync(IEnumerable<int> absentStudentIds);
    Task CheckTeachersNotRecordedAsync();
}

public class NotificationService(AppDbContext context) : INotificationService
{
    private const int AbsenceThreshold = 5;

    public async Task<List<Models.Notification>> GetForUserAsync(int userId)
    {
        return await context.Notifications
            .Where(n => n.UserId == userId || n.UserId == null)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int id)
    {
        var n = await context.Notifications.FindAsync(id);
        if (n != null) { n.IsRead = true; await context.SaveChangesAsync(); }
    }

    public async Task CheckHighAbsenceAsync(IEnumerable<int> absentStudentIds)
    {
        foreach (var studentId in absentStudentIds)
        {
            var absenceCount = await context.AttendanceDetails
                .CountAsync(d => d.StudentId == studentId && d.Status == AttendanceStatus.Absent);

            if (absenceCount >= AbsenceThreshold)
            {
                var student = await context.Students.FindAsync(studentId);
                if (student == null) continue;

                var exists = await context.Notifications.AnyAsync(n =>
                    n.Type == NotificationType.HighAbsence &&
                    n.Message.Contains(student.UniversityNumber) &&
                    n.CreatedAt > DateTime.UtcNow.AddDays(-7));

                if (!exists)
                {
                    context.Notifications.Add(new Models.Notification
                    {
                        Type = NotificationType.HighAbsence,
                        Title = "تنبيه: كثرة غياب",
                        Message = $"الطالب {student.FullName} ({student.UniversityNumber}) تجاوز {AbsenceThreshold} غيابات"
                    });
                }
            }
        }
        await context.SaveChangesAsync();
    }

    public async Task CheckTeachersNotRecordedAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var teachers = await context.Teachers.Include(t => t.User).ToListAsync();

        foreach (var teacher in teachers)
        {
            var hasRecorded = await context.Attendances.AnyAsync(a =>
                a.TeacherId == teacher.Id && a.Date == today);

            if (!hasRecorded)
            {
                context.Notifications.Add(new Models.Notification
                {
                    UserId = teacher.UserId,
                    Type = NotificationType.AttendanceNotRecorded,
                    Title = "تذكير: تسجيل الحضور",
                    Message = $"لم يتم تسجيل حضور اليوم ({today})"
                });
            }
        }
        await context.SaveChangesAsync();
    }
}
