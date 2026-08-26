using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.Repositories;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Registrations.Domain;

public sealed class StudentRegistrationUseCaseTests
{
    [Fact]
    public async Task Create_RejectsMissingAttendanceBeforeCallingRepository()
    {
        var repository = new FakeStudentRegistrationRepository();
        var command = CreateValidCommand() with
        {
            AttendancePreferences = new RegistrationAttendancePreferences("Asia/Riyadh", Array.Empty<RegistrationWeeklyAvailabilitySlot>(), 30)
        };

        var result = await new CreateStudentRegistrationRequestUseCase(repository).ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.CreatedCommand);
    }

    [Fact]
    public async Task Create_ForwardsValidDirectedRequestToRepository()
    {
        var repository = new FakeStudentRegistrationRepository();
        var command = CreateValidCommand();

        var result = await new CreateStudentRegistrationRequestUseCase(repository).ExecuteAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal(command.TeacherCode, repository.CreatedCommand?.TeacherCode);
        Assert.Equal(command.ClientOperationId, repository.CreatedCommand?.ClientOperationId);
    }

    [Fact]
    public async Task List_RejectsSearchLongerThanContractLimit()
    {
        var repository = new FakeStudentRegistrationRepository();

        var result = await new ListAvailableTeachersUseCase(repository).ExecuteAsync(search: new string('x', 121));

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.ListSearch);
    }

    private static CreateStudentRegistrationRequestCommand CreateValidCommand() => new(
        "AHMAD-01",
        Guid.NewGuid(),
        null,
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
        Guid.NewGuid());

    private sealed class FakeStudentRegistrationRepository : IStudentRegistrationRepository
    {
        public CreateStudentRegistrationRequestCommand? CreatedCommand { get; private set; }
        public string? ListSearch { get; private set; }

        public Task<Result<AvailableTeacherPage>> ListAvailableTeachersAsync(
            string? code = null,
            string? search = null,
            int page = 1,
            CancellationToken cancellationToken = default)
        {
            ListSearch = search;
            return Task.FromResult(Result<AvailableTeacherPage>.Success(new AvailableTeacherPage(Array.Empty<AvailableTeacher>(), 1, 1, 20, 0)));
        }

        public Task<Result<AvailableTeacher>> GetPublicTeacherAsync(Guid teacherId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<AvailableTeacher>.Success(CreateTeacher()));

        public Task<Result<RegistrationRequest>> CreateAsync(
            CreateStudentRegistrationRequestCommand command,
            CancellationToken cancellationToken = default)
        {
            CreatedCommand = command;
            return Task.FromResult(Result<RegistrationRequest>.Success(new RegistrationRequest(
                Guid.NewGuid(),
                new RegistrationApplicant(Guid.NewGuid(), "طالب", null, RegistrationState.Pending, DateTimeOffset.UtcNow, true),
                RegistrationState.Pending,
                "student_visible",
                null,
                null,
                null,
                DateTimeOffset.UtcNow)));
        }

        private static AvailableTeacher CreateTeacher() => new(
            Guid.NewGuid(), "المعلم", "CODE", null, RegistrationGender.Male, "السعودية", "الرياض", "إجازة", 5, true, null, 0, Array.Empty<PublicHalaqa>());
    }
}
