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
    public event EventHandler? TeacherApplicationsRequested;
    public event EventHandler? StudentRegistrationsRequested;
    public event EventHandler? FollowUpRequested;
    public event EventHandler? ProgressRequested;
    public event EventHandler? QuranReaderRequested;
    public event EventHandler? NotificationsRequested;
    public event EventHandler? SessionsRequested;
    public event EventHandler? PasswordChangeRequested;

    public string UserName { get; }
    public string RoleLabel { get; }
    public bool IsStudent => _isStudent;
    public string PrimaryActionTitle { get; }
    public string PrimaryActionDescription { get; }

    public IReadOnlyList<string> SiteGalleryImages { get; } = new[]
    {
        "/Assets/Images/TagAlwaqar/Site/IMG_20250913_065259.jpg",
        "/Assets/Images/TagAlwaqar/Site/a1ad54c8e6151bcb749cbda9c25d7713.jpg",
        "/Assets/Images/TagAlwaqar/Site/blog/blog-post-1.jpg",
        "/Assets/Images/TagAlwaqar/Site/blog/blog-post-2.jpg",
        "/Assets/Images/TagAlwaqar/Site/blog/blog-post-3.jpg",
        "/Assets/Images/TagAlwaqar/Site/blog/blog-post-5.png",
        "/Assets/Images/TagAlwaqar/Site/blog/featured-post.jpg",
        "/Assets/Images/TagAlwaqar/Site/blog/management.jpg",
        "/Assets/Images/TagAlwaqar/Site/blog/professional.jpg",
        "/Assets/Images/TagAlwaqar/Site/blog/quran.jpg",
        "/Assets/Images/TagAlwaqar/Site/blog/student-life.jpg",
        "/Assets/Images/TagAlwaqar/Site/blog/technology.jpg",
        "/Assets/Images/TagAlwaqar/Site/certificate/certificate_02.jpg",
        "/Assets/Images/TagAlwaqar/Site/certificate/certificate_03.jpg",
        "/Assets/Images/TagAlwaqar/Site/certificate/certificate_04.jpg",
        "/Assets/Images/TagAlwaqar/Site/certificate/certificate_1.jpg",
        "/Assets/Images/TagAlwaqar/Site/coreValues/approach.png",
        "/Assets/Images/TagAlwaqar/Site/coreValues/efficiency.png",
        "/Assets/Images/TagAlwaqar/Site/coreValues/feedbacks.png",
        "/Assets/Images/TagAlwaqar/Site/coreValues/shedule.png",
        "/Assets/Images/TagAlwaqar/Site/iconElements/goal.png",
        "/Assets/Images/TagAlwaqar/Site/icons/teacher.png",
        "/Assets/Images/TagAlwaqar/Site/illustrations/022bea317ccc54d7da73fddef12c0d97.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/40394969-the-holy-quran.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/about-us.jpeg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/best-surah-for-health-.jpeg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/close-up-holy-book-alquran-green-prayer-rug-islamic-photo-concept_992019-2467.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/close-up-holy-book-alquran-green-prayer-rug-islamic-photo-concept_992019-2684.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/hero.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/high-angle-view-book-table_1048944-27339446.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/high-angle-view-man-holding-book_1048944-30050374.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/high-angle-view-of-koran-with-rehal-on-hardwood-floor-760320451-5af3903dff1b7800205c9147.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/islam-religious-text-quran-consists-260nw-2590072893.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/muslim-man-praying-reading-quran_570907-56.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/muslim-woman-reading-quran-medium-shot-from-side_570907-62.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/muslim-woman-reading-quran-pointing-verses_570907-61.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/pngtree-holy-quran-opened-front-view-photography-image_605242.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/premium_photo-1677013623482-6d71ca2dc71a.jpeg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/premium_photo-1677013624162-db18bf12f2be.jpeg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/quran-holy-book-muslims-mosque-koran-38732923.jpg",
        "/Assets/Images/TagAlwaqar/Site/illustrations/stock-photo-muslim-man-praying-reading-quran-mosque.jpeg",
        "/Assets/Images/TagAlwaqar/Site/png-clipart-career-development-computer-icons-job-employment-career-coach-blue-logo.png",
        "/Assets/Images/TagAlwaqar/Site/student_3.jpeg",
        "/Assets/Images/TagAlwaqar/Site/students/60cae08d-a744-419f-879b-45138a078af9-1024x1024.jpg",
        "/Assets/Images/TagAlwaqar/Site/students/99a2d3b8b86d85c1633e32d49d0eb548.jpg",
        "/Assets/Images/TagAlwaqar/Site/students/Donate3.jpg",
        "/Assets/Images/TagAlwaqar/Site/students/file-3338-c61da063421d1b6fadf200d5dda9573b.jpg",
        "/Assets/Images/TagAlwaqar/Site/students/images.jpg",
        "/Assets/Images/TagAlwaqar/Site/students/kuran-kursu-zxWy_cover.jpg",
        "/Assets/Images/TagAlwaqar/Site/students/student_01.jpg",
        "/Assets/Images/TagAlwaqar/Site/students/student_2.jpg",
        "/Assets/Images/TagAlwaqar/Site/students/قرآن-2.jpg"
    };

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
    private void OpenTeacherApplications()
    {
        if (!_isStudent)
        {
            TeacherApplicationsRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private void OpenNotifications() => NotificationsRequested?.Invoke(this, EventArgs.Empty);

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
    private void OpenProgress()
    {
        if (_isStudent)
        {
            ProgressRequested?.Invoke(this, EventArgs.Empty);
        }
    }

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
