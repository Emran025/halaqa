using Halaqa.Desktop.Features.FollowUp.Data.DataSources.Remote;
using Halaqa.Desktop.Features.FollowUp.Data.Mappers;
using Halaqa.Desktop.Features.FollowUp.Data.Models;
using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.FollowUp.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.FollowUp.Data.Repositories;

internal sealed class FollowUpRepository : IFollowUpRepository
{

    private readonly IFollowUpRemoteDataSource remoteDataSource;


    public FollowUpRepository(

        IFollowUpRemoteDataSource remoteDataSource

    )

    {

        this.remoteDataSource = remoteDataSource;

    }

    public async Task<Result<FollowUpPlan>> GetPlanAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.GetPlanAsync(studentId, cancellationToken);
        return response.IsSuccess && response.Value is not null
            ? FollowUpMapper.ToDomain(response.Value)
            : Result<FollowUpPlan>.Failure(response.Error!);
    }

    public async Task<Result<FollowUpPlan>> UpdatePlanAsync(UpdateFollowUpPlanCommand command, CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.UpdatePlanAsync(command.StudentId, FollowUpMapper.ToDto(command), cancellationToken);
        return response.IsSuccess && response.Value is not null
            ? FollowUpMapper.ToDomain(response.Value)
            : Result<FollowUpPlan>.Failure(response.Error!);
    }

    public async Task<Result<AttendancePreferences>> GetAvailabilityAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.GetAvailabilityAsync(studentId, cancellationToken);
        return response.IsSuccess && response.Value is not null
            ? FollowUpMapper.ToDomain(response.Value)
            : Result<AttendancePreferences>.Failure(response.Error!);
    }

    public async Task<Result<AttendancePreferences>> UpdateAvailabilityAsync(UpdateAvailabilityCommand command, CancellationToken cancellationToken = default)
    {
        var request = FollowUpMapper.ToDto(command.Preferences);
        var response = await remoteDataSource.UpdateAvailabilityAsync(command.StudentId, request, cancellationToken);
        return response.IsSuccess && response.Value is not null
            ? FollowUpMapper.ToDomain(response.Value)
            : Result<AttendancePreferences>.Failure(response.Error!);
    }

    public async Task<Result<FollowUpItemPage>> ListItemsAsync(FollowUpItemQuery query, CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.ListItemsAsync(query, cancellationToken);
        return response.IsSuccess && response.Value is not null
            ? FollowUpMapper.ToDomain(response.Value)
            : Result<FollowUpItemPage>.Failure(response.Error!);
    }

    public async Task<Result<FollowUpItem>> CompleteItemAsync(Guid itemId, Guid clientOperationId, CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.CompleteItemAsync(itemId, new CompleteFollowUpInputDto(clientOperationId), cancellationToken);
        return response.IsSuccess && response.Value is not null
            ? FollowUpMapper.ToDomain(response.Value)
            : Result<FollowUpItem>.Failure(response.Error!);
    }

    public async Task<Result<FollowUpItem>> SkipItemAsync(Guid itemId, string reason, Guid clientOperationId, CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.SkipItemAsync(itemId, new SkipFollowUpInputDto(reason.Trim(), clientOperationId), cancellationToken);
        return response.IsSuccess && response.Value is not null
            ? FollowUpMapper.ToDomain(response.Value)
            : Result<FollowUpItem>.Failure(response.Error!);
    }

    public async Task<Result<FollowUpItem>> RescheduleItemAsync(RescheduleFollowUpItemCommand command, CancellationToken cancellationToken = default)
    {
        var request = new RescheduleFollowUpInputDto(command.ScheduledAt, command.Timezone?.Trim(), NormalizeOptional(command.Reason), command.ClientOperationId);
        var response = await remoteDataSource.RescheduleItemAsync(command.ItemId, request, cancellationToken);
        return response.IsSuccess && response.Value is not null
            ? FollowUpMapper.ToDomain(response.Value)
            : Result<FollowUpItem>.Failure(response.Error!);
    }

    public async Task<Result<TrackingPage>> ListTrackingsAsync(Guid studentId, DateOnly? from, DateOnly? to, int page, int perPage, CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.ListTrackingsAsync(studentId, from, to, page, perPage, cancellationToken);
        return response.IsSuccess && response.Value is not null
            ? FollowUpMapper.ToDomain(response.Value)
            : Result<TrackingPage>.Failure(response.Error!);
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
