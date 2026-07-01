using AcademicAttendance.API.Data;
using AcademicAttendance.API.DTOs;
using AcademicAttendance.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicAttendance.API.Services;

public interface ISubjectService
{
    Task<List<SubjectDto>> GetAllAsync(int? teacherId, UserRole role);
    Task<SubjectDto?> GetByIdAsync(int id);
    Task<SubjectDto> CreateAsync(CreateSubjectRequest request, int? userId);
    Task<SubjectDto?> UpdateAsync(int id, CreateSubjectRequest request, int? userId);
    Task<bool> DeleteAsync(int id, int? userId);
}

public class SubjectService(AppDbContext context, IAuditService audit) : ISubjectService
{
    public async Task<List<SubjectDto>> GetAllAsync(int? teacherId, UserRole role)
    {
        var query = context.Subjects.Include(s => s.Stage).Include(s => s.Department).AsQueryable();
        if (role == UserRole.Teacher && teacherId.HasValue)
        {
            var ids = await context.TeacherSubjects.Where(ts => ts.TeacherId == teacherId).Select(ts => ts.SubjectId).ToListAsync();
            query = query.Where(s => ids.Contains(s.Id));
        }
        return await query.Select(s => MapToDto(s)).ToListAsync();
    }

    public async Task<SubjectDto?> GetByIdAsync(int id)
    {
        var s = await context.Subjects.Include(x => x.Stage).Include(x => x.Department).FirstOrDefaultAsync(x => x.Id == id);
        return s == null ? null : MapToDto(s);
    }

    public async Task<SubjectDto> CreateAsync(CreateSubjectRequest request, int? userId)
    {
        var subject = new Subject
        {
            Name = request.Name, Code = request.Code,
            StageId = request.StageId, DepartmentId = request.DepartmentId,
            CreditHours = request.CreditHours
        };
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Create", "Subject", subject.Id.ToString());
        return (await GetByIdAsync(subject.Id))!;
    }

    public async Task<SubjectDto?> UpdateAsync(int id, CreateSubjectRequest request, int? userId)
    {
        var subject = await context.Subjects.FindAsync(id);
        if (subject == null) return null;
        subject.Name = request.Name; subject.Code = request.Code;
        subject.StageId = request.StageId; subject.DepartmentId = request.DepartmentId;
        subject.CreditHours = request.CreditHours;
        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Update", "Subject", id.ToString());
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id, int? userId)
    {
        var subject = await context.Subjects.FindAsync(id);
        if (subject == null) return false;
        context.Subjects.Remove(subject);
        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Delete", "Subject", id.ToString());
        return true;
    }

    private static SubjectDto MapToDto(Subject s) => new(
        s.Id, s.Name, s.Code, s.StageId, s.Stage.Name, s.DepartmentId, s.Department.Name, s.CreditHours
    );
}

public interface ITeacherService
{
    Task<List<TeacherDto>> GetAllAsync();
    Task<TeacherDto?> GetByIdAsync(int id);
    Task<TeacherDto> CreateAsync(CreateTeacherRequest request, int? userId);
    Task<TeacherDto?> UpdateAsync(int id, CreateTeacherRequest request, int? userId);
    Task<bool> DeleteAsync(int id, int? userId);
}

public class TeacherService(AppDbContext context, IAuditService audit) : ITeacherService
{
    public async Task<List<TeacherDto>> GetAllAsync()
    {
        var teachers = await context.Teachers
            .Include(t => t.User).Include(t => t.Department)
            .Include(t => t.TeacherSubjects).Include(t => t.TeacherStageSections)
            .ThenInclude(tss => tss.Stage)
            .Include(t => t.TeacherStageSections).ThenInclude(tss => tss.Section)
            .ToListAsync();
        return teachers.Select(MapToDto).ToList();
    }

    public async Task<TeacherDto?> GetByIdAsync(int id)
    {
        var t = await context.Teachers
            .Include(x => x.User).Include(x => x.Department)
            .Include(x => x.TeacherSubjects).Include(x => x.TeacherStageSections)
            .ThenInclude(tss => tss.Stage)
            .Include(x => x.TeacherStageSections).ThenInclude(tss => tss.Section)
            .FirstOrDefaultAsync(x => x.Id == id);
        return t == null ? null : MapToDto(t);
    }

