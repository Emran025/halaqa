using Halaqa.Desktop.Features.Auth.Domain.Entities;
using Halaqa.Desktop.Presentation;
using Xunit;

namespace Halaqa.Desktop.Tests.Presentation;

public sealed class DashboardViewModelTests
{
    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Teacher)]
    public void OpenGeneralProfile_RaisesGeneralProfileEventForEachRole(UserRole role)
    {
        var viewModel = new DashboardViewModel(new AuthUser(
            Guid.NewGuid(),
            role,
            "مستخدم اختبار",
            "user@example.test",
            "active"));
        var raised = false;
        viewModel.ProfileRequested += (_, _) => raised = true;

        viewModel.OpenGeneralProfileCommand.Execute(null);

        Assert.True(raised);
    }

    [Fact]
    public void OpenProfile_KeepsStudentSpecializedProfileRoute()
    {
        var viewModel = new DashboardViewModel(new AuthUser(
            Guid.NewGuid(),
            UserRole.Student,
            "طالب اختبار",
            "student@example.test",
            "active"));
        var generalRaised = false;
        var studentRaised = false;
        viewModel.ProfileRequested += (_, _) => generalRaised = true;
        viewModel.StudentProfileRequested += (_, _) => studentRaised = true;

        viewModel.OpenProfileCommand.Execute(null);

        Assert.False(generalRaised);
        Assert.True(studentRaised);
    }
}
