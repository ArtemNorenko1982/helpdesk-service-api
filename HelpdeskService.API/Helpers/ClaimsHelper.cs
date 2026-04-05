using HelpdeskService.Core.Entities;
using System.Security.Claims;

namespace HelpdeskService.API.Helpers
{
    public static class ClaimsHelper
    {
        public static int GetCurrentUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User ID claim not found."));

        public static UserRole GetCurrentUserRole(this ClaimsPrincipal user) =>
            Enum.TryParse<UserRole>(user.FindFirstValue(ClaimTypes.Role), out var role)
                ? role
                : UserRole.User;

        public static string GetCurrentUserName(this ClaimsPrincipal user) =>
            user.Identity?.Name ?? string.Empty;

        public static string GetCurrentUserEmail(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email") ?? string.Empty;
    }
}
