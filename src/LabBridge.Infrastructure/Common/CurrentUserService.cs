using LabBridge.Application.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;


namespace LabBridge.Infrastructure.Common;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");

            if (Guid.TryParse(userId, out var id))
                return id;

            return null;
        }
    }


    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    //public bool IsAuthenticated =>
    //    Principal?.Identity?.IsAuthenticated == true;

    //public Guid? UserId
    //{
    //    get
    //    {
    //        var value = Principal?.FindFirstValue(
    //            ClaimTypes.NameIdentifier);

    //        return Guid.TryParse(value, out var id) ? id : null;
    //    }
    //}

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    public string? Name => Principal?.FindFirstValue(ClaimTypes.Name);

    public IReadOnlyCollection<string> Roles => Principal?.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray() ?? Array.Empty<string>();
}


