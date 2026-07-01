using AcademicAttendance.API.Data;
using AcademicAttendance.API.DTOs;
using AcademicAttendance.API.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace AcademicAttendance.API.Services;

public interface IStudentService
{
    Task<List<StudentDto>> GetAllAsync(int? stageId, int? sectionId, int? departmentId);
    Task<StudentDto?> GetByIdAsync(int id);
    Task<StudentDto> CreateAsync(CreateStudentRequest request, int? userId);
    Task<StudentDto?> UpdateAsync(int id, CreateStudentRequest request, int? userId);
    Task<bool> DeleteAsync(int id, int? userId);
    Task<int> ImportAsync(Stream fileStream, int? userId);
    Task<byte[]> ExportAsync(int? stageId, int? sectionId);
}

public class StudentService(AppDbContext context, IAuditService audit) : IStudentService
{
    public async Task<List<StudentDto>> GetAllAsync(int? stageId, int? sectionId, int? departmentId)
    {
        var query = context.Students
            .Include(s => s.Stage)
            .Include(s => s.Section)
            .Include(s => s.Department)
            .Where(s => s.IsActive)
            .AsQueryable();

        if (stageId.HasValue) query = query.Where(s => s.StageId == stageId);
        if (sectionId.HasValue) query = query.Where(s => s.SectionId == sectionId);
        if (departmentId.HasValue) query = query.Where(s => s.DepartmentId == departmentId);

        var students = await query.ToListAsync();
        return students.Select(MapToDto).ToList();
    }

    public async Task<StudentDto?> GetByIdAsync(int id)
    {
        var s = await context.Students
            .Include(x => x.Stage).Include(x => x.Section).Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id);
        return s == null ? null : MapToDto(s);
    }

    public async Task<StudentDto> CreateAsync(CreateStudentRequest request, int? userId)
    {
        var student = new Student
        {
            FullName = request.FullName,
            UniversityNumber = request.UniversityNumber,
            StageId = request.StageId,
            SectionId = request.SectionId,
            DepartmentId = request.DepartmentId
        };
        context.Students.Add(student);
        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Create", "Student", student.Id.ToString(), student.FullName);
        return (await GetByIdAsync(student.Id))!;
    }

    public async Task<StudentDto?> UpdateAsync(int id, CreateStudentRequest request, int? userId)
    {
        var student = await context.Students.FindAsync(id);
        if (student == null) return null;

        student.FullName = request.FullName;
        student.UniversityNumber = request.UniversityNumber;
        student.StageId = request.StageId;
        student.SectionId = request.SectionId;
        student.DepartmentId = request.DepartmentId;
        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Update", "Student", id.ToString());
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id, int? userId)
    {
        var student = await context.Students.FindAsync(id);
        if (student == null) return false;
        student.IsActive = false;
        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Delete", "Student", id.ToString());
        return true;
    }

    public async Task<int> ImportAsync(Stream fileStream, int? userId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(fileStream);
        var sheet = package.Workbook.Worksheets[0];
        var count = 0;

        for (var row = 2; row <= sheet.Dimension.End.Row; row++)
        {
            var name = sheet.Cells[row, 1].Text?.Trim();
            var uniNum = sheet.Cells[row, 2].Text?.Trim();
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(uniNum)) continue;

            var stageName = sheet.Cells[row, 3].Text?.Trim() ?? "1";
            var sectionName = sheet.Cells[row, 4].Text?.Trim() ?? "A";
            var deptName = sheet.Cells[row, 5].Text?.Trim() ?? "CS";

            var stage = await context.Stages.FirstOrDefaultAsync(s =>
                s.Name.Contains(stageName) || s.Order.ToString() == stageName) ?? await context.Stages.FirstAsync();
            var section = await context.Sections.FirstOrDefaultAsync(s => s.Name == sectionName) ?? await context.Sections.FirstAsync();
            var dept = await context.Departments.FirstOrDefaultAsync(d => d.Code == deptName || d.Name.Contains(deptName)) ?? await context.Departments.FirstAsync();

            if (await context.Students.AnyAsync(s => s.UniversityNumber == uniNum)) continue;

            context.Students.Add(new Student
            {
                FullName = name,
                UniversityNumber = uniNum,
                StageId = stage.Id,
                SectionId = section.Id,
                DepartmentId = dept.Id
            });
            count++;
        }

        await context.SaveChangesAsync();
        await audit.LogAsync(userId, "Import", "Student", null, $"Imported {count} students");
        return count;
    }

    public async Task<byte[]> ExportAsync(int? stageId, int? sectionId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var students = await GetAllAsync(stageId, sectionId, null);
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Students");
        sheet.Cells[1, 1].Value = "الاسم";
        sheet.Cells[1, 2].Value = "الرقم الجامعي";
        sheet.Cells[1, 3].Value = "المرحلة";
        sheet.Cells[1, 4].Value = "الشعبة";
        sheet.Cells[1, 5].Value = "القسم";

        for (var i = 0; i < students.Count; i++)
        {
            var s = students[i];
            sheet.Cells[i + 2, 1].Value = s.FullName;
            sheet.Cells[i + 2, 2].Value = s.UniversityNumber;
            sheet.Cells[i + 2, 3].Value = s.StageName;
            sheet.Cells[i + 2, 4].Value = s.SectionName;
            sheet.Cells[i + 2, 5].Value = s.DepartmentName;
        }

        return package.GetAsByteArray();
    }

    private static StudentDto MapToDto(Student s) => new(
        s.Id, s.FullName, s.UniversityNumber,
        s.StageId, s.Stage.Name, s.SectionId, s.Section.Name,
        s.DepartmentId, s.Department.Name, s.IsActive
    );
}
