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
    public event EventHandler? TeacherDocumentsRequested;
    public event EventHandler? HalaqasRequested;
    public event EventHandler? TeacherApplicationsRequested;
    public event EventHandler? StudentRegistrationsRequested;
    public event EventHandler? StudentRequestsRequested;
    public event EventHandler? FollowUpRequested;
    public event EventHandler? ProgressRequested;
    public event EventHandler? QuranReaderRequested;
    public event EventHandler? NotificationsRequested;
    public event EventHandler? SessionsRequested;
    public event EventHandler? PasswordChangeRequested;

    public string UserName { get; }
    public string RoleLabel { get; }
    public bool IsStudent => _isStudent;
    public bool IsTeacher => !_isStudent;
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
    private void OpenHalaqas() => HalaqasRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenTeacherApplications() => TeacherApplicationsRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenTeacherDocuments() => TeacherDocumentsRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenStudentTeacherDirectory() => StudentRegistrationsRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenStudentRequests() => StudentRequestsRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenNotifications() => NotificationsRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenQuranReader() => QuranReaderRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenFollowUp() => FollowUpRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenProgress() => ProgressRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenSessions() => SessionsRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenPasswordChange() => PasswordChangeRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenGeneralProfile() => ProfileRequested?.Invoke(this, EventArgs.Empty);

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
