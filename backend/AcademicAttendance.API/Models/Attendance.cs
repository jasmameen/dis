namespace AcademicAttendance.API.Models;

public class Attendance
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public int TeacherId { get; set; }
    public int StageId { get; set; }
    public int SectionId { get; set; }
    public DateOnly Date { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Subject Subject { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public Stage Stage { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public ICollection<AttendanceDetail> Details { get; set; } = [];
}
