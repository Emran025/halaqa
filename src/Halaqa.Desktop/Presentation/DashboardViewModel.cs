using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Auth.Domain.Entities;

namespace Halaqa.Desktop.Presentation;

public sealed partial class DashboardViewModel
{
    private readonly bool _isStudent;

    public DashboardViewModel(AuthUser user)
    {
        _isStudent = user.Role == UserRole.Student;
        UserName = user.Name;
        RoleLabel = user.Role == UserRole.Teacher ? "المعلم" : "الطالب";
        PrimaryActionTitle = user.Role == UserRole.Teacher ? "إدارة الحلقات" : "البحث عن معلم";
        PrimaryActionDescription = user.Role == UserRole.Teacher
            ? "أنشئ حلقة، راجع طلبات الانضمام، ونظّم طلابك."
            : "تصفح المعلمين المتاحين وقدّم طلب تسجيل موجهاً بالبيانات الرسمية من ملفك.";
    }

    public event EventHandler? ProfileRequested;
    public event EventHandler? StudentProfileRequested;
    public event EventHandler? TeacherProfileRequested;
    public event EventHandler? HalaqasRequested;
    public event EventHandler? StudentRegistrationsRequested;
    public event EventHandler? FollowUpRequested;
    public event EventHandler? QuranReaderRequested;

    public string UserName { get; }
    public string RoleLabel { get; }
    public bool IsStudent => _isStudent;
    public string PrimaryActionTitle { get; }
    public string PrimaryActionDescription { get; }

    [RelayCommand]
    private void OpenPrimaryAction()
    {
        if (_isStudent)
        {
            StudentRegistrationsRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        HalaqasRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void OpenQuranReader() => QuranReaderRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenFollowUp()
    {
        if (_isStudent)
        {
            FollowUpRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private void OpenProfile()
    {
        if (_isStudent)
        {
            StudentProfileRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        TeacherProfileRequested?.Invoke(this, EventArgs.Empty);
    }
}
