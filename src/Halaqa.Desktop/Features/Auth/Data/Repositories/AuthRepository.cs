using Halaqa.Desktop.Config.Persistence;
using Halaqa.Desktop.Features.Auth.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Auth.Data.Mappers;
using Halaqa.Desktop.Features.Auth.Data.Models;
using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Data.Repositories;

internal sealed class AuthRepository : IAuthRepository
{

    private readonly IAuthRemoteDataSource remoteDataSource;

    private readonly IAuthSessionStore sessionStore;


    public AuthRepository(

        IAuthRemoteDataSource remoteDataSource,

        IAuthSessionStore sessionStore

    )

    {

        this.remoteDataSource = remoteDataSource;

        this.sessionStore = sessionStore;

    }

    public async Task<Result<AuthenticatedUser>> LoginAsync(string email, string password, CancellationToken cancellationToken = default) =>
        await PersistAuthenticationAsync(
            await remoteDataSource.LoginAsync(new LoginRequestDto(email, password), cancellationToken),
            cancellationToken);

    public async Task<Result<AuthenticatedUser>> RegisterStudentAsync(StudentRegistrationCommand command, CancellationToken cancellationToken = default) =>
        await PersistAuthenticationAsync(
            await remoteDataSource.RegisterStudentAsync(RegistrationMapper.ToDto(command), cancellationToken),
            cancellationToken);

    public async Task<Result<AuthenticatedUser>> RegisterTeacherAsync(TeacherRegistrationCommand command, CancellationToken cancellationToken = default) =>
        await PersistAuthenticationAsync(
            await remoteDataSource.RegisterTeacherAsync(RegistrationMapper.ToDto(command), cancellationToken),
            cancellationToken);

    public Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) =>
        remoteDataSource.RequestPasswordResetAsync(new ForgotPasswordRequestDto(email), cancellationToken);

    public Task<Result> ResetPasswordAsync(
        string email,
        string token,
        string password,
        string passwordConfirmation,
        CancellationToken cancellationToken = default) =>
        remoteDataSource.ResetPasswordAsync(new ResetPasswordRequestDto(email, token, password, passwordConfirmation), cancellationToken);

    public Task<Result> ChangePasswordAsync(
        string currentPassword,
        string password,
        string passwordConfirmation,
        CancellationToken cancellationToken = default) =>
        remoteDataSource.ChangePasswordAsync(new ChangePasswordRequestDto(currentPassword, password, passwordConfirmation), cancellationToken);

    public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
    {
        var result = await remoteDataSource.LogoutAsync(cancellationToken);
        await sessionStore.ClearAsync(cancellationToken);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error!);
    }

    private async Task<Result<AuthenticatedUser>> PersistAuthenticationAsync(
        Result<AuthResponseDto> response,
        CancellationToken cancellationToken)
    {
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
}
