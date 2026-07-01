namespace AcademicAttendance.API.Models;

public enum UserRole
{
    Admin = 0,
    Teacher = 1
}

public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2,
    Leave = 3
}

public enum NotificationType
{
    HighAbsence = 0,
    AttendanceNotRecorded = 1,
    AdminAlert = 2
}
