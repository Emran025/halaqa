using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Profile.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Data.DataSources.Remote;

internal interface IStudentProfileRemoteDataSource
{
    Task<Result<StudentProfileResponseDto>> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<Result<StudentProfileResponseDto>> UpdateCurrentAsync(
        UpdateStudentProfileRequestDto request,
        CancellationToken cancellationToken = default);
}

internal sealed class StudentProfileRemoteDataSource : IStudentProfileRemoteDataSource
{

    private readonly IApiClient apiClient;


    public StudentProfileRemoteDataSource(

        IApiClient apiClient

    )

    {

        this.apiClient = apiClient;

    }

    public Task<Result<StudentProfileResponseDto>> GetCurrentAsync(CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<StudentProfileResponseDto>("me/student-profile", cancellationToken);

    public Task<Result<StudentProfileResponseDto>> UpdateCurrentAsync(
        UpdateStudentProfileRequestDto request,
        CancellationToken cancellationToken = default) =>
        apiClient.PatchAsync<UpdateStudentProfileRequestDto, StudentProfileResponseDto>(
            "me/student-profile",
            request,
            cancellationToken);
}
