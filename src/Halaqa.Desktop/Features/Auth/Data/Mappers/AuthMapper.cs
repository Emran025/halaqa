using Halaqa.Desktop.Features.Auth.Data.Models;
using Halaqa.Desktop.Features.Auth.Domain.Entities;

namespace Halaqa.Desktop.Features.Auth.Data.Mappers;

internal static class AuthMapper
{
    public static AuthenticatedUser ToDomain(AuthResponseDto response)
    {
        var role = response.User.Role switch
        {
            "teacher" => UserRole.Teacher,
            "student" => UserRole.Student,
            _ => throw new InvalidOperationException("أعاد الخادم دور مستخدم غير مدعوم.")
        };

        return new AuthenticatedUser(
            new AuthUser(response.User.Id, role, response.User.Name, response.User.Email, response.User.Status),
            response.Token,
            response.ExpiresAt);
    }
}