    public async Task<TeacherDto> CreateAsync(CreateTeacherRequest request, int? userId)
    {
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Teacher
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var teacher = new Teacher
        {
            UserId = user.Id,
            FullName = request.FullName,
            Phone = request.Phone,
            DepartmentId = request.DepartmentId
        };
        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();

        foreach (var sid in request.SubjectIds)
            context.TeacherSubjects.Add(new TeacherSubject { TeacherId = teacher.Id, SubjectId = sid });
        foreach (var ss in request.StageSections)
            context.TeacherStageSections.Add(new TeacherStageSection { TeacherId = teacher.Id, StageId = ss.StageId, SectionId = ss.SectionId });

        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Create", "Teacher", teacher.Id.ToString());
        return (await GetByIdAsync(teacher.Id))!;
    }

    public async Task<TeacherDto?> UpdateAsync(int id, CreateTeacherRequest request, int? userId)
    {
        var teacher = await context.Teachers.Include(t => t.User)
            .Include(t => t.TeacherSubjects).Include(t => t.TeacherStageSections)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (teacher == null) return null;

        teacher.FullName = request.FullName;
        teacher.Phone = request.Phone;
        teacher.DepartmentId = request.DepartmentId;
        teacher.User.Email = request.Email;
        teacher.User.Username = request.Username;
        if (!string.IsNullOrEmpty(request.Password))
            teacher.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        context.TeacherSubjects.RemoveRange(teacher.TeacherSubjects);
        context.TeacherStageSections.RemoveRange(teacher.TeacherStageSections);
        foreach (var sid in request.SubjectIds)
            context.TeacherSubjects.Add(new TeacherSubject { TeacherId = id, SubjectId = sid });
        foreach (var ss in request.StageSections)
            context.TeacherStageSections.Add(new TeacherStageSection { TeacherId = id, StageId = ss.StageId, SectionId = ss.SectionId });

        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Update", "Teacher", id.ToString());
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id, int? userId)
    {
        var teacher = await context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
        if (teacher == null) return false;
        teacher.User.IsActive = false;
        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Delete", "Teacher", id.ToString());
        return true;
    }

    private static TeacherDto MapToDto(Teacher t) => new(
        t.Id, t.UserId, t.FullName, t.User.Email, t.User.Username, t.Phone,
        t.DepartmentId, t.Department?.Name,
        t.TeacherSubjects.Select(ts => ts.SubjectId).ToList(),
        t.TeacherStageSections.Select(tss => new TeacherStageSectionDto(
            tss.StageId, tss.SectionId, tss.Stage.Name, tss.Section.Name)).ToList()
    );
}

public interface ILookupService
{
    Task<object> GetAllLookupsAsync();
    Task<List<StudentDto>> SearchAsync(SearchRequest request);
}

public class LookupService(AppDbContext context) : ILookupService
{
    public async Task<object> GetAllLookupsAsync()
    {
        return new
        {
            Departments = await context.Departments.Select(d => new LookupDto(d.Id, d.Name)).ToListAsync(),
            Stages = await context.Stages.OrderBy(s => s.Order).Select(s => new LookupDto(s.Id, s.Name)).ToListAsync(),
            Sections = await context.Sections.Select(s => new LookupDto(s.Id, s.Name)).ToListAsync(),
            Subjects = await context.Subjects.Select(s => new LookupDto(s.Id, s.Name)).ToListAsync()
        };
    }

    public async Task<List<StudentDto>> SearchAsync(SearchRequest request)
    {
        var query = context.Students
            .Include(s => s.Stage).Include(s => s.Section).Include(s => s.Department)
            .Where(s => s.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var q = request.Query.Trim();
            query = query.Where(s => s.FullName.Contains(q) || s.UniversityNumber.Contains(q));
        }
        if (request.StageId.HasValue) query = query.Where(s => s.StageId == request.StageId);
        if (request.SectionId.HasValue) query = query.Where(s => s.SectionId == request.SectionId);

        return await query.Take(100).Select(s => new StudentDto(
            s.Id, s.FullName, s.UniversityNumber,
            s.StageId, s.Stage.Name, s.SectionId, s.Section.Name,
            s.DepartmentId, s.Department.Name, s.IsActive
        )).ToListAsync();
    }
}
