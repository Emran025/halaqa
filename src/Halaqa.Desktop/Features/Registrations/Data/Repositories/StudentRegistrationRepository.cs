using Halaqa.Desktop.Features.Registrations.Data.DataSources.Remote;
using Halaqa.Desktop.Features.Registrations.Data.Mappers;
using Halaqa.Desktop.Features.Registrations.Data.Models;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;
using Halaqa.Desktop.Features.Registrations.Domain.Repositories;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Features.Registrations.Data.Repositories;

internal sealed class StudentRegistrationRepository : IStudentRegistrationRepository
{

    private readonly IStudentRegistrationRemoteDataSource remoteDataSource;


    public StudentRegistrationRepository(

        IStudentRegistrationRemoteDataSource remoteDataSource

    )

    {

        this.remoteDataSource = remoteDataSource;

    }

    public async Task<Result<AvailableTeacherPage>> ListAvailableTeachersAsync(
        string? code = null,
        string? search = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.ListAvailableTeachersAsync(code, search, page, cancellationToken);
        return response.IsSuccess && response.Value is not null
            ? StudentRegistrationMapper.ToDomain(response.Value)
            : Result<AvailableTeacherPage>.Failure(response.Error!);
    }

    public async Task<Result<AvailableTeacher>> GetPublicTeacherAsync(
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.GetPublicTeacherAsync(teacherId, cancellationToken);
        return response.IsSuccess && response.Value?.Teacher is not null
            ? StudentRegistrationMapper.ToDomain(response.Value.Teacher)
            : Result<AvailableTeacher>.Failure(response.Error!);
    }

    public async Task<Result<RegistrationRequest>> CreateAsync(
        CreateStudentRegistrationRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await remoteDataSource.CreateAsync(StudentRegistrationMapper.ToDto(command), cancellationToken);
        return response.IsSuccess && response.Value?.RegistrationRequest is not null
            ? RegistrationRequestMapper.ToDomain(response.Value.RegistrationRequest)
            : Result<RegistrationRequest>.Failure(response.Error!);
    }
}
