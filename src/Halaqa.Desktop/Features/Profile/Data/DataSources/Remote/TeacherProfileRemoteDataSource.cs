using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Profile.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Data.DataSources.Remote;

internal interface ITeacherProfileRemoteDataSource
{
    Task<Result<TeacherProfileResponseDto>> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task<Result<TeacherProfileResponseDto>> UpdateCurrentAsync(
        UpdateTeacherProfileRequestDto request,
        CancellationToken cancellationToken = default);
}

internal sealed class TeacherProfileRemoteDataSource : ITeacherProfileRemoteDataSource
{

    private readonly IApiClient apiClient;


    public TeacherProfileRemoteDataSource(

        IApiClient apiClient

    )

    {

        this.apiClient = apiClient;

    }

    public Task<Result<TeacherProfileResponseDto>> GetCurrentAsync(CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<TeacherProfileResponseDto>("me/teacher-profile", cancellationToken);

    public Task<Result<TeacherProfileResponseDto>> UpdateCurrentAsync(
        UpdateTeacherProfileRequestDto request,
        CancellationToken cancellationToken = default) =>
        apiClient.PatchAsync<UpdateTeacherProfileRequestDto, TeacherProfileResponseDto>(
            "me/teacher-profile",
            request,
            cancellationToken);
}
