using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AcademicAttendance.API.Data;
using AcademicAttendance.API.DTOs;
using AcademicAttendance.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AcademicAttendance.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<(bool Success, string? Error)> RegisterTeacherAsync(RegisterTeacherRequest request);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}

public class AuthService(AppDbContext context, IConfiguration config) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await context.Users
            .Include(u => u.Teacher)
            .FirstOrDefaultAsync(u =>
                (u.Email == request.Login || u.Username == request.Login) && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = GenerateToken(user);
        return new LoginResponse(
            token,
            user.Username,
            user.Email,
            user.Role,
            user.Teacher?.Id,
            user.Teacher?.FullName
        );
    }

    public async Task<(bool Success, string? Error)> RegisterTeacherAsync(RegisterTeacherRequest request)
    {
        if (await context.Users.AnyAsync(u => u.Email == request.Email))
            return (false, "البريد الإلكتروني مستخدم مسبقاً");
        if (await context.Users.AnyAsync(u => u.Username == request.Username))
            return (false, "اسم المستخدم مستخدم مسبقاً");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Teacher,
            IsActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var teacher = new Teacher
        {
            UserId = user.Id,
            FullName = request.FullName,
            Phone = request.Phone,
            DepartmentId = request.DepartmentId
        };
        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return true;

        user.ResetToken = Guid.NewGuid().ToString("N");
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(24);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await context.Users.FirstOrDefaultAsync(u =>
            u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        await context.SaveChangesAsync();
        return true;
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.Teacher != null)
            claims.Add(new Claim("TeacherId", user.Teacher.Id.ToString()));

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
