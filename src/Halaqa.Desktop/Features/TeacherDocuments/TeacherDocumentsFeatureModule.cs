using Halaqa.Desktop.Features.TeacherDocuments.Data.DataSources.Remote;
using Halaqa.Desktop.Features.TeacherDocuments.Data.Repositories;
using Halaqa.Desktop.Features.TeacherDocuments.Domain.Repositories;
using Halaqa.Desktop.Features.TeacherDocuments.Domain.UseCases;
using Halaqa.Desktop.Features.TeacherDocuments.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.TeacherDocuments;

public static class TeacherDocumentsFeatureModule
{
    public static IServiceCollection AddTeacherDocumentsFeature(this IServiceCollection services)
    {
        services.AddSingleton<ITeacherDocumentRemoteDataSource, TeacherDocumentRemoteDataSource>();
        services.AddSingleton<ITeacherDocumentRepository, TeacherDocumentRepository>();
        services.AddSingleton<ListTeacherDocumentsUseCase>();
        services.AddSingleton<CreateTeacherDocumentUseCase>();
        services.AddSingleton<DeleteTeacherDocumentUseCase>();
        services.AddSingleton<TeacherDocumentsViewModel>();
        return services;
    }
}
