using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.Progress.Data.Models;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Progress.Data.DataSources.Remote;

internal interface IStudentProgressRemoteDataSource
{
    Task<Result<StudentProgressResponseDto>> GetAsync(Guid studentId, string? taskType, CancellationToken cancellationToken = default);
}

internal sealed class StudentProgressRemoteDataSource : IStudentProgressRemoteDataSource
{
    private readonly IApiClient apiClient;

    public StudentProgressRemoteDataSource(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public Task<Result<StudentProgressResponseDto>> GetAsync(Guid studentId, string? taskType, CancellationToken cancellationToken = default)
    {
        var path = $"students/{studentId}/progress";
        if (!string.IsNullOrWhiteSpace(taskType))
        {
            path += $"?task_type={Uri.EscapeDataString(taskType)}";
        }
        return apiClient.GetAsync<StudentProgressResponseDto>(path, cancellationToken);
    }
}
