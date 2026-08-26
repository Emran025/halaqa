using System.Text.Json;
using Halaqa.Desktop.Features.Registrations.Data.Mappers;
using Halaqa.Desktop.Features.Registrations.Data.Models;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Registrations.Data;

public sealed class StudentRegistrationMapperTests
{
    [Fact]
    public void ToDomain_MapsTeacherPublicCardAndPublicHalaqas()
    {
        var dto = CreateTeacherDto();

        var result = StudentRegistrationMapper.ToDomain(dto);

        Assert.True(result.IsSuccess);
        var teacher = result.Value!;
        Assert.Equal("المعلم أحمد", teacher.DisplayName);
        Assert.Equal("AHMAD-01", teacher.TeacherCode);
        var halaqa = Assert.Single(teacher.PublicHalaqas);
        Assert.Equal("حلقة الفجر", halaqa.Name);
        Assert.Equal(8, halaqa.AvailableCapacity);
    }

    [Fact]
    public void ToDto_SerializesDirectedRequestWithContractRootNames()
    {
        var teacherCode = "AHMAD-01";
        var halaqaId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var command = new CreateStudentRegistrationRequestCommand(
            teacherCode,
            halaqaId,
            "  أرغب بالانضمام  ",
            new RegistrationApplicationProfile(
                RegistrationGender.Male,
                new DateOnly(2000, 1, 1),
                "السعودية",
                "الرياض",
                null,
                "500000000",
                "+966",
                null,
                null,
                null,
                null,
                null),
            null,
            new RegistrationAttendancePreferences(
                "Asia/Riyadh",
                new[] {new RegistrationWeeklyAvailabilitySlot(0, new TimeOnly(18, 0), new TimeOnly(18, 30), true)},
                30),
            new RegistrationFollowUpPlan(
                "onceAWeek",
                new[] {new RegistrationPlanDetail("memorization", "page", 1, null)},
                null,
                null),
            operationId);

        var json = JsonSerializer.Serialize(StudentRegistrationMapper.ToDto(command), new JsonSerializerOptions(JsonSerializerDefaults.Web));

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(teacherCode, root.GetProperty("teacher_code").GetString());
        Assert.Equal(halaqaId, root.GetProperty("requested_halaqa_id").GetGuid());
        Assert.Equal("أرغب بالانضمام", root.GetProperty("message").GetString());
        Assert.Equal(operationId, root.GetProperty("client_operation_id").GetGuid());
        Assert.Equal("male", root.GetProperty("profile").GetProperty("gender").GetString());
        Assert.Equal("Asia/Riyadh", root.GetProperty("attendance_preferences").GetProperty("timezone").GetString());
        Assert.Equal("onceAWeek", root.GetProperty("follow_up_plan").GetProperty("frequency").GetString());
    }

    private static AvailableTeacherDto CreateTeacherDto() => new(
        Guid.NewGuid(),
        "المعلم أحمد",
        "AHMAD-01",
        null,
        "male",
        "السعودية",
        "الرياض",
        "إجازة في القرآن",
        12,
        true,
        "معلّم متاح",
        1,
        new[] {new PublicHalaqaDto(
            Guid.NewGuid(),
            "حلقة الفجر",
            "active",
            "male",
            "السعودية",
            "الرياض",
            8)});
}
