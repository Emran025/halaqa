using Halaqa.Desktop.Features.TeacherDocuments.Domain.Entities;
using Halaqa.Desktop.Features.TeacherDocuments.Domain.Repositories;
using Halaqa.Desktop.Features.TeacherDocuments.Domain.UseCases;
using Halaqa.Desktop.Shared.Domain.Common;
using Xunit;

namespace Halaqa.Desktop.Tests.Features.TeacherDocuments.Domain;

public sealed class TeacherDocumentUseCaseTests
{
    [Fact]
    public async Task Create_RejectsBlankCertificateTypeBeforeCallingRepository()
    {
        var repository = new FakeTeacherDocumentRepository();
        var command = new CreateTeacherDocumentCommand("إجازة", " ", null, null, null, null, null);

        var result = await new CreateTeacherDocumentUseCase(repository).ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.LastCreated);
    }

    [Fact]
    public async Task Create_ForwardsValidUploadToRepository()
    {
        var repository = new FakeTeacherDocumentRepository();
        var file = new TeacherDocumentFile("certificate.pdf", "application/pdf", new byte[] { 1, 2 });
        var command = new CreateTeacherDocumentCommand(
            "إجازة حفص",
            "إجازة",
            null,
            "حفص",
            "الرياض",
            new DateOnly(2020, 1, 1),
            file);

        var result = await new CreateTeacherDocumentUseCase(repository).ExecuteAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Same(command, repository.LastCreated);
        Assert.Equal("certificate.pdf", repository.LastCreated?.File?.FileName);
    }

    [Fact]
    public async Task Delete_RejectsNonPositiveIdentifierBeforeCallingRepository()
    {
        var repository = new FakeTeacherDocumentRepository();

        var result = await new DeleteTeacherDocumentUseCase(repository).ExecuteAsync(0);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorKind.Validation, result.Error?.Kind);
        Assert.Null(repository.LastDeletedId);
    }

    private sealed class FakeTeacherDocumentRepository : ITeacherDocumentRepository
    {
        public CreateTeacherDocumentCommand? LastCreated { get; private set; }
        public int? LastDeletedId { get; private set; }

        public Task<Result<TeacherDocumentPage>> ListAsync(int page = 1, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<TeacherDocumentPage>.Success(new TeacherDocumentPage(Array.Empty<TeacherDocument>(), 1, 1, 20, 0)));

        public Task<Result<TeacherDocument>> CreateAsync(
            CreateTeacherDocumentCommand command,
            CancellationToken cancellationToken = default)
        {
            LastCreated = command;
            return Task.FromResult(Result<TeacherDocument>.Success(new TeacherDocument(
                1,
                command.Name,
                command.CertificateType,
                command.CertificateTypeOther,
                command.Riwayah,
                command.IssuingPlace,
                command.IssuingDate,
                null,
                command.File is not null)));
        }

        public Task<Result> DeleteAsync(int documentId, CancellationToken cancellationToken = default)
        {
            LastDeletedId = documentId;
            return Task.FromResult(Result.Success());
        }
    }
}
