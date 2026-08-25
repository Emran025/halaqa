using Halaqa.Desktop.Features.Profile.Domain.Entities;
using Halaqa.Desktop.Features.Profile.Domain.Repositories;
using Halaqa.Desktop.Features.Profile.Domain.UseCases;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.Repositories;
using Halaqa.Desktop.Features.Registrations.Domain.UseCases;
using Halaqa.Desktop.Features.Registrations.Presentation.ViewModels;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Registrations.Presentation;

public sealed class StudentTeacherDirectoryViewModelTests
{
    [Fact]
    public async Task Submit_LoadsProfileSnapshotAndSendsDirectedRequest()
    {
        var registrationRepository = new FakeStudentRegistrationRepository();
        var profileRepository = new FakeStudentProfileRepository();
        var viewModel = new StudentTeacherDirectoryViewModel(
            new ListAvailableTeachersUseCase(registrationRepository),
            new CreateStudentRegistrationRequestUseCase(registrationRepository),
            new GetCurrentStudentProfileUseCase(profileRepository));
        viewModel.Initialize();

        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedTeacher = Assert.Single(viewModel.Teachers);
        await viewModel.RefreshProfileCommand.ExecuteAsync(null);
        viewModel.MessageText = "  أرغب بالانضمام  ";
        await viewModel.SubmitCommand.ExecuteAsync(null);

        var command = Assert.IsType<CreateStudentRegistrationRequestCommand>(registrationRepository.CreatedCommand);
        Assert.Equal("AHMAD-01", command.TeacherCode);
        Assert.Equal("أرغب بالانضمام", command.Message?.Trim());
        Assert.Equal("Asia/Riyadh", command.AttendancePreferences.Timezone);
        Assert.Single(command.FollowUpPlan.Details);
        Assert.False(viewModel.IsError);
    }

    private sealed class FakeStudentRegistrationRepository : IStudentRegistrationRepository
    {
        private readonly AvailableTeacher _teacher = new(
            Guid.NewGuid(),
            "المعلم أحمد",
            "AHMAD-01",
            null,
            RegistrationGender.Male,
            "السعودية",
            "الرياض",
            "إجازة في القرآن",
            12,
            true,
            null,
            1,
            [new PublicHalaqa(Guid.NewGuid(), "حلقة الفجر", "active", RegistrationGender.Male, "السعودية", "الرياض", 8)]);

        public CreateStudentRegistrationRequestCommand? CreatedCommand { get; private set; }

        public Task<Result<AvailableTeacherPage>> ListAvailableTeachersAsync(
            string? code = null,
            string? search = null,
            int page = 1,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<AvailableTeacherPage>.Success(new AvailableTeacherPage([_teacher], 1, 1, 20, 1)));

        public Task<Result<AvailableTeacher>> GetPublicTeacherAsync(Guid teacherId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<AvailableTeacher>.Success(_teacher));

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
                command.Message,
                null,
                null,
                DateTimeOffset.UtcNow)));
        }
    }

    private sealed class FakeStudentProfileRepository : IStudentProfileRepository
    {
        private readonly StudentProfile _profile = CreateProfile();

        public Task<Result<StudentProfile>> GetCurrentAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<StudentProfile>.Success(_profile));

        public Task<Result<StudentProfile>> UpdateCurrentAsync(
            UpdateStudentProfileCommand command,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<StudentProfile>.Success(_profile));

        private static StudentProfile CreateProfile()
        {
            var attendance = new StudentAttendancePreferences(
                "Asia/Riyadh",
                [new StudentWeeklyAvailabilitySlot(0, new TimeOnly(18, 0), new TimeOnly(18, 30), true)],
                30);
            var plan = new StudentFollowUpPlan(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                FollowUpFrequency.OnceAWeek,
                "active",
                "Asia/Riyadh",
                [new StudentPlanDetail(
                    Guid.NewGuid(),
                    QuranTaskType.Memorization,
                    QuranPlanUnit.Page,
                    1,
                    null,
                    1,
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow)],
                attendance,
                null,
                null,
                1,
                null,
                null,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow);

            return new StudentProfile(
                Guid.NewGuid(),
                "طالب اختبار",
                "student@example.test",
                "active",
                new DateOnly(2000, 1, 1),
                StudentGender.Male,
                "السعودية",
                "الرياض",
                null,
                "500000000",
                "+966",
                null,
                null,
                null,
                null,
                null,
                attendance,
                plan,
                StudentProfileVisibility.Self);
        }
    }
}
