using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.FollowUp.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.FollowUp.Domain.UseCases;

public sealed class GetFollowUpPlanUseCase(IFollowUpRepository repository)
{
    public Task<Result<FollowUpPlan>> ExecuteAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        repository.GetPlanAsync(studentId, cancellationToken);
}

public sealed class UpdateFollowUpPlanUseCase(IFollowUpRepository repository)
{
    public Task<Result<FollowUpPlan>> ExecuteAsync(UpdateFollowUpPlanCommand command, CancellationToken cancellationToken = default) =>
        repository.UpdatePlanAsync(command, cancellationToken);
}

public sealed class GetAvailabilityUseCase(IFollowUpRepository repository)
{
    public Task<Result<AttendancePreferences>> ExecuteAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        repository.GetAvailabilityAsync(studentId, cancellationToken);
}

public sealed class UpdateAvailabilityUseCase(IFollowUpRepository repository)
{
    public Task<Result<AttendancePreferences>> ExecuteAsync(UpdateAvailabilityCommand command, CancellationToken cancellationToken = default) =>
        repository.UpdateAvailabilityAsync(command, cancellationToken);
}

public sealed class ListFollowUpItemsUseCase(IFollowUpRepository repository)
{
    public Task<Result<FollowUpItemPage>> ExecuteAsync(FollowUpItemQuery query, CancellationToken cancellationToken = default) =>
        repository.ListItemsAsync(query, cancellationToken);
}

public sealed class CompleteFollowUpItemUseCase(IFollowUpRepository repository)
{
    public Task<Result<FollowUpItem>> ExecuteAsync(Guid itemId, Guid clientOperationId, CancellationToken cancellationToken = default) =>
        repository.CompleteItemAsync(itemId, clientOperationId, cancellationToken);
}

public sealed class SkipFollowUpItemUseCase(IFollowUpRepository repository)
{
    public Task<Result<FollowUpItem>> ExecuteAsync(Guid itemId, string reason, Guid clientOperationId, CancellationToken cancellationToken = default) =>
        repository.SkipItemAsync(itemId, reason, clientOperationId, cancellationToken);
}

public sealed class RescheduleFollowUpItemUseCase(IFollowUpRepository repository)
{
    public Task<Result<FollowUpItem>> ExecuteAsync(RescheduleFollowUpItemCommand command, CancellationToken cancellationToken = default) =>
        repository.RescheduleItemAsync(command, cancellationToken);
}

public sealed class ListStudentTrackingsUseCase(IFollowUpRepository repository)
{
    public Task<Result<TrackingPage>> ExecuteAsync(Guid studentId, DateOnly? from, DateOnly? to, int page, int perPage, CancellationToken cancellationToken = default) =>
        repository.ListTrackingsAsync(studentId, from, to, page, perPage, cancellationToken);
}
