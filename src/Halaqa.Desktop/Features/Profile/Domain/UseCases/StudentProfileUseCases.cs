using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Profile.Domain.UseCases;

public sealed class GetCurrentStudentProfileUseCase
{

    private readonly IStudentProfileRepository repository;


    public GetCurrentStudentProfileUseCase(

        IStudentProfileRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<StudentProfile>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        repository.GetCurrentAsync(cancellationToken);
}

public sealed class UpdateCurrentStudentProfileUseCase
{

    private readonly IStudentProfileRepository repository;


    public UpdateCurrentStudentProfileUseCase(

        IStudentProfileRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<StudentProfile>> ExecuteAsync(
        UpdateStudentProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(command);
        return validationError is null
            ? repository.UpdateCurrentAsync(command, cancellationToken)
            : Task.FromResult(Result<StudentProfile>.Failure(validationError));
    }

    private static AppError? Validate(UpdateStudentProfileCommand command)
    {
        if (!command.HasChanges)
        {
            return new AppError(AppErrorKind.Validation, "أدخل حقلاً واحداً على الأقل لتحديث الملف التفصيلي.");
        }

        if (command.Name.IsSpecified &&
            (string.IsNullOrWhiteSpace(command.Name.Value) || command.Name.Value.Trim().Length is < 2 or > 120))
        {
            return new AppError(AppErrorKind.Validation, "يجب أن يتكون الاسم من حرفين إلى 120 حرفاً.");
        }

        var stringLengthErrors = new (bool IsSpecified, string? Value, int Maximum, string FieldLabel)[]
        {
            (command.Country.IsSpecified, command.Country.Value, 100, "الدولة"),
            (command.City.IsSpecified, command.City.Value, 100, "المدينة"),
            (command.Residence.IsSpecified, command.Residence.Value, 200, "محل الإقامة"),
            (command.Phone.IsSpecified, command.Phone.Value, 30, "رقم الهاتف"),
            (command.PhoneZone.IsSpecified, command.PhoneZone.Value, 8, "رمز الهاتف"),
            (command.WhatsappPhone.IsSpecified, command.WhatsappPhone.Value, 30, "رقم واتساب"),
            (command.WhatsappZone.IsSpecified, command.WhatsappZone.Value, 8, "رمز واتساب"),
            (command.MemorizationLevel.IsSpecified, command.MemorizationLevel.Value, 120, "مستوى الحفظ"),
            (command.ReviewLevel.IsSpecified, command.ReviewLevel.Value, 120, "مستوى المراجعة"),
            (command.Bio.IsSpecified, command.Bio.Value, 2000, "التعريف المختصر")
        };

        foreach (var field in stringLengthErrors)
        {
            if (field.IsSpecified && field.Value?.Trim().Length > field.Maximum)
            {
                return new AppError(AppErrorKind.Validation, $"يجب ألا يتجاوز {field.FieldLabel} {field.Maximum} حرفاً.");
            }
        }

        if (command.PreviousMemorization.IsSpecified && command.PreviousMemorization.Value is { } previous)
        {
            if (previous.MemorizedJuzCount is < 0 or > 30 ||
                previous.MemorizationLevel?.Length > 120 ||
                previous.ReviewLevel?.Length > 120 ||
                previous.PreviousTeacherNotes?.Length > 2000 ||
                previous.StopReasons?.Length > 2000)
            {
                return new AppError(AppErrorKind.Validation, "بيانات الحفظ السابق غير صالحة.");
            }
        }

        if (command.AttendancePreferences.IsSpecified && command.AttendancePreferences.Value is { } attendance)
        {
            if (string.IsNullOrWhiteSpace(attendance.Timezone) ||
                attendance.WeeklySlots.Count == 0 ||
                attendance.PreferredSessionDurationMinutes is < 10 or > 180 ||
                attendance.WeeklySlots.Any(slot =>
                    slot.DayOfWeek is < 0 or > 6 || slot.From >= slot.To))
            {
                return new AppError(AppErrorKind.Validation, "تفضيلات الحضور غير صالحة.");
            }
        }

        if (command.FollowUpPlan.IsSpecified && command.FollowUpPlan.Value is { } plan)
        {
            if (plan.Details.Count == 0 ||
                plan.StartsOn is { } start && plan.EndsOn is { } end && end < start ||
                plan.Details.Any(detail => detail.Amount <= 0 || detail.Notes?.Length > 500))
            {
                return new AppError(AppErrorKind.Validation, "خطة المتابعة غير صالحة.");
            }
        }

        return null;
    }
}
