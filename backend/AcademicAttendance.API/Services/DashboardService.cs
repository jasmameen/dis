using AcademicAttendance.API.Data;
using AcademicAttendance.API.DTOs;
using AcademicAttendance.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicAttendance.API.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(int? teacherId, UserRole role);
}

public class DashboardService(AppDbContext context) : IDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync(int? teacherId, UserRole role)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        IQueryable<Student> studentsQuery = context.Students.Where(s => s.IsActive);
        IQueryable<Subject> subjectsQuery = context.Subjects;
        IQueryable<Attendance> attendanceQuery = context.Attendances.Where(a => a.Date == today);

        if (role == UserRole.Teacher && teacherId.HasValue)
        {
            var subjectIds = await context.TeacherSubjects
                .Where(ts => ts.TeacherId == teacherId)
                .Select(ts => ts.SubjectId)
                .ToListAsync();

            var stageSections = await context.TeacherStageSections
                .Where(tss => tss.TeacherId == teacherId)
                .Select(tss => new { tss.StageId, tss.SectionId })
                .ToListAsync();

            subjectsQuery = subjectsQuery.Where(s => subjectIds.Contains(s.Id));
            studentsQuery = studentsQuery.Where(s =>
                stageSections.Any(ss => ss.StageId == s.StageId && ss.SectionId == s.SectionId));
            attendanceQuery = attendanceQuery.Where(a => a.TeacherId == teacherId);
        }

        var totalStudents = await studentsQuery.CountAsync();
        var assignedSubjects = await subjectsQuery.CountAsync();

        var todayDetails = await context.AttendanceDetails
            .Include(d => d.Attendance)
            .Where(d => d.Attendance.Date == today &&
                        (role == UserRole.Admin || (teacherId.HasValue && d.Attendance.TeacherId == teacherId)))
            .ToListAsync();

        var presentToday = todayDetails.Count(d => d.Status == AttendanceStatus.Present || d.Status == AttendanceStatus.Late);
        var absentToday = todayDetails.Count(d => d.Status == AttendanceStatus.Absent);
        var totalToday = todayDetails.Count;
        var rate = totalToday > 0 ? Math.Round(presentToday * 100.0 / totalToday, 1) : 0;

        var weeklyStats = new List<ChartPointDto>();
        for (var i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayDetails = await context.AttendanceDetails
                .Include(d => d.Attendance)
                .Where(d => d.Attendance.Date == date &&
                            (role == UserRole.Admin || (teacherId.HasValue && d.Attendance.TeacherId == teacherId)))
                .ToListAsync();

            weeklyStats.Add(new ChartPointDto(
                date.ToString("MM/dd"),
                dayDetails.Count(d => d.Status == AttendanceStatus.Present || d.Status == AttendanceStatus.Late),
                dayDetails.Count(d => d.Status == AttendanceStatus.Absent)
            ));
        }

        return new DashboardStatsDto(assignedSubjects, totalStudents, presentToday, absentToday, rate, weeklyStats);
    }
}
