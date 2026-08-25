using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.Entities;

namespace Halaqa.Desktop.Presentation;

public sealed partial class DashboardViewModel
{
    public DashboardViewModel(AuthUser user)
    {
        UserName = user.Name;
        RoleLabel = user.Role == UserRole.Teacher ? "المعلم" : "الطالب";
        PrimaryActionTitle = user.Role == UserRole.Teacher ? "إدارة الحلقات" : "متابعة خطتي";
        PrimaryActionDescription = user.Role == UserRole.Teacher
            ? "أنشئ حلقة، راجع طلبات الانضمام، ونظّم طلابك."
            : "راجع عناصر الحفظ والمراجعة والتلاوة لليوم.";
    }

    public event EventHandler? ProfileRequested;

    public string UserName { get; }
    public string RoleLabel { get; }
    public string PrimaryActionTitle { get; }
    public string PrimaryActionDescription { get; }

    [RelayCommand]
    private void OpenProfile() => ProfileRequested?.Invoke(this, EventArgs.Empty);
}
