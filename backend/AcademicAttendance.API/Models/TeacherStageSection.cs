namespace AcademicAttendance.API.Models;

public class TeacherStageSection
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public int StageId { get; set; }
    public int SectionId { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public Stage Stage { get; set; } = null!;
    public Section Section { get; set; } = null!;
}
