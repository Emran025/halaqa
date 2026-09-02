using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Halaqas.Domain.UseCases;

public sealed class ListHalaqasUseCase
{

    private readonly IHalaqaRepository repository;


    public ListHalaqasUseCase(

        IHalaqaRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<HalaqaPage>> ExecuteAsync(int page = 1, CancellationToken cancellationToken = default) =>
        page < 1
            ? Task.FromResult(Result<HalaqaPage>.Failure(new AppError(AppErrorKind.Validation, "رقم الصفحة غير صالح.")))
            : repository.ListAsync(page, cancellationToken);
}

public sealed class CreateHalaqaUseCase
{

    private readonly IHalaqaRepository repository;


    public CreateHalaqaUseCase(

        IHalaqaRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<HalaqaItem>> ExecuteAsync(CreateHalaqaCommand command, CancellationToken cancellationToken = default) =>
        HalaqaCommandValidation.Validate(command.Name, command.Description, command.Country, command.Residence, command.MaxStudents, command.Timezone) is { } error
            ? Task.FromResult(Result<HalaqaItem>.Failure(error))
            : repository.CreateAsync(command, cancellationToken);
}

public sealed class UpdateHalaqaUseCase
{

    private readonly IHalaqaRepository repository;


    public UpdateHalaqaUseCase(

        IHalaqaRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<HalaqaItem>> ExecuteAsync(UpdateHalaqaCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
        {
            return Task.FromResult(Result<HalaqaItem>.Failure(new AppError(AppErrorKind.Validation, "معرّف الحلقة غير صالح.")));
        }

        return HalaqaCommandValidation.Validate(command.Name, command.Description, command.Country, command.Residence, command.MaxStudents, command.Timezone) is { } error
            ? Task.FromResult(Result<HalaqaItem>.Failure(error))
            : repository.UpdateAsync(command, cancellationToken);
    }
}

public sealed class ActivateHalaqaUseCase
{

    private readonly IHalaqaRepository repository;


    public ActivateHalaqaUseCase(

        IHalaqaRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<HalaqaItem>> ExecuteAsync(Guid halaqaId, CancellationToken cancellationToken = default) =>
        halaqaId == Guid.Empty
            ? Task.FromResult(Result<HalaqaItem>.Failure(new AppError(AppErrorKind.Validation, "معرّف الحلقة غير صالح.")))
            : repository.ActivateAsync(halaqaId, cancellationToken);
}

public sealed class DeactivateHalaqaUseCase
{

    private readonly IHalaqaRepository repository;


    public DeactivateHalaqaUseCase(

        IHalaqaRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<HalaqaItem>> ExecuteAsync(Guid halaqaId, CancellationToken cancellationToken = default) =>
        halaqaId == Guid.Empty
            ? Task.FromResult(Result<HalaqaItem>.Failure(new AppError(AppErrorKind.Validation, "معرّف الحلقة غير صالح.")))
            : repository.DeactivateAsync(halaqaId, cancellationToken);
}

internal static class HalaqaCommandValidation
{
    public static AppError? Validate(
        string name,
        string? description,
        string country,
        string residence,
        int? maxStudents,
        string timezone)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length is < 2 or > 150)
        {
            return new AppError(AppErrorKind.Validation, "اسم الحلقة يجب أن يتكون من حرفين إلى 150 حرفاً.");
        }
        if (description?.Trim().Length > 1000)
        {
            return new AppError(AppErrorKind.Validation, "وصف الحلقة لا يتجاوز 1000 حرف.");
        }
        if (string.IsNullOrWhiteSpace(country) || country.Trim().Length is < 2 or > 100)
        {
            return new AppError(AppErrorKind.Validation, "الدولة يجب أن تتكون من حرفين إلى 100 حرف.");
        }
        if (string.IsNullOrWhiteSpace(residence) || residence.Trim().Length > 200)
        {
            return new AppError(AppErrorKind.Validation, "مقر الحلقة مطلوب ولا يتجاوز 200 حرف.");
        }
        if (maxStudents is <= 0)
        {
            return new AppError(AppErrorKind.Validation, "الحد الأقصى للطلاب يجب أن يكون رقماً موجباً.");
        }
        if (string.IsNullOrWhiteSpace(timezone) || timezone.Trim().Length > 64)
        {
            return new AppError(AppErrorKind.Validation, "المنطقة الزمنية مطلوبة ولا تتجاوز 64 حرفاً.");
        }

        return null;
    }
}
