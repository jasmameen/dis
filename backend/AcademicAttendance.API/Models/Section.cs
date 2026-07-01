namespace AcademicAttendance.API.Models;

public class Section
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; } = [];
    public ICollection<TeacherStageSection> TeacherStageSections { get; set; } = [];
}
