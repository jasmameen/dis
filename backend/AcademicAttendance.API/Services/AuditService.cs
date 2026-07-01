using AcademicAttendance.API.Data;
using AcademicAttendance.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicAttendance.API.Services;

public interface IAuditService
{
    Task LogAsync(int? userId, string action, string entity, string? entityId = null, string? details = null, string? ip = null);
}

public class AuditService(AppDbContext context) : IAuditService
{
    public async Task LogAsync(int? userId, string action, string entity, string? entityId = null, string? details = null, string? ip = null)
    {
        context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            Details = details,
            IpAddress = ip
        });
        await context.SaveChangesAsync();
    }
}
