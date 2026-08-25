using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Registrations.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Data.DataSources.Remote;

internal interface IStudentRegistrationRemoteDataSource
{
    Task<Result<TeacherPublicCollectionResponseDto>> ListAvailableTeachersAsync(
        string? code,
        string? search,
        int page,
        CancellationToken cancellationToken = default);

    Task<Result<TeacherPublicResponseDto>> GetPublicTeacherAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<Result<RegistrationResponseDto>> CreateAsync(
        CreateStudentRegistrationRequestDto request,
        CancellationToken cancellationToken = default);
}

internal sealed class StudentRegistrationRemoteDataSource(IApiClient apiClient) : IStudentRegistrationRemoteDataSource
{
    public Task<Result<TeacherPublicCollectionResponseDto>> ListAvailableTeachersAsync(
        string? code,
        string? search,
        int page,
        CancellationToken cancellationToken = default)
    {
        var query = $"teachers?page={page}";
        if (!string.IsNullOrWhiteSpace(code))
        {
            query += $"&code={Uri.EscapeDataString(code)}";
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search)}";
        }

        return apiClient.GetAsync<TeacherPublicCollectionResponseDto>(query, cancellationToken);
    }

    public Task<Result<TeacherPublicResponseDto>> GetPublicTeacherAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<TeacherPublicResponseDto>($"teachers/{teacherId}", cancellationToken);

    public Task<Result<RegistrationResponseDto>> CreateAsync(
        CreateStudentRegistrationRequestDto request,
        CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<CreateStudentRegistrationRequestDto, RegistrationResponseDto>(
            "registration-requests",
            request,
            cancellationToken);
}
