using System.Globalization;
using Halaqa.Desktop.Config.Http;
using Halaqa.Desktop.Features.FollowUp.Data.Models;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.FollowUp.Data.DataSources.Remote;

internal interface IFollowUpRemoteDataSource
{
    Task<Result<FollowUpPlanResponseDto>> GetPlanAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<Result<FollowUpPlanResponseDto>> UpdatePlanAsync(Guid studentId, FollowUpPlanInputDto request, CancellationToken cancellationToken = default);
    Task<Result<AttendancePreferencesResponseDto>> GetAvailabilityAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<Result<AttendancePreferencesResponseDto>> UpdateAvailabilityAsync(Guid studentId, AttendancePreferencesDto request, CancellationToken cancellationToken = default);
    Task<Result<FollowUpItemCollectionResponseDto>> ListItemsAsync(FollowUpItemQuery query, CancellationToken cancellationToken = default);
    Task<Result<FollowUpItemResponseDto>> CompleteItemAsync(Guid itemId, CompleteFollowUpInputDto request, CancellationToken cancellationToken = default);
    Task<Result<FollowUpItemResponseDto>> SkipItemAsync(Guid itemId, SkipFollowUpInputDto request, CancellationToken cancellationToken = default);
    Task<Result<FollowUpItemResponseDto>> RescheduleItemAsync(Guid itemId, RescheduleFollowUpInputDto request, CancellationToken cancellationToken = default);
    Task<Result<TrackingCollectionResponseDto>> ListTrackingsAsync(Guid studentId, DateOnly? from, DateOnly? to, int page, int perPage, CancellationToken cancellationToken = default);
}

internal sealed class FollowUpRemoteDataSource : IFollowUpRemoteDataSource
{

    private readonly IApiClient apiClient;


    public FollowUpRemoteDataSource(

        IApiClient apiClient

    )

    {

        this.apiClient = apiClient;

    }

    public Task<Result<FollowUpPlanResponseDto>> GetPlanAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<FollowUpPlanResponseDto>($"students/{studentId}/follow-up-plan", cancellationToken);

    public Task<Result<FollowUpPlanResponseDto>> UpdatePlanAsync(Guid studentId, FollowUpPlanInputDto request, CancellationToken cancellationToken = default) =>
        apiClient.PutAsync<FollowUpPlanInputDto, FollowUpPlanResponseDto>($"students/{studentId}/follow-up-plan", request, cancellationToken);

    public Task<Result<AttendancePreferencesResponseDto>> GetAvailabilityAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<AttendancePreferencesResponseDto>($"students/{studentId}/availability", cancellationToken);

    public Task<Result<AttendancePreferencesResponseDto>> UpdateAvailabilityAsync(Guid studentId, AttendancePreferencesDto request, CancellationToken cancellationToken = default) =>
        apiClient.PutAsync<AttendancePreferencesDto, AttendancePreferencesResponseDto>($"students/{studentId}/availability", request, cancellationToken);

    public Task<Result<FollowUpItemCollectionResponseDto>> ListItemsAsync(FollowUpItemQuery query, CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<FollowUpItemCollectionResponseDto>(BuildItemsPath(query), cancellationToken);

    public Task<Result<FollowUpItemResponseDto>> CompleteItemAsync(Guid itemId, CompleteFollowUpInputDto request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<CompleteFollowUpInputDto, FollowUpItemResponseDto>($"follow-up-items/{itemId}/complete", request, cancellationToken);

    public Task<Result<FollowUpItemResponseDto>> SkipItemAsync(Guid itemId, SkipFollowUpInputDto request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<SkipFollowUpInputDto, FollowUpItemResponseDto>($"follow-up-items/{itemId}/skip", request, cancellationToken);

    public Task<Result<FollowUpItemResponseDto>> RescheduleItemAsync(Guid itemId, RescheduleFollowUpInputDto request, CancellationToken cancellationToken = default) =>
        apiClient.PostAsync<RescheduleFollowUpInputDto, FollowUpItemResponseDto>($"follow-up-items/{itemId}/reschedule", request, cancellationToken);

    public Task<Result<TrackingCollectionResponseDto>> ListTrackingsAsync(Guid studentId, DateOnly? from, DateOnly? to, int page, int perPage, CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<TrackingCollectionResponseDto>(BuildTrackingsPath(studentId, from, to, page, perPage), cancellationToken);

    private static string BuildItemsPath(FollowUpItemQuery query)
    {
        var parameters = new List<string>
        {
            $"page={query.Page}",
            $"per_page={query.PerPage}"
        };
        if (query.Date is { } date)
        {
            parameters.Add($"date={date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        }
        if (query.State is { } state)
        {
            parameters.Add($"state={ToContractState(state)}");
        }
        if (query.TaskType is { } taskType)
        {
            parameters.Add($"task_type={ToContractValue(taskType)}");
        }
        if (query.StudentId is { } studentId)
        {
            parameters.Add($"student_id={Uri.EscapeDataString(studentId.ToString())}");
        }

        return $"follow-up-items?{string.Join("&", parameters)}";
    }

    private static string BuildTrackingsPath(Guid studentId, DateOnly? from, DateOnly? to, int page, int perPage)
    {
        var parameters = new List<string> { $"page={page}", $"per_page={perPage}" };
        if (from is { } fromDate)
        {
            parameters.Add($"from={fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        }
        if (to is { } toDate)
        {
            parameters.Add($"to={toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        }

        return $"students/{studentId}/trackings?{string.Join("&", parameters)}";
    }

    private static string ToContractValue<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string ToContractState(FollowUpItemState value) => value switch
    {
        FollowUpItemState.InProgress => "in_progress",
        _ => ToContractValue(value)
    };
}
