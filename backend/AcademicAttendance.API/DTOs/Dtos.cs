using System.ComponentModel.DataAnnotations;
using AcademicAttendance.API.Models;

namespace AcademicAttendance.API.DTOs;

public record LoginRequest(
    [Required] string Login,
    [Required] string Password
);

public record LoginResponse(
    string Token,
    string Username,
    string Email,
    UserRole Role,
    int? TeacherId,
    string? FullName
);

public record RegisterTeacherRequest(
    [Required] string FullName,
    [Required][EmailAddress] string Email,
    [Required] string Username,
    [Required][MinLength(6)] string Password,
    string? Phone,
    int? DepartmentId
);

public record ForgotPasswordRequest([Required][EmailAddress] string Email);
public record ResetPasswordRequest([Required] string Token, [Required] string NewPassword);

public record DashboardStatsDto(
    int AssignedSubjects,
    int TotalStudents,
    int PresentToday,
    int AbsentToday,
    double AttendanceRateToday,
    List<ChartPointDto> WeeklyStats
);

public record ChartPointDto(string Label, int Present, int Absent);

public record StudentDto(
    int Id,
    string FullName,
    string UniversityNumber,
    int StageId,
    string StageName,
    int SectionId,
    string SectionName,
    int DepartmentId,
    string DepartmentName,
    bool IsActive
);

public record CreateStudentRequest(
    string FullName,
    string UniversityNumber,
    int StageId,
    int SectionId,
    int DepartmentId
);

public record SubjectDto(
    int Id,
    string Name,
    string Code,
    int StageId,
    string StageName,
    int DepartmentId,
    string DepartmentName,
    int CreditHours
);

public record CreateSubjectRequest(
    string Name,
    string Code,
    int StageId,
    int DepartmentId,
    int CreditHours
);

public record TeacherDto(
    int Id,
    int UserId,
    string FullName,
    string Email,
    string Username,
    string? Phone,
    int? DepartmentId,
    string? DepartmentName,
    List<int> SubjectIds,
    List<TeacherStageSectionDto> StageSections
);

public record TeacherStageSectionDto(int StageId, int SectionId, string StageName, string SectionName);

public record CreateTeacherRequest(
    string FullName,
    string Email,
    string Username,
    string Password,
    string? Phone,
    int? DepartmentId,
    List<int> SubjectIds,
    List<TeacherStageSectionDto> StageSections
);

public record AttendanceFilterRequest(
    int StageId,
    int SectionId,
    int SubjectId,
    DateOnly Date
);

public record AttendanceStudentRowDto(
    int StudentId,
    string FullName,
    string UniversityNumber,
    AttendanceStatus Status,
    int? DetailId
);

public record SaveAttendanceRequest(
    int StageId,
    int SectionId,
    int SubjectId,
    DateOnly Date,
    List<AttendanceEntryDto> Entries
);

public record AttendanceEntryDto(int StudentId, AttendanceStatus Status, string? Notes);

public record ReportFilterRequest(
    DateOnly? FromDate,
    DateOnly? ToDate,
    int? StageId,
    int? SectionId,
    int? SubjectId,
    int? StudentId
);

public record DailyReportDto(
    DateOnly Date,
    int TotalStudents,
    int Present,
    int Absent,
    int Late,
    int Leave,
    double AttendanceRate
);

public record MonthlyReportDto(
    int Year,
    int Month,
    int TotalSessions,
    int TotalPresent,
    int TotalAbsent,
    double AttendanceRate,
    List<StudentAbsenceDto> StudentAbsences
);

public record StudentAbsenceDto(
    int StudentId,
    string FullName,
    string UniversityNumber,
    int AbsenceCount,
    double AttendanceRate
);

public record YearlyReportDto(
    int Year,
    int StudentId,
    string FullName,
    string UniversityNumber,
    double AttendanceRate,
    int TotalSessions,
    int Present,
    int Absent,
    int Late,
    int Leave,
    List<MonthlyBreakdownDto> MonthlyBreakdown
);

public record MonthlyBreakdownDto(int Month, string MonthName, double Rate, int Absent);

public record SearchRequest(
    string? Query,
    int? StageId,
    int? SectionId,
    int? SubjectId
);

public record NotificationDto(
    int Id,
    NotificationType Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt
);

public record LookupDto(int Id, string Name);

public record ApiResponse<T>(bool Success, T? Data, string? Message = null);
