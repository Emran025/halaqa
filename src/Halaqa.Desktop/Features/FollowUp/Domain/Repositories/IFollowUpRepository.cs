using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.FollowUp.Domain.Repositories;

public interface IFollowUpRepository
{
    Task<Result<FollowUpPlan>> GetPlanAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<Result<FollowUpPlan>> UpdatePlanAsync(UpdateFollowUpPlanCommand command, CancellationToken cancellationToken = default);
    Task<Result<AttendancePreferences>> GetAvailabilityAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<Result<AttendancePreferences>> UpdateAvailabilityAsync(UpdateAvailabilityCommand command, CancellationToken cancellationToken = default);
    Task<Result<FollowUpItemPage>> ListItemsAsync(FollowUpItemQuery query, CancellationToken cancellationToken = default);
    Task<Result<FollowUpItem>> CompleteItemAsync(Guid itemId, Guid clientOperationId, CancellationToken cancellationToken = default);
    Task<Result<FollowUpItem>> SkipItemAsync(Guid itemId, string reason, Guid clientOperationId, CancellationToken cancellationToken = default);
    Task<Result<FollowUpItem>> RescheduleItemAsync(RescheduleFollowUpItemCommand command, CancellationToken cancellationToken = default);
    Task<Result<TrackingPage>> ListTrackingsAsync(Guid studentId, DateOnly? from, DateOnly? to, int page, int perPage, CancellationToken cancellationToken = default);
}
