namespace AcademicAttendance.API.Models;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UniversityNumber { get; set; } = string.Empty;
    public int StageId { get; set; }
    public int SectionId { get; set; }
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Stage Stage { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public ICollection<AttendanceDetail> AttendanceDetails { get; set; } = [];
}
