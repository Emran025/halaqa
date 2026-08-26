using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Auth.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Data.DataSources.Remote;

internal interface IAuthRemoteDataSource
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RegisterStudentAsync(StudentRegistrationRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RegisterTeacherAsync(TeacherRegistrationRequestDto request, CancellationToken cancellationToken = default);
    Task<Result> RequestPasswordResetAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(CancellationToken cancellationToken = default);
}

internal sealed class AuthRemoteDataSource : IAuthRemoteDataSource
{

    private readonly IApiClient apiClient;


    public AuthRemoteDataSource(

        IApiClient apiClient

    )

    {

        this.apiClient = apiClient;

    }

    public Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<LoginRequestDto, AuthResponseDto>("auth/login", request, cancellationToken);

    public Task<Result<AuthResponseDto>> RegisterStudentAsync(StudentRegistrationRequestDto request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<StudentRegistrationRequestDto, AuthResponseDto>("auth/register/student", request, cancellationToken);

    public Task<Result<AuthResponseDto>> RegisterTeacherAsync(TeacherRegistrationRequestDto request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<TeacherRegistrationRequestDto, AuthResponseDto>("auth/register/teacher", request, cancellationToken);

    public Task<Result> RequestPasswordResetAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync("auth/password/forgot", request, cancellationToken);

    public Task<Result> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync("auth/password/reset", request, cancellationToken);

    public Task<Result> ChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync("auth/password/change", request, cancellationToken);

    public Task<Result> LogoutAsync(CancellationToken cancellationToken = default) =>
        apiClient.PostAsync("auth/logout", new { }, cancellationToken);
}
