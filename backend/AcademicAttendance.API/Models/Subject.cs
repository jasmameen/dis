namespace AcademicAttendance.API.Models;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int StageId { get; set; }
    public int DepartmentId { get; set; }
    public int CreditHours { get; set; }

    public Stage Stage { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public ICollection<TeacherSubject> TeacherSubjects { get; set; } = [];
    public ICollection<Attendance> Attendances { get; set; } = [];
}
