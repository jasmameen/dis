namespace AcademicAttendance.API.Models;

public class Teacher
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int? DepartmentId { get; set; }

    public User User { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<TeacherSubject> TeacherSubjects { get; set; } = [];
    public ICollection<TeacherStageSection> TeacherStageSections { get; set; } = [];
    public ICollection<Attendance> Attendances { get; set; } = [];
}
