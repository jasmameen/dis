using AcademicAttendance.API.Data;
using AcademicAttendance.API.DTOs;
using AcademicAttendance.API.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace AcademicAttendance.API.Services;

public interface IReportService
{
    Task<DailyReportDto> GetDailyReportAsync(ReportFilterRequest filter);
    Task<MonthlyReportDto> GetMonthlyReportAsync(int year, int month, ReportFilterRequest filter);
    Task<YearlyReportDto?> GetYearlyReportAsync(int year, int studentId);
    Task<byte[]> ExportDailyExcelAsync(ReportFilterRequest filter);
}

public class ReportService(AppDbContext context) : IReportService
{
    public async Task<DailyReportDto> GetDailyReportAsync(ReportFilterRequest filter)
    {
        var date = filter.FromDate ?? DateOnly.FromDateTime(DateTime.Today);
        var details = await GetFilteredDetails(filter with { FromDate = date, ToDate = date });
        var total = details.Select(d => d.StudentId).Distinct().Count();
        var present = details.Count(d => d.Status == AttendanceStatus.Present);
        var absent = details.Count(d => d.Status == AttendanceStatus.Absent);
        var late = details.Count(d => d.Status == AttendanceStatus.Late);
        var leave = details.Count(d => d.Status == AttendanceStatus.Leave);
        var rate = total > 0 ? Math.Round((present + late) * 100.0 / details.Count, 1) : 0;

        return new DailyReportDto(date, total, present, absent, late, leave, rate);
    }

    public async Task<MonthlyReportDto> GetMonthlyReportAsync(int year, int month, ReportFilterRequest filter)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        var details = await GetFilteredDetails(filter with { FromDate = from, ToDate = to });

        var sessions = details.Select(d => d.AttendanceId).Distinct().Count();
        var present = details.Count(d => d.Status == AttendanceStatus.Present || d.Status == AttendanceStatus.Late);
        var absent = details.Count(d => d.Status == AttendanceStatus.Absent);
        var rate = details.Count > 0 ? Math.Round(present * 100.0 / details.Count, 1) : 0;

        var studentAbsences = details
            .GroupBy(d => d.StudentId)
            .Select(g =>
            {
                var student = g.First().Student;
                var total = g.Count();
                var absences = g.Count(x => x.Status == AttendanceStatus.Absent);
                return new StudentAbsenceDto(
                    g.Key, student.FullName, student.UniversityNumber,
                    absences, total > 0 ? Math.Round((total - absences) * 100.0 / total, 1) : 0
                );
            })
            .OrderByDescending(s => s.AbsenceCount)
            .ToList();

        return new MonthlyReportDto(year, month, sessions, present, absent, rate, studentAbsences);
    }

    public async Task<YearlyReportDto?> GetYearlyReportAsync(int year, int studentId)
    {
        var student = await context.Students.FindAsync(studentId);
        if (student == null) return null;

        var from = new DateOnly(year, 1, 1);
        var to = new DateOnly(year, 12, 31);
        var details = await context.AttendanceDetails
            .Include(d => d.Student)
            .Include(d => d.Attendance)
            .Where(d => d.StudentId == studentId && d.Attendance.Date >= from && d.Attendance.Date <= to)
            .ToListAsync();

        var total = details.Count;
        var present = details.Count(d => d.Status == AttendanceStatus.Present);
        var absent = details.Count(d => d.Status == AttendanceStatus.Absent);
        var late = details.Count(d => d.Status == AttendanceStatus.Late);
        var leave = details.Count(d => d.Status == AttendanceStatus.Leave);
        var rate = total > 0 ? Math.Round((present + late) * 100.0 / total, 1) : 0;

        var monthNames = new[] { "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };
        var monthly = Enumerable.Range(1, 12).Select(m =>
        {
            var monthDetails = details.Where(d => d.Attendance.Date.Month == m).ToList();
            var monthTotal = monthDetails.Count;
            var monthAbsent = monthDetails.Count(d => d.Status == AttendanceStatus.Absent);
            var monthRate = monthTotal > 0 ? Math.Round((monthTotal - monthAbsent) * 100.0 / monthTotal, 1) : 0;
            return new MonthlyBreakdownDto(m, monthNames[m], monthRate, monthAbsent);
        }).Where(m => m.Rate > 0 || m.Absent > 0).ToList();

        return new YearlyReportDto(year, studentId, student.FullName, student.UniversityNumber,
            rate, details.Select(d => d.AttendanceId).Distinct().Count(),
            present, absent, late, leave, monthly);
    }

    public async Task<byte[]> ExportDailyExcelAsync(ReportFilterRequest filter)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var report = await GetDailyReportAsync(filter);
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Daily Report");
        sheet.Cells[1, 1].Value = "التاريخ";
        sheet.Cells[1, 2].Value = report.Date.ToString();
        sheet.Cells[2, 1].Value = "الحاضرون";
        sheet.Cells[2, 2].Value = report.Present;
        sheet.Cells[3, 1].Value = "الغائبون";
        sheet.Cells[3, 2].Value = report.Absent;
        sheet.Cells[4, 1].Value = "نسبة الحضور";
        sheet.Cells[4, 2].Value = $"{report.AttendanceRate}%";
        return package.GetAsByteArray();
    }

    private async Task<List<AttendanceDetail>> GetFilteredDetails(ReportFilterRequest filter)
    {
        var query = context.AttendanceDetails
            .Include(d => d.Student)
            .Include(d => d.Attendance)
            .AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(d => d.Attendance.Date >= filter.FromDate);
        if (filter.ToDate.HasValue)
            query = query.Where(d => d.Attendance.Date <= filter.ToDate);
        if (filter.StageId.HasValue)
            query = query.Where(d => d.Attendance.StageId == filter.StageId);
        if (filter.SectionId.HasValue)
            query = query.Where(d => d.Attendance.SectionId == filter.SectionId);
        if (filter.SubjectId.HasValue)
            query = query.Where(d => d.Attendance.SubjectId == filter.SubjectId);
        if (filter.StudentId.HasValue)
            query = query.Where(d => d.StudentId == filter.StudentId);

        return await query.ToListAsync();
    }
}
