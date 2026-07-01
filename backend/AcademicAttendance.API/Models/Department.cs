namespace AcademicAttendance.API.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string Code { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; } = [];
    public ICollection<Teacher> Teachers { get; set; } = [];
    public ICollection<Subject> Subjects { get; set; } = [];
}
