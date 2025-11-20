using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AppointmentSystem.Data;

namespace AppointmentSystem.Services;

/// <summary>
/// Cari istifadəçi məlumatları servisi
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;

    // Cache üçün
    private Guid? _cachedTeacherId;
    private Guid? _cachedParentId;
    private Guid? _cachedStudentId;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        AppDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? FullName => _httpContextAccessor.HttpContext?.User?.FindFirst("FullName")?.Value;

    public Guid? CompanyId
    {
        get
        {
            var companyIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("CompanyId")?.Value;
            return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public IEnumerable<string> GetRoles()
    {
        return _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value)
               ?? Enumerable.Empty<string>();
    }

    /// <summary>
    /// Teacher ID-ni qaytarır (cache ilə)
    /// </summary>
    public async Task<Guid?> GetTeacherIdAsync()
    {
        if (_cachedTeacherId.HasValue)
            return _cachedTeacherId;

        if (!UserId.HasValue)
            return null;

        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.UserId == UserId.Value && t.IsActive);

        _cachedTeacherId = teacher?.Id;
        return _cachedTeacherId;
    }

    /// <summary>
    /// Parent ID-ni qaytarır (cache ilə)
    /// </summary>
    public async Task<Guid?> GetParentIdAsync()
    {
        if (_cachedParentId.HasValue)
            return _cachedParentId;

        if (!UserId.HasValue)
            return null;

        var parent = await _context.Parents
            .FirstOrDefaultAsync(p => p.UserId == UserId.Value && p.IsActive);

        _cachedParentId = parent?.Id;
        return _cachedParentId;
    }

    /// <summary>
    /// Student ID-ni qaytarır (cache ilə)
    /// </summary>
    //public async Task<Guid?> GetStudentIdAsync()
    //{
    //    if (_cachedStudentId.HasValue)
    //        return _cachedStudentId;

    //    if (!UserId.HasValue)
    //        return null;

    //    var student = await _context.Students
    //        .FirstOrDefaultAsync(s => s.UserId == UserId.Value && s.IsActive);

    //    _cachedStudentId = student?.Id;
    //    return _cachedStudentId;
    //}
}