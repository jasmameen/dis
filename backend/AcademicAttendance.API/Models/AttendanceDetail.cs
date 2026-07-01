namespace AcademicAttendance.API.Models;

public class AttendanceDetail
{
    public int Id { get; set; }
    public int AttendanceId { get; set; }
    public int StudentId { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }

    public Attendance Attendance { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
