using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Domain.UseCases;

public sealed class ListMyRegistrationRequestsUseCase
{

    private readonly IRegistrationRequestRepository repository;


    public ListMyRegistrationRequestsUseCase(

        IRegistrationRequestRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<RegistrationRequestPage>> ExecuteAsync(
        RegistrationState? state = null,
        int page = 1,
        CancellationToken cancellationToken = default) =>
        page < 1
            ? Task.FromResult(Result<RegistrationRequestPage>.Failure(RegistrationRequestValidationErrors.InvalidPage()))
            : repository.ListMineAsync(state, page, cancellationToken);
}

public sealed class ListHalaqaRegistrationRequestsUseCase
{

    private readonly IRegistrationRequestRepository repository;


    public ListHalaqaRegistrationRequestsUseCase(

        IRegistrationRequestRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<RegistrationRequestPage>> ExecuteAsync(
        Guid halaqaId,
        RegistrationState? state = null,
        int page = 1,
        CancellationToken cancellationToken = default) =>
        halaqaId == Guid.Empty || page < 1
            ? Task.FromResult(Result<RegistrationRequestPage>.Failure(RegistrationRequestValidationErrors.InvalidHalaqaOrPage()))
            : repository.ListForHalaqaAsync(halaqaId, state, page, cancellationToken);
}

public sealed class ListTeacherApplicationInboxUseCase
{

    private readonly IRegistrationRequestRepository repository;


    public ListTeacherApplicationInboxUseCase(

        IRegistrationRequestRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<RegistrationRequestPage>> ExecuteAsync(
        RegistrationState? state = null,
        string? search = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return Task.FromResult(Result<RegistrationRequestPage>.Failure(RegistrationRequestValidationErrors.InvalidPage()));
        }
        if (search?.Trim().Length > 120)
        {
            return Task.FromResult(Result<RegistrationRequestPage>.Failure(new AppError(
                AppErrorKind.Validation,
                "نص البحث لا يتجاوز 120 حرفاً.")));
        }

        return repository.ListTeacherInboxAsync(state, search, page, cancellationToken);
    }
}

public sealed class AcceptRegistrationRequestUseCase
{

    private readonly IRegistrationRequestRepository repository;


    public AcceptRegistrationRequestUseCase(

        IRegistrationRequestRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<RegistrationRequest>> ExecuteAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default) =>
        registrationId == Guid.Empty
            ? Task.FromResult(Result<RegistrationRequest>.Failure(RegistrationRequestValidationErrors.InvalidRegistration()))
            : repository.AcceptAsync(registrationId, cancellationToken);
}

public sealed class RejectRegistrationRequestUseCase
{

    private readonly IRegistrationRequestRepository repository;


    public RejectRegistrationRequestUseCase(

        IRegistrationRequestRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<RegistrationRequest>> ExecuteAsync(
        RejectRegistrationRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.RegistrationId == Guid.Empty)
        {
            return Task.FromResult(Result<RegistrationRequest>.Failure(RegistrationRequestValidationErrors.InvalidRegistration()));
        }

        return command.Note?.Trim().Length > 1000
            ? Task.FromResult(Result<RegistrationRequest>.Failure(new AppError(
                AppErrorKind.Validation,
                "ملاحظة الرفض لا تتجاوز 1000 حرف.")))
            : repository.RejectAsync(command, cancellationToken);
    }
}

public sealed class CancelRegistrationRequestUseCase
{

    private readonly IRegistrationRequestRepository repository;


    public CancelRegistrationRequestUseCase(

        IRegistrationRequestRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result> ExecuteAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default) =>
        registrationId == Guid.Empty
            ? Task.FromResult(Result.Failure(RegistrationRequestValidationErrors.InvalidRegistration()))
            : repository.CancelAsync(registrationId, cancellationToken);
}

public sealed class RequestRegistrationCompletionUseCase
{

    private readonly IRegistrationRequestRepository repository;


    public RequestRegistrationCompletionUseCase(

        IRegistrationRequestRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<RegistrationRequest>> ExecuteAsync(
        RequestRegistrationCompletionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.RegistrationId == Guid.Empty)
        {
            return Task.FromResult(Result<RegistrationRequest>.Failure(RegistrationRequestValidationErrors.InvalidRegistration()));
        }
        if (command.RequiredFields is null || command.RequiredFields.Count == 0 ||
            command.RequiredFields.Any(string.IsNullOrWhiteSpace))
        {
            return Task.FromResult(Result<RegistrationRequest>.Failure(new AppError(
                AppErrorKind.Validation,
                "أضف حقلاً واحداً على الأقل لطلب الاستكمال.")));
        }
        if (command.Note?.Trim().Length > 1000)
        {
            return Task.FromResult(Result<RegistrationRequest>.Failure(new AppError(
                AppErrorKind.Validation,
                "ملاحظة طلب الاستكمال لا تتجاوز 1000 حرف.")));
        }

        return repository.RequestCompletionAsync(command, cancellationToken);
    }
}

internal static class RegistrationRequestValidationErrors
{
    public static AppError InvalidRegistration() => new(
        AppErrorKind.Validation,
        "معرّف طلب التسجيل غير صالح.");

    public static AppError InvalidHalaqaOrPage() => new(
        AppErrorKind.Validation,
        "معرّف الحلقة أو رقم الصفحة غير صالح.");

    public static AppError InvalidPage() => new(
        AppErrorKind.Validation,
        "رقم الصفحة غير صالح.");
}
