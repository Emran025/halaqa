using Halaqa.Desktop.Config.Persistence;
using Halaqa.Desktop.Features.Auth.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Auth.Data.Mappers;
using Halaqa.Desktop.Features.Auth.Data.Models;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Data.Repositories;

internal sealed class AuthRepository(
    IAuthRemoteDataSource remoteDataSource,
    IAuthSessionStore sessionStore) : IAuthRepository
{
    public async Task<Result<AuthenticatedUser>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.LoginAsync(new LoginRequestDto(email, password), cancellationToken);
        if (!response.IsSuccess || response.Value is null)
        {
            return Result<AuthenticatedUser>.Failure(response.Error!);
        }

        try
        {
            var authenticatedUser = AuthMapper.ToDomain(response.Value);
            var session = new AuthSession(
                authenticatedUser.User.Id.ToString(),
                authenticatedUser.User.Role == UserRole.Teacher ? "teacher" : "student",
                authenticatedUser.User.Name,
                authenticatedUser.User.Email,
                authenticatedUser.AccessToken,
                authenticatedUser.ExpiresAt);
            await sessionStore.SaveAsync(session, cancellationToken);
            return Result<AuthenticatedUser>.Success(authenticatedUser);
        }
        catch (InvalidOperationException)
        {
            return Result<AuthenticatedUser>.Failure(new AppError(
                AppErrorKind.Unknown,
                "أعاد الخادم دوراً غير متوقع. تعذر بدء الجلسة بأمان."));
        }
    }

    public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.LogoutAsync(cancellationToken);
        await sessionStore.ClearAsync(cancellationToken);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error!);
    }
}
