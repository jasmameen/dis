using AcademicAttendance.API.Data;
using AcademicAttendance.API.DTOs;
using AcademicAttendance.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicAttendance.API.Services;

public interface IAttendanceService
{
    Task<List<AttendanceStudentRowDto>> GetStudentsForAttendanceAsync(AttendanceFilterRequest filter, int? teacherId);
    Task<(bool Success, string? Error)> SaveAsync(SaveAttendanceRequest request, int? teacherId, UserRole role, int? userId);
    Task<bool> UpdateAsync(int attendanceId, SaveAttendanceRequest request, int? userId);
}

public class AttendanceService(AppDbContext context, IAuditService audit, INotificationService notifications) : IAttendanceService
{
    public async Task<List<AttendanceStudentRowDto>> GetStudentsForAttendanceAsync(AttendanceFilterRequest filter, int? teacherId)
    {
        var students = await context.Students
            .Where(s => s.IsActive && s.StageId == filter.StageId && s.SectionId == filter.SectionId)
            .OrderBy(s => s.FullName)
            .ToListAsync();

        var attendance = await context.Attendances
            .Include(a => a.Details)
            .FirstOrDefaultAsync(a =>
                a.SubjectId == filter.SubjectId &&
                a.StageId == filter.StageId &&
                a.SectionId == filter.SectionId &&
                a.Date == filter.Date);

        return students.Select(s =>
        {
            var detail = attendance?.Details.FirstOrDefault(d => d.StudentId == s.Id);
            return new AttendanceStudentRowDto(
                s.Id, s.FullName, s.UniversityNumber,
                detail?.Status ?? AttendanceStatus.Present,
                detail?.Id
            );
        }).ToList();
    }

    public async Task<(bool Success, string? Error)> SaveAsync(SaveAttendanceRequest request, int? teacherId, UserRole role, int? userId)
    {
        var resolvedTeacherId = teacherId;
        if (!resolvedTeacherId.HasValue && role == UserRole.Admin)
        {
            resolvedTeacherId = await context.TeacherSubjects
                .Where(ts => ts.SubjectId == request.SubjectId)
                .Select(ts => (int?)ts.TeacherId)
                .FirstOrDefaultAsync();
        }

        if (!resolvedTeacherId.HasValue)
            return (false, "يجب أن تكون أستاذاً لتسجيل الحضور أو ربط مادة بأستاذ");

        if (role == UserRole.Teacher)
        {
            var assigned = await context.TeacherSubjects
                .AnyAsync(ts => ts.TeacherId == resolvedTeacherId && ts.SubjectId == request.SubjectId);
            if (!assigned)
                return (false, "المادة غير مسندة لك");
        }

        if (request.Entries == null || request.Entries.Count == 0)
            return (false, "لا توجد سجلات حضور للحفظ");

        var existing = await context.Attendances
            .Include(a => a.Details)
            .FirstOrDefaultAsync(a =>
                a.SubjectId == request.SubjectId &&
                a.StageId == request.StageId &&
                a.SectionId == request.SectionId &&
                a.Date == request.Date);

        if (existing != null)
        {
            context.AttendanceDetails.RemoveRange(existing.Details);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.IsLocked = true;
            foreach (var entry in request.Entries)
            {
                existing.Details.Add(new AttendanceDetail
                {
                    StudentId = entry.StudentId,
                    Status = entry.Status,
                    Notes = entry.Notes
                });
            }
        }
        else
        {
            var attendance = new Attendance
            {
                SubjectId = request.SubjectId,
                TeacherId = resolvedTeacherId.Value,
                StageId = request.StageId,
                SectionId = request.SectionId,
                Date = request.Date,
                IsLocked = true,
                Details = request.Entries.Select(e => new AttendanceDetail
                {
                    StudentId = e.StudentId,
                    Status = e.Status,
                    Notes = e.Notes
                }).ToList()
            };
            context.Attendances.Add(attendance);
        }

        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Save", "Attendance", request.Date.ToString());
        await notifications.CheckHighAbsenceAsync(request.Entries.Where(e => e.Status == AttendanceStatus.Absent).Select(e => e.StudentId));
        return (true, null);
    }

    public async Task<bool> UpdateAsync(int attendanceId, SaveAttendanceRequest request, int? userId)
    {
        var attendance = await context.Attendances.Include(a => a.Details).FirstOrDefaultAsync(a => a.Id == attendanceId);
        if (attendance == null) return false;

        context.AttendanceDetails.RemoveRange(attendance.Details);
        attendance.UpdatedAt = DateTime.UtcNow;
        foreach (var entry in request.Entries)
        {
            attendance.Details.Add(new AttendanceDetail
            {
                StudentId = entry.StudentId,
                Status = entry.Status,
                Notes = entry.Notes
            });
        }

        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Update", "Attendance", attendanceId.ToString());
        return true;
    }
}
