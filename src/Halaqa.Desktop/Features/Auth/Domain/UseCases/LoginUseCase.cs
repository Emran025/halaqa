using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Features.Auth.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Auth.Domain.UseCases;

public sealed class LoginUseCase
{

    private readonly IAuthRepository repository;


    public LoginUseCase(

        IAuthRepository repository

    )

    {

        this.repository = repository;

    }

    public Task<Result<AuthenticatedUser>> ExecuteAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(Result<AuthenticatedUser>.Failure(
                AppError.Validation("أدخل البريد الإلكتروني وكلمة المرور.", Array.Empty<FieldError>())));
        }

        return repository.LoginAsync(email.Trim(), password, cancellationToken);
    }
}
