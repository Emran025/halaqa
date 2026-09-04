using Halaqa.Desktop.Features.FollowUp.Domain.Entities;
using Halaqa.Desktop.Features.FollowUp.Domain.Repositories;
using Halaqa.Desktop.Features.FollowUp.Domain.UseCases;
using Halaqa.Desktop.Features.FollowUp.Presentation.ViewModels;
using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.Repositories;
using Halaqa.Desktop.Features.Halaqas.Domain.UseCases;
using Halaqa.Desktop.Features.Memberships.Domain.Entities;
using Halaqa.Desktop.Features.Memberships.Domain.Repositories;
using Halaqa.Desktop.Features.Memberships.Domain.UseCases;
using Halaqa.Desktop.Features.Progress.Domain.Entities;
using Halaqa.Desktop.Features.Progress.Domain.Repositories;
using Halaqa.Desktop.Features.Progress.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.FollowUp.Presentation;

public sealed class ComprehensiveTrackingViewModelTests
{
    private static readonly Guid StudentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid HalaqaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task LoadStudents_PopulatesStudentsFromMembershipAndCalculatesExpectedToday()
    {
        var viewModel = CreateViewModel();

        await viewModel.LoadStudentsAsync();

        Assert.NotEmpty(viewModel.DisplayedStudents);
        Assert.Equal(1, viewModel.AllStudentsCount);
        Assert.True(viewModel.ExpectedTodayCount > 0);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public async Task LoadStudents_UsesProgressAndTrackingDataFromRepositories()
    {
        var viewModel = CreateViewModel();

        await viewModel.LoadStudentsAsync();

        var student = Assert.Single(viewModel.DisplayedStudents);
        Assert.Equal(42, student.CurrentMemorizationPage);
        Assert.Equal(84, student.CurrentReviewPage);
        Assert.Equal(126, student.CurrentRecitationPage);
        Assert.Equal(3, student.TotalMistakesRecorded);
        Assert.Equal("ملاحظة حقيقية", student.LastEvaluation);
    }

    [Fact]
    public async Task MarkStudentCompleted_UpdatesRecitationStatusAndCounts()
    {
        var viewModel = CreateViewModel();
        await viewModel.LoadStudentsAsync();

        var firstStudent = viewModel.DisplayedStudents[0];
        var initialCompleted = viewModel.CompletedTodayCount;

        viewModel.MarkStudentCompleted(firstStudent.StudentId);

        Assert.Equal(initialCompleted + 1, viewModel.CompletedTodayCount);
    }

    [Fact]
    public async Task FilterBySearchText_FiltersMatchingStudents()
    {
        var viewModel = CreateViewModel();
        await viewModel.LoadStudentsAsync();

        var targetStudent = viewModel.DisplayedStudents[0];
        viewModel.SearchText = targetStudent.StudentName;

        Assert.Contains(viewModel.DisplayedStudents, s => s.StudentName == targetStudent.StudentName);
    }

    [Fact]
    public async Task StartSardRecitation_TriggersRecitationRequestedWithSard()
    {
        var viewModel = CreateViewModel();
        await viewModel.LoadStudentsAsync();

        var student = viewModel.DisplayedStudents[0];
        string? receivedTaskType = null;
        viewModel.RecitationRequested += (_, args) => receivedTaskType = args.TaskType;

        viewModel.StartSardRecitationCommand.Execute(student);

        Assert.Equal("سرد", receivedTaskType);
    }

    private static ComprehensiveTrackingViewModel CreateViewModel()
    {
        var followUpRepository = new FakeFollowUpRepository();
        return new ComprehensiveTrackingViewModel(
            new ListHalaqasUseCase(new FakeHalaqasRepository()),
            new ListHalaqaMembershipsUseCase(new FakeMembershipsRepository()),
            new GetFollowUpPlanUseCase(followUpRepository),
            new ListStudentTrackingsUseCase(followUpRepository),
            new GetStudentProgressUseCase(new FakeProgressRepository()));
    }

    private sealed class FakeHalaqasRepository : IHalaqaRepository
    {
        public Task<Result<HalaqaPage>> ListAsync(int page = 1, CancellationToken cancellationToken = default)
        {
            var teacher = new HalaqaTeacher(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "المعلم الاختباري",
                "TCH-TEST",
                HalaqaGender.Male,
                "SA",
                "الرياض",
                "إجازة",
                5,
                true);
            var halaqa = new HalaqaItem(
                HalaqaId,
                teacher,
                "الحلقة الفعلية للاختبار",
                null,
                HalaqaStatus.Active,
                1,
                20,
                19,
                HalaqaGender.Male,
                "SA",
                "الرياض",
                "Asia/Riyadh",
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow);
            return Task.FromResult(Result<HalaqaPage>.Success(new HalaqaPage(new[] { halaqa }, 1, 1, 10, 1)));
        }

        public Task<Result<HalaqaItem>> CreateAsync(CreateHalaqaCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<HalaqaItem>> UpdateAsync(UpdateHalaqaCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<HalaqaItem>> ActivateAsync(Guid halaqaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<HalaqaItem>> DeactivateAsync(Guid halaqaId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeMembershipsRepository : IHalaqaMembershipRepository
    {
        public Task<Result<MembershipPage>> ListAsync(Guid halaqaId, string? status = null, int page = 1, int perPage = 30, CancellationToken cancellationToken = default)
        {
            var student = new MembershipStudent(
                StudentId,
                "طالب من قاعدة الاختبار",
                "student@example.test",
                null,
                "active",
                DateTimeOffset.UtcNow.AddDays(-10),
                DateTimeOffset.UtcNow);
            var membership = new HalaqaMembership(
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                HalaqaId,
                student,
                MembershipStatus.Active,
                DateTimeOffset.UtcNow.AddDays(-5));
            return Task.FromResult(Result<MembershipPage>.Success(new MembershipPage(new[] { membership }, 1, 1, 10, 1)));
        }

        public Task<Result<HalaqaMembership>> AssignAsync(AssignStudentToHalaqaCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<HalaqaMembership>> UpdateAsync(UpdateHalaqaMembershipCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> RemoveAsync(Guid halaqaId, Guid membershipId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeFollowUpRepository : IFollowUpRepository
    {
        public Task<Result<FollowUpPlan>> GetPlanAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            var slot = new WeeklyAvailabilitySlot(
                (int)DateTime.Today.DayOfWeek,
                new TimeOnly(18, 30),
                new TimeOnly(19, 30),
                true);
            var plan = new FollowUpPlan(
                Guid.Parse("55555555-5555-5555-5555-555555555555"),
                StudentId,
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                null,
                FollowUpFrequency.Daily,
                "active",
                "Asia/Riyadh",
                Array.Empty<FollowUpPlanDetail>(),
                new AttendancePreferences("Asia/Riyadh", new[] { slot }, 60),
                null,
                null,
                1,
                null,
                null,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow);
            return Task.FromResult(Result<FollowUpPlan>.Success(plan));
        }

        public Task<Result<TrackingPage>> ListTrackingsAsync(Guid studentId, DateOnly? from, DateOnly? to, int page, int perPage, CancellationToken cancellationToken = default)
        {
            var tracking = new TrackingItem(
                Guid.Parse("66666666-6666-6666-6666-666666666666"),
                StudentId,
                HalaqaId,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                AttendanceType.Present,
                "ملاحظة حقيقية",
                null,
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(-1));
            return Task.FromResult(Result<TrackingPage>.Success(new TrackingPage(new[] { tracking }, 1, 1, 1, 1)));
        }

        public Task<Result<FollowUpPlan>> UpdatePlanAsync(UpdateFollowUpPlanCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<AttendancePreferences>> GetAvailabilityAsync(Guid studentId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<AttendancePreferences>> UpdateAvailabilityAsync(UpdateAvailabilityCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<FollowUpItemPage>> ListItemsAsync(FollowUpItemQuery query, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<FollowUpItem>> CompleteItemAsync(Guid itemId, Guid clientOperationId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<FollowUpItem>> SkipItemAsync(Guid itemId, string reason, Guid clientOperationId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<FollowUpItem>> RescheduleItemAsync(RescheduleFollowUpItemCommand command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeProgressRepository : IStudentProgressRepository
    {
        public Task<Result<StudentProgress>> GetAsync(Guid studentId, string? taskType, CancellationToken cancellationToken = default)
        {
            var progress = new StudentProgress(
                StudentId,
                new StudentLastCompletedProgress(
                    new CompletedRecitationRange(1, 42, 100, 43, 101, 1),
                    new CompletedRecitationRange(1, 84, 200, 85, 201, 2),
                    new CompletedRecitationRange(1, 126, 300, 127, 301, 3)),
                new StudentProgressTotals(4, 8, 3, 3, 3, 2));
            return Task.FromResult(Result<StudentProgress>.Success(progress));
        }
    }
}
