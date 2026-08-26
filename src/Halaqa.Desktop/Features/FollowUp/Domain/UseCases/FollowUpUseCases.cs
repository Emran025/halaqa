using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.FollowUp.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.FollowUp.Domain.UseCases;

public sealed class GetFollowUpPlanUseCase
{

    private readonly IFollowUpRepository repository;


    public GetFollowUpPlanUseCase(

        IFollowUpRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<FollowUpPlan>> ExecuteAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        repository.GetPlanAsync(studentId, cancellationToken);
}

public sealed class UpdateFollowUpPlanUseCase
{

    private readonly IFollowUpRepository repository;


    public UpdateFollowUpPlanUseCase(

        IFollowUpRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<FollowUpPlan>> ExecuteAsync(UpdateFollowUpPlanCommand command, CancellationToken cancellationToken = default) =>
        repository.UpdatePlanAsync(command, cancellationToken);
}

public sealed class GetAvailabilityUseCase
{

    private readonly IFollowUpRepository repository;


    public GetAvailabilityUseCase(

        IFollowUpRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<AttendancePreferences>> ExecuteAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        repository.GetAvailabilityAsync(studentId, cancellationToken);
}

public sealed class UpdateAvailabilityUseCase
{

    private readonly IFollowUpRepository repository;


    public UpdateAvailabilityUseCase(

        IFollowUpRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<AttendancePreferences>> ExecuteAsync(UpdateAvailabilityCommand command, CancellationToken cancellationToken = default) =>
        repository.UpdateAvailabilityAsync(command, cancellationToken);
}

public sealed class ListFollowUpItemsUseCase
{

    private readonly IFollowUpRepository repository;


    public ListFollowUpItemsUseCase(

        IFollowUpRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<FollowUpItemPage>> ExecuteAsync(FollowUpItemQuery query, CancellationToken cancellationToken = default) =>
        repository.ListItemsAsync(query, cancellationToken);
}

public sealed class CompleteFollowUpItemUseCase
{

    private readonly IFollowUpRepository repository;


    public CompleteFollowUpItemUseCase(

        IFollowUpRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<FollowUpItem>> ExecuteAsync(Guid itemId, Guid clientOperationId, CancellationToken cancellationToken = default) =>
        repository.CompleteItemAsync(itemId, clientOperationId, cancellationToken);
}

public sealed class SkipFollowUpItemUseCase
{

    private readonly IFollowUpRepository repository;


    public SkipFollowUpItemUseCase(

        IFollowUpRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<FollowUpItem>> ExecuteAsync(Guid itemId, string reason, Guid clientOperationId, CancellationToken cancellationToken = default) =>
        repository.SkipItemAsync(itemId, reason, clientOperationId, cancellationToken);
}

public sealed class RescheduleFollowUpItemUseCase
{

    private readonly IFollowUpRepository repository;


    public RescheduleFollowUpItemUseCase(

        IFollowUpRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<FollowUpItem>> ExecuteAsync(RescheduleFollowUpItemCommand command, CancellationToken cancellationToken = default) =>
        repository.RescheduleItemAsync(command, cancellationToken);
}

public sealed class ListStudentTrackingsUseCase
{

    private readonly IFollowUpRepository repository;


    public ListStudentTrackingsUseCase(

        IFollowUpRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<TrackingPage>> ExecuteAsync(Guid studentId, DateOnly? from, DateOnly? to, int page, int perPage, CancellationToken cancellationToken = default) =>
        repository.ListTrackingsAsync(studentId, from, to, page, perPage, cancellationToken);
}
