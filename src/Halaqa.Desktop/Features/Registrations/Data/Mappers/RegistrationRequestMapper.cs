using Halaqa.Desktop.Features.Registrations.Data.Models;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Data.Mappers;

internal static class RegistrationRequestMapper
{
    public static Result<RegistrationRequest> ToDomain(RegistrationRequestDto dto)
    {
        if (dto.Id == Guid.Empty || dto.StudentSummary is null || dto.CreatedAt == default ||
            string.IsNullOrWhiteSpace(dto.Visibility) || !TryParseState(dto.State, out var state))
        {
            return Result<RegistrationRequest>.Failure(UnexpectedResponseError());
        }

        var applicant = ToApplicant(dto.StudentSummary);
        if (!applicant.IsSuccess || applicant.Value is null)
        {
            return Result<RegistrationRequest>.Failure(applicant.Error!);
        }

        return Result<RegistrationRequest>.Success(new RegistrationRequest(
            dto.Id,
            applicant.Value,
            state,
            dto.Visibility,
            dto.Message,
            dto.DecisionNote,
            dto.DecidedAt,
            dto.CreatedAt));
    }

    public static Result<RegistrationRequestPage> ToDomain(RegistrationCollectionResponseDto dto)
    {
        if (dto.RegistrationRequests is null || dto.Meta is null ||
            dto.Meta.CurrentPage < 1 || dto.Meta.LastPage < 1 ||
            dto.Meta.PerPage < 1 || dto.Meta.Total < 0)
        {
            return Result<RegistrationRequestPage>.Failure(UnexpectedResponseError());
        }

        var requests = dto.RegistrationRequests.Select(ToDomain).ToArray();
        var error = requests.Select(result => result.Error).FirstOrDefault(value => value is not null);
        if (error is not null)
        {
            return Result<RegistrationRequestPage>.Failure(error);
        }

        return Result<RegistrationRequestPage>.Success(new RegistrationRequestPage(
            requests.Select(result => result.Value!).ToArray(),
            dto.Meta.CurrentPage,
            dto.Meta.LastPage,
            dto.Meta.PerPage,
            dto.Meta.Total));
    }

    public static Result<RegistrationRequestPage> ToDomain(ApplicantCollectionResponseDto dto)
    {
        if (dto.Applicants is null || dto.Meta is null ||
            dto.Meta.CurrentPage < 1 || dto.Meta.LastPage < 1 ||
            dto.Meta.PerPage < 1 || dto.Meta.Total < 0)
        {
            return Result<RegistrationRequestPage>.Failure(UnexpectedResponseError());
        }

        var requests = dto.Applicants.Select(ToDomain).ToArray();
        var error = requests.Select(result => result.Error).FirstOrDefault(value => value is not null);
        if (error is not null)
        {
            return Result<RegistrationRequestPage>.Failure(error);
        }

        return Result<RegistrationRequestPage>.Success(new RegistrationRequestPage(
            requests.Select(result => result.Value!).ToArray(),
            dto.Meta.CurrentPage,
            dto.Meta.LastPage,
            dto.Meta.PerPage,
            dto.Meta.Total));
    }

    public static DecisionNoteRequestDto ToDto(RejectRegistrationRequestCommand command) =>
        new(NormalizeOptional(command.Note));

    public static CompletionRequestDto ToDto(RequestRegistrationCompletionCommand command) => new(
        command.RequiredFields.Select(value => value.Trim()).Distinct(StringComparer.Ordinal).ToArray(),
        NormalizeOptional(command.Note));

    public static string ToContractValue(RegistrationState state) => state switch
    {
        RegistrationState.Pending => "pending",
        RegistrationState.Accepted => "accepted",
        RegistrationState.Rejected => "rejected",
        RegistrationState.CompletionRequested => "completion_requested",
        RegistrationState.Withdrawn => "withdrawn",
        RegistrationState.Cancelled => "cancelled",
        _ => throw new ArgumentOutOfRangeException(nameof(state))
    };

    private static Result<RegistrationApplicant> ToApplicant(RegistrationApplicantDto dto)
    {
        if (dto.Id == Guid.Empty || string.IsNullOrWhiteSpace(dto.DisplayName) ||
            dto.SubmittedAt == default || !dto.SensitiveFieldsHidden ||
            !TryParseState(dto.Status, out var status))
        {
            return Result<RegistrationApplicant>.Failure(UnexpectedResponseError());
        }

        return Result<RegistrationApplicant>.Success(new RegistrationApplicant(
            dto.Id,
            dto.DisplayName,
            dto.Avatar,
            status,
            dto.SubmittedAt,
            dto.SensitiveFieldsHidden));
    }

    private static bool TryParseState(string? value, out RegistrationState state)
    {
        state = value switch
        {
            "pending" => RegistrationState.Pending,
            "accepted" => RegistrationState.Accepted,
            "rejected" => RegistrationState.Rejected,
            "completion_requested" => RegistrationState.CompletionRequested,
            "withdrawn" => RegistrationState.Withdrawn,
            "cancelled" => RegistrationState.Cancelled,
            _ => default
        };
        return value is "pending" or "accepted" or "rejected" or "completion_requested" or "withdrawn" or "cancelled";
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AppError UnexpectedResponseError() => new(
        AppErrorKind.Unknown,
        "أعاد الخادم بيانات طلب تسجيل بصورة غير متوقعة.");
}
