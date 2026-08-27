using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Sessions.Domain.UseCases;

public sealed class ListSessionsUseCase
{
    private readonly ISessionDirectoryRepository repository;

    public ListSessionsUseCase(ISessionDirectoryRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<SessionPage>> ExecuteAsync(SessionQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Page < 1 || query.PerPage is < 1 or > 100)
        {
            return Task.FromResult(Result<SessionPage>.Failure(new AppError(AppErrorKind.Validation, "قيم الترقيم غير صالحة.")));
        }
        if (query.From is { } from && query.To is { } to && to < from)
        {
            return Task.FromResult(Result<SessionPage>.Failure(new AppError(AppErrorKind.Validation, "لا يمكن أن يسبق تاريخ النهاية تاريخ البداية.")));
        }

        return repository.ListAsync(query, cancellationToken);
    }
}
