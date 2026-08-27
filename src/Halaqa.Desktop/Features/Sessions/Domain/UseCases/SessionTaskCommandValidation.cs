using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.UseCases;

internal static class SessionTaskCommandValidation
{
    public static AppError? Validate(CreateSessionTaskCommand command)
    {
        if (command.SessionId == Guid.Empty || command.ClientOperationId == Guid.Empty)
        {
            return Invalid("client_operation_id", "تعذر تجهيز طلب المهمة.");
        }

        return ValidateValues(
            command.SequenceNo,
            command.PlannedAmount,
            command.PlannedFromUnitId,
            command.PlannedToUnitId,
            command.StartPage,
            command.StartAyahId,
            command.EndPage,
            command.EndAyahId,
            null,
            null,
            null);
    }

    public static AppError? Validate(SaveSessionTaskDraftCommand command)
    {
        if (command.SessionId == Guid.Empty || command.TaskId == Guid.Empty || command.ClientOperationId == Guid.Empty)
        {
            return Invalid("client_operation_id", "تعذر تجهيز حفظ مسودة المهمة.");
        }
        if (!IsPage(command.CurrentPage))
        {
            return Invalid("current_page", "رقم صفحة المصحف يجب أن يكون بين 1 و604.");
        }
        if (!IsAyah(command.CurrentAyahId))
        {
            return Invalid("current_ayah_id", "رقم الآية يجب أن يكون بين 1 و6236.");
        }

        return null;
    }

    public static AppError? Validate(UpdateSessionTaskCommand command)
    {
        if (command.SessionId == Guid.Empty || command.TaskId == Guid.Empty)
        {
            return Invalid("task_id", "معرّف الجلسة أو المهمة غير صالح.");
        }
        if (!command.HasChanges)
        {
            return Invalid("request", "يجب تعديل حقل واحد على الأقل في المهمة.");
        }

        return ValidateValues(
            null,
            command.PlannedAmount,
            command.PlannedFromUnitId,
            command.PlannedToUnitId,
            command.StartPage,
            command.StartAyahId,
            command.EndPage,
            command.EndAyahId,
            command.CurrentPage,
            command.CurrentAyahId,
            command.ActualAmount);
    }

    private static AppError? ValidateValues(
        int? sequenceNo,
        decimal? plannedAmount,
        int? plannedFromUnitId,
        int? plannedToUnitId,
        int? startPage,
        int? startAyahId,
        int? endPage,
        int? endAyahId,
        int? currentPage,
        int? currentAyahId,
        decimal? actualAmount)
    {
        if (sequenceNo is <= 0)
        {
            return Invalid("sequence_no", "رقم تسلسل المهمة يجب أن يبدأ من 1.");
        }
        if (plannedAmount is < 0)
        {
            return Invalid("planned_amount", "الكمية المخططة لا يمكن أن تكون سالبة.");
        }
        if (actualAmount is < 0)
        {
            return Invalid("actual_amount", "الكمية الفعلية لا يمكن أن تكون سالبة.");
        }
        if (plannedFromUnitId is <= 0 || plannedToUnitId is <= 0)
        {
            return Invalid("planned_from_unit_id", "معرّف وحدة النطاق يجب أن يبدأ من 1.");
        }
        if (!IsPage(startPage) || !IsPage(endPage) || !IsPage(currentPage))
        {
            return Invalid("start_page", "رقم صفحة المصحف يجب أن يكون بين 1 و604.");
        }
        if (!IsAyah(startAyahId) || !IsAyah(endAyahId) || !IsAyah(currentAyahId))
        {
            return Invalid("start_ayah_id", "رقم الآية يجب أن يكون بين 1 و6236.");
        }

        return null;
    }

    private static bool IsPage(int? value) => !value.HasValue || value is >= 1 and <= 604;

    private static bool IsAyah(int? value) => !value.HasValue || value is >= 1 and <= 6236;

    private static AppError Invalid(string field, string message) =>
        AppError.Validation(message, new[] { new FieldError(field, new[] { message }) });
}
