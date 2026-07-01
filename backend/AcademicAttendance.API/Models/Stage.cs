namespace AcademicAttendance.API.Models;

public class Stage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public int Order { get; set; }

    public ICollection<Student> Students { get; set; } = [];
    public ICollection<Subject> Subjects { get; set; } = [];
    public ICollection<TeacherStageSection> TeacherStageSections { get; set; } = [];
}
