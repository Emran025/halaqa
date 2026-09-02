using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Domain.UseCases;

public sealed class ListAvailableTeachersUseCase
{

    private readonly IStudentRegistrationRepository repository;


    public ListAvailableTeachersUseCase(

        IStudentRegistrationRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<AvailableTeacherPage>> ExecuteAsync(
        string? code = null,
        string? search = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || code?.Trim().Length > 40 || search?.Trim().Length > 120)
        {
            return Task.FromResult(Result<AvailableTeacherPage>.Failure(new AppError(
                AppErrorKind.Validation,
                "معايير البحث عن المعلم غير صالحة.")));
        }

        return repository.ListAvailableTeachersAsync(
            StudentRegistrationUseCaseText.NormalizeOptional(code),
            StudentRegistrationUseCaseText.NormalizeOptional(search),
            page,
            cancellationToken);
    }
}

public sealed class GetPublicTeacherUseCase
{

    private readonly IStudentRegistrationRepository repository;


    public GetPublicTeacherUseCase(

        IStudentRegistrationRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<AvailableTeacher>> ExecuteAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default) =>
        teacherId == Guid.Empty
            ? Task.FromResult(Result<AvailableTeacher>.Failure(new AppError(
                AppErrorKind.Validation,
                "معرّف المعلم غير صالح.")))
            : repository.GetPublicTeacherAsync(teacherId, cancellationToken);
}

public sealed class CreateStudentRegistrationRequestUseCase
{

    private readonly IStudentRegistrationRepository repository;


    public CreateStudentRegistrationRequestUseCase(

        IStudentRegistrationRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<RegistrationRequest>> ExecuteAsync(
        CreateStudentRegistrationRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var error = Validate(command);
        return error is null
            ? repository.CreateAsync(command, cancellationToken)
            : Task.FromResult(Result<RegistrationRequest>.Failure(error));
    }

    private static AppError? Validate(CreateStudentRegistrationRequestCommand command)
    {
        if (command.ClientOperationId == Guid.Empty || command.Profile is null ||
            command.AttendancePreferences is null || command.FollowUpPlan is null)
        {
            return new AppError(AppErrorKind.Validation, "بيانات طلب التسجيل غير مكتملة.");
        }
        if (command.TeacherCode?.Trim().Length > 40 || command.Message?.Trim().Length > 1000)
        {
            return new AppError(AppErrorKind.Validation, "كود المعلم أو رسالة الطلب أطول من الحد المسموح.");
        }

        var profile = command.Profile;
        if (profile.BirthDate == default || string.IsNullOrWhiteSpace(profile.Country) ||
            string.IsNullOrWhiteSpace(profile.City) || string.IsNullOrWhiteSpace(profile.Phone) ||
            string.IsNullOrWhiteSpace(profile.PhoneZone) || profile.Country.Trim().Length > 100 ||
            profile.City.Trim().Length > 100 || profile.Residence?.Trim().Length > 200 ||
            profile.Phone.Trim().Length > 30 || profile.PhoneZone.Trim().Length > 8 ||
            profile.WhatsappPhone?.Trim().Length > 30 || profile.WhatsappZone?.Trim().Length > 8 ||
            profile.MemorizationLevel?.Trim().Length > 120 || profile.ReviewLevel?.Trim().Length > 120 ||
            profile.Bio?.Trim().Length > 2000)
        {
            return new AppError(AppErrorKind.Validation, "بيانات ملف طلب التسجيل لا تطابق القيود المطلوبة.");
        }

        var attendance = command.AttendancePreferences;
        if (string.IsNullOrWhiteSpace(attendance.Timezone) || attendance.WeeklySlots.Count == 0 ||
            attendance.PreferredSessionDurationMinutes is < 10 or > 180 ||
            attendance.WeeklySlots.Any(slot => slot.DayOfWeek is < 0 or > 6 || slot.From >= slot.To))
        {
            return new AppError(AppErrorKind.Validation, "تفضيلات الحضور في طلب التسجيل غير صالحة.");
        }

        var plan = command.FollowUpPlan;
        if (!IsFrequency(plan.Frequency) || plan.Details.Count == 0 ||
            plan.StartsOn is { } start && plan.EndsOn is { } end && end < start ||
            plan.Details.Any(detail => !IsTaskType(detail.TaskType) || !IsPlanUnit(detail.Unit) ||
                                      detail.Amount <= 0 || detail.Notes?.Trim().Length > 500))
        {
            return new AppError(AppErrorKind.Validation, "خطة المتابعة في طلب التسجيل غير صالحة.");
        }

        var previous = command.PreviousMemorization;
        if (previous is not null && (previous.MemorizedJuzCount is < 0 or > 30 ||
            previous.MemorizationLevel?.Trim().Length > 120 || previous.ReviewLevel?.Trim().Length > 120 ||
            previous.PreviousTeacherNotes?.Trim().Length > 2000 || previous.StopReasons?.Trim().Length > 2000))
        {
            return new AppError(AppErrorKind.Validation, "بيانات الحفظ السابق في طلب التسجيل غير صالحة.");
        }

        return null;
    }

    private static bool IsFrequency(string value) => value is "daily" or "onceAWeek" or "twiceAWeek" or "thriceAWeek";
    private static bool IsTaskType(string value) => value is "memorization" or "review" or "recitation";
    private static bool IsPlanUnit(string value) => value is "juz" or "hizb" or "halfHizb" or "quarterHizb" or "page";
}

internal static class StudentRegistrationUseCaseText
{
    public static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
