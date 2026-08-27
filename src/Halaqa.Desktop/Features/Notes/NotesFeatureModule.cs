using Halaqa.Desktop.Features.Notes.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Notes.Data.Repositories;
using Halaqa.Desktop.Features.Notes.Domain.Repositories;
using Halaqa.Desktop.Features.Notes.Domain.UseCases;
using Halaqa.Desktop.Features.Notes.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop.Features.Notes;

public static class NotesFeatureModule
{
    public static IServiceCollection AddNotesFeature(this IServiceCollection services)
    {
        services.AddSingleton<ITaskNoteRemoteDataSource, TaskNoteRemoteDataSource>();
        services.AddSingleton<ITaskNoteRepository, TaskNoteRepository>();
        services.AddSingleton<ListTaskNotesUseCase>();
        services.AddSingleton<CreateTaskNoteUseCase>();
        services.AddSingleton<UpdateTaskNoteUseCase>();
        services.AddSingleton<DeleteTaskNoteUseCase>();
        services.AddTransient<TaskNotesViewModel>();
        return services;
    }
}
