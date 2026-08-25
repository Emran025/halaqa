using Halaqa.Desktop.Features.Auth.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Auth.Data.Repositories;
using Halaqa.Desktop.Features.Auth.Domain.Repositories;
using Halaqa.Desktop.Features.Auth.Domain.UseCases;
using Halaqa.Desktop.Features.Auth.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Auth;

public static class AuthFeatureModule
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        services.AddSingleton<IAuthRemoteDataSource, AuthRemoteDataSource>();
        services.AddSingleton<IAuthRepository, AuthRepository>();
        services.AddSingleton<LoginUseCase>();
        services.AddSingleton<RegisterStudentUseCase>();
        services.AddSingleton<RegisterTeacherUseCase>();
        services.AddSingleton<RequestPasswordResetUseCase>();
        services.AddSingleton<LoginViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();
        services.AddTransient<StudentRegistrationViewModel>();
        return services;
    }
}
