using Halaqa.Desktop.Features.Halaqas.Domain.Entities;
using Halaqa.Desktop.Features.Halaqas.Domain.Repositories;
using Halaqa.Desktop.Features.Halaqas.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.Halaqas.Domain;

public sealed class HalaqaUseCaseTests
{
    [Fact]
    public async Task Create_RejectsZeroMaximumStudentsBeforeCallingRepository()
    {
        var repository = new FakeHalaqaRepository();
        var command = new CreateHalaqaCommand(
            "حلقة اختبار",
            null,
            HalaqaGender.Male,
            "السعودية",
            "الرياض",
            0,
            "UTC",
            HalaqaStatus.Active);

        var result = await new CreateHalaqaUseCase(repository).ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.CreatedCommand);
    }

    [Fact]
    public async Task Create_ForwardsValidCommandToRepository()
    {
        var repository = new FakeHalaqaRepository();
        var command = new CreateHalaqaCommand(
            "حلقة اختبار",
            null,
            HalaqaGender.Female,
            "السعودية",
            "جدة",
            25,
            "Asia/Riyadh",
            HalaqaStatus.Active);

        var result = await new CreateHalaqaUseCase(repository).ExecuteAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Same(command, repository.CreatedCommand);
        Assert.Equal("حلقة اختبار", result.Value?.Name);
    }

    [Fact]
    public async Task Activate_RejectsEmptyIdentifierBeforeCallingRepository()
    {
        var repository = new FakeHalaqaRepository();

        var result = await new ActivateHalaqaUseCase(repository).ExecuteAsync(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.ActivatedId);
    }

    private sealed class FakeHalaqaRepository : IHalaqaRepository
    {
        public CreateHalaqaCommand? CreatedCommand { get; private set; }
        public Guid? ActivatedId { get; private set; }

        public Task<Result<HalaqaPage>> ListAsync(int page = 1, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<HalaqaPage>.Success(new HalaqaPage(Array.Empty<HalaqaItem>(), 1, 1, 20, 0)));

        public Task<Result<HalaqaItem>> CreateAsync(CreateHalaqaCommand command, CancellationToken cancellationToken = default)
        {
            CreatedCommand = command;
            return Task.FromResult(Result<HalaqaItem>.Success(CreateHalaqa()));
        }

        public Task<Result<HalaqaItem>> UpdateAsync(UpdateHalaqaCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<HalaqaItem>.Success(CreateHalaqa()));

        public Task<Result<HalaqaItem>> ActivateAsync(Guid halaqaId, CancellationToken cancellationToken = default)
        {
            ActivatedId = halaqaId;
            return Task.FromResult(Result<HalaqaItem>.Success(CreateHalaqa()));
        }

        public Task<Result<HalaqaItem>> DeactivateAsync(Guid halaqaId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<HalaqaItem>.Success(CreateHalaqa()));

        private static HalaqaItem CreateHalaqa() => new(
            Guid.NewGuid(),
            new HalaqaTeacher(Guid.NewGuid(), "معلم", "T-1", HalaqaGender.Male, "السعودية", "الرياض", "بكالوريوس", 5, true),
            "حلقة اختبار",
            null,
            HalaqaStatus.Active,
            0,
            25,
            25,
            HalaqaGender.Male,
            "السعودية",
            "الرياض",
            "UTC",
            null,
            null);
    }
}
