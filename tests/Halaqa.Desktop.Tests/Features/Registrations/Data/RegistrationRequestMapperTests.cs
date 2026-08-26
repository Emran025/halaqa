using System.Text.Json;
using Halaqa.Desktop.Features.Registrations.Data.Mappers;
using Halaqa.Desktop.Features.Registrations.Data.Models;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Registrations.Data;

public sealed class RegistrationRequestMapperTests
{
    [Fact]
    public void ToDto_SerializesDecisionAndCompletionUsingContractNames()
    {
        var rejection = RegistrationRequestMapper.ToDto(new RejectRegistrationRequestCommand(
            Guid.NewGuid(),
            "  لا يتوفر مقعد حالياً  "));
        var completion = RegistrationRequestMapper.ToDto(new RequestRegistrationCompletionCommand(
            Guid.NewGuid(),
            [" phone ", "country", "phone"],
            "  يرجى الاستكمال  "));

        var rejectionJson = JsonSerializer.Serialize(rejection, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var completionJson = JsonSerializer.Serialize(completion, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        using var rejectionDocument = JsonDocument.Parse(rejectionJson);
        using var completionDocument = JsonDocument.Parse(completionJson);
        Assert.Equal("لا يتوفر مقعد حالياً", rejectionDocument.RootElement.GetProperty("note").GetString());
        Assert.Equal("يرجى الاستكمال", completionDocument.RootElement.GetProperty("note").GetString());
        Assert.Equal(new[] { "phone", "country" }, completionDocument.RootElement.GetProperty("required_fields").EnumerateArray().Select(item => item.GetString()).ToArray());
    }

    [Fact]
    public void ToDomain_MapsPublicApplicantCollectionWithoutPrivateProfileFields()
    {
        var response = new RegistrationCollectionResponseDto(
        [
            CreateValidRequest()
        ],
        new RegistrationPaginationMetaDto(1, 1, 20, 1));

        var result = RegistrationRequestMapper.ToDomain(response);

        Assert.True(result.IsSuccess);
        var request = Assert.Single(result.Value!.Requests);
        Assert.Equal("طالب اختبار", request.Applicant.DisplayName);
        Assert.True(request.Applicant.SensitiveFieldsHidden);
        Assert.Equal(RegistrationState.Pending, request.State);
    }

    [Fact]
    public void ToDomain_MapsTeacherApplicantInboxWrapper()
    {
        var response = new ApplicantCollectionResponseDto(
        [
            CreateValidRequest()
        ],
        new RegistrationPaginationMetaDto(1, 2, 20, 21));

        var result = RegistrationRequestMapper.ToDomain(response);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Requests);
        Assert.Equal(2, result.Value.LastPage);
        Assert.Equal(21, result.Value.Total);
    }

    [Fact]
    public void ToDomain_RejectsApplicantWhenServerDoesNotConfirmSensitiveFieldsAreHidden()
    {
        var request = CreateValidRequest(sensitiveFieldsHidden: false);

        var result = RegistrationRequestMapper.ToDomain(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Unknown, result.Error?.Kind);
    }

    private static RegistrationRequestDto CreateValidRequest(bool sensitiveFieldsHidden = true) => new(
        Guid.NewGuid(),
        new RegistrationApplicantDto(
            Guid.NewGuid(),
            "طالب اختبار",
            null,
            "pending",
            DateTimeOffset.Parse("2026-08-25T09:00:00Z"),
            sensitiveFieldsHidden),
        "pending",
        "public_summary",
        "أرغب بالانضمام",
        null,
        null,
        DateTimeOffset.Parse("2026-08-25T09:00:00Z"));
}
