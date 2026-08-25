using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Domain.UseCases;

public sealed class GetCurrentTeacherProfileUseCase(ITeacherProfileRepository repository)
{
    public Task<Result<TeacherProfile>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        repository.GetCurrentAsync(cancellationToken);
}

public sealed class UpdateCurrentTeacherProfileUseCase(ITeacherProfileRepository repository)
{
    public Task<Result<TeacherProfile>> ExecuteAsync(
        UpdateTeacherProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(command);
        return validationError is null
            ? repository.UpdateCurrentAsync(command, cancellationToken)
            : Task.FromResult(Result<TeacherProfile>.Failure(validationError));
    }

    private static AppError? Validate(UpdateTeacherProfileCommand command)
    {
        if (!command.HasChanges)
        {
            return new AppError(AppErrorKind.Validation, "أدخل حقلاً واحداً على الأقل لتحديث ملف المعلم.");
        }

        if (command.Name.IsSpecified &&
            (string.IsNullOrWhiteSpace(command.Name.Value) || command.Name.Value.Trim().Length is < 2 or > 120))
        {
            return new AppError(AppErrorKind.Validation, "يجب أن يتكون الاسم من حرفين إلى 120 حرفاً.");
        }

        var fields = new (bool IsSpecified, string? Value, int Maximum, string Label)[]
        {
            (command.Country.IsSpecified, command.Country.Value, 100, "الدولة"),
            (command.City.IsSpecified, command.City.Value, 100, "المدينة"),
            (command.Residence.IsSpecified, command.Residence.Value, 200, "محل الإقامة"),
            (command.Phone.IsSpecified, command.Phone.Value, 30, "رقم الهاتف"),
            (command.PhoneZone.IsSpecified, command.PhoneZone.Value, 8, "رمز الهاتف"),
            (command.WhatsappPhone.IsSpecified, command.WhatsappPhone.Value, 30, "رقم واتساب"),
            (command.WhatsappZone.IsSpecified, command.WhatsappZone.Value, 8, "رمز واتساب"),
            (command.Qualification.IsSpecified, command.Qualification.Value, 250, "المؤهل"),
            (command.Bio.IsSpecified, command.Bio.Value, 2000, "التعريف المختصر")
        };

        foreach (var field in fields)
        {
            if (field.IsSpecified && field.Value?.Trim().Length > field.Maximum)
            {
                return new AppError(AppErrorKind.Validation, $"يجب ألا يتجاوز {field.Label} {field.Maximum} حرفاً.");
            }
        }

        if (command.ExperienceYears.IsSpecified && command.ExperienceYears.Value is < 0 or > 80)
        {
            return new AppError(AppErrorKind.Validation, "سنوات الخبرة يجب أن تكون بين 0 و80.");
        }

        if (command.MaxHalaqas.IsSpecified && command.MaxHalaqas.Value is < 0)
        {
            return new AppError(AppErrorKind.Validation, "الحد الأقصى للحلقات لا يمكن أن يكون سالباً.");
        }

        return null;
    }
}
