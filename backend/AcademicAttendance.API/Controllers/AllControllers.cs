using System.Security.Claims;
using AcademicAttendance.API.Data;
using AcademicAttendance.API.DTOs;
using AcademicAttendance.API.Models;
using AcademicAttendance.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(new ApiResponse<LoginResponse>(false, null, "بيانات الدخول غير صحيحة"));
        return Ok(new ApiResponse<LoginResponse>(true, result));
    }

    [HttpPost("register-teacher")]
    public async Task<ActionResult<ApiResponse<bool>>> RegisterTeacher([FromBody] RegisterTeacherRequest request)
    {
        var (success, error) = await authService.RegisterTeacherAsync(request);
        if (!success)
            return BadRequest(new ApiResponse<bool>(false, false, error));
        return Ok(new ApiResponse<bool>(true, true, "تم إنشاء حساب الأستاذ بنجاح، يمكنك تسجيل الدخول"));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await authService.ForgotPasswordAsync(request.Email);
        return Ok(new ApiResponse<bool>(true, true, "تم إرسال رابط استعادة كلمة المرور"));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var ok = await authService.ResetPasswordAsync(request.Token, request.NewPassword);
        if (!ok) return BadRequest(new ApiResponse<bool>(false, false, "رمز غير صالح"));
        return Ok(new ApiResponse<bool>(true, true, "تم تغيير كلمة المرور"));
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetStats()
    {
        var role = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
        int? teacherId = int.TryParse(User.FindFirstValue("TeacherId"), out var tid) ? tid : null;
        var stats = await dashboardService.GetStatsAsync(teacherId, role);
        return Ok(new ApiResponse<DashboardStatsDto>(true, stats));
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController(IStudentService studentService) : ControllerBase
{
    private int? UserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StudentDto>>>> GetAll(
        [FromQuery] int? stageId, [FromQuery] int? sectionId, [FromQuery] int? departmentId)
    {
        var students = await studentService.GetAllAsync(stageId, sectionId, departmentId);
        return Ok(new ApiResponse<List<StudentDto>>(true, students));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(int id)
    {
        var student = await studentService.GetByIdAsync(id);
        if (student == null) return NotFound(new ApiResponse<StudentDto>(false, null, "الطالب غير موجود"));
        return Ok(new ApiResponse<StudentDto>(true, student));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Create([FromBody] CreateStudentRequest request)
    {
        var student = await studentService.CreateAsync(request, UserId);
        return Ok(new ApiResponse<StudentDto>(true, student));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Update(int id, [FromBody] CreateStudentRequest request)
    {
        var student = await studentService.UpdateAsync(id, request, UserId);
        if (student == null) return NotFound(new ApiResponse<StudentDto>(false, null, "الطالب غير موجود"));
        return Ok(new ApiResponse<StudentDto>(true, student));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var ok = await studentService.DeleteAsync(id, UserId);
        if (!ok) return NotFound(new ApiResponse<bool>(false, false, "الطالب غير موجود"));
        return Ok(new ApiResponse<bool>(true, true));
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ApiResponse<int>(false, 0, "الملف فارغ"));
        await using var stream = file.OpenReadStream();
        var count = await studentService.ImportAsync(stream, UserId);
        return Ok(new ApiResponse<int>(true, count, $"تم استيراد {count} طالب"));
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] int? stageId, [FromQuery] int? sectionId)
    {
        var bytes = await studentService.ExportAsync(stageId, sectionId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "students.xlsx");
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController(IAttendanceService attendanceService) : ControllerBase
{
    private int? UserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private int? TeacherId => int.TryParse(User.FindFirstValue("TeacherId"), out var id) ? id : null;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AttendanceStudentRowDto>>>> Get(
        [FromQuery] int stageId, [FromQuery] int sectionId,
        [FromQuery] int subjectId, [FromQuery] DateOnly date)
    {
        var rows = await attendanceService.GetStudentsForAttendanceAsync(
            new AttendanceFilterRequest(stageId, sectionId, subjectId, date), TeacherId);
        return Ok(new ApiResponse<List<AttendanceStudentRowDto>>(true, rows));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> Save([FromBody] SaveAttendanceRequest request)
    {
        var role = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
        var (success, error) = await attendanceService.SaveAsync(request, TeacherId, role, UserId);
        if (!success)
            return BadRequest(new ApiResponse<bool>(false, false, error));
        return Ok(new ApiResponse<bool>(true, true, "تم حفظ الحضور"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] SaveAttendanceRequest request)
    {
        var ok = await attendanceService.UpdateAsync(id, request, UserId);
        if (!ok) return NotFound(new ApiResponse<bool>(false, false, "سجل الحضور غير موجود"));
        return Ok(new ApiResponse<bool>(true, true, "تم تعديل الحضور"));
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubjectsController(ISubjectService subjectService) : ControllerBase
{
    private int? UserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private int? TeacherId => int.TryParse(User.FindFirstValue("TeacherId"), out var id) ? id : null;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SubjectDto>>>> GetAll()
    {
        var role = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
        var subjects = await subjectService.GetAllAsync(TeacherId, role);
        return Ok(new ApiResponse<List<SubjectDto>>(true, subjects));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SubjectDto>>> Create([FromBody] CreateSubjectRequest request)
    {
        var subject = await subjectService.CreateAsync(request, UserId);
        return Ok(new ApiResponse<SubjectDto>(true, subject));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SubjectDto>>> Update(int id, [FromBody] CreateSubjectRequest request)
    {
        var subject = await subjectService.UpdateAsync(id, request, UserId);
        if (subject == null) return NotFound();
        return Ok(new ApiResponse<SubjectDto>(true, subject));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var ok = await subjectService.DeleteAsync(id, UserId);
        return Ok(new ApiResponse<bool>(true, ok));
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TeachersController(ITeacherService teacherService) : ControllerBase
{
    private int? UserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TeacherDto>>>> GetAll()
        => Ok(new ApiResponse<List<TeacherDto>>(true, await teacherService.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TeacherDto>>> GetById(int id)
    {
        var teacher = await teacherService.GetByIdAsync(id);
        if (teacher == null) return NotFound();
        return Ok(new ApiResponse<TeacherDto>(true, teacher));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TeacherDto>>> Create([FromBody] CreateTeacherRequest request)
        => Ok(new ApiResponse<TeacherDto>(true, await teacherService.CreateAsync(request, UserId)));

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TeacherDto>>> Update(int id, [FromBody] CreateTeacherRequest request)
    {
        var teacher = await teacherService.UpdateAsync(id, request, UserId);
        if (teacher == null) return NotFound();
        return Ok(new ApiResponse<TeacherDto>(true, teacher));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        => Ok(new ApiResponse<bool>(true, await teacherService.DeleteAsync(id, UserId)));
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("daily")]
    public async Task<ActionResult<ApiResponse<DailyReportDto>>> Daily([FromQuery] ReportFilterRequest filter)
        => Ok(new ApiResponse<DailyReportDto>(true, await reportService.GetDailyReportAsync(filter)));

    [HttpGet("monthly")]
    public async Task<ActionResult<ApiResponse<MonthlyReportDto>>> Monthly(
        [FromQuery] int year, [FromQuery] int month, [FromQuery] ReportFilterRequest filter)
        => Ok(new ApiResponse<MonthlyReportDto>(true, await reportService.GetMonthlyReportAsync(year, month, filter)));

    [HttpGet("yearly")]
    public async Task<ActionResult<ApiResponse<YearlyReportDto>>> Yearly([FromQuery] int year, [FromQuery] int studentId)
    {
        var report = await reportService.GetYearlyReportAsync(year, studentId);
        if (report == null) return NotFound();
        return Ok(new ApiResponse<YearlyReportDto>(true, report));
    }

    [HttpGet("daily/export")]
    public async Task<IActionResult> ExportDaily([FromQuery] ReportFilterRequest filter)
    {
        var bytes = await reportService.ExportDailyExcelAsync(filter);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "daily-report.xlsx");
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetAll()
    {
        var notifications = await notificationService.GetForUserAsync(UserId);
        return Ok(new ApiResponse<List<NotificationDto>>(true, notifications.Select(n => new NotificationDto(
            n.Id, n.Type, n.Title, n.Message, n.IsRead, n.CreatedAt)).ToList()));
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkRead(int id)
    {
        await notificationService.MarkAsReadAsync(id);
        return Ok(new ApiResponse<bool>(true, true));
    }
}

[ApiController]
[Route("api/[controller]")]
public class LookupController(ILookupService lookupService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> GetLookups()
        => Ok(new ApiResponse<object>(true, await lookupService.GetAllLookupsAsync()));

    [HttpGet("search")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<StudentDto>>>> Search([FromQuery] SearchRequest request)
        => Ok(new ApiResponse<List<StudentDto>>(true, await lookupService.SearchAsync(request)));
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext context) : ControllerBase
{
    [HttpGet("departments")]
    public async Task<ActionResult<ApiResponse<List<LookupDto>>>> GetDepartments()
    {
        var deps = await context.Departments.Select(d => new LookupDto(d.Id, d.Name)).ToListAsync();
        return Ok(new ApiResponse<List<LookupDto>>(true, deps));
    }

    [HttpPost("departments")]
    public async Task<ActionResult<ApiResponse<LookupDto>>> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        var dept = new Department { Name = request.Name, NameEn = request.NameEn, Code = request.Code };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();
        return Ok(new ApiResponse<LookupDto>(true, new LookupDto(dept.Id, dept.Name)));
    }
}

public record CreateDepartmentRequest(string Name, string? NameEn, string Code);
