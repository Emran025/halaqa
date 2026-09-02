﻿﻿using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Features.Quran.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.Repositories;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;
using Halaqa.Desktop.Features.Sessions.Presentation.Stores;

namespace Halaqa.Desktop.Features.Sessions.Presentation.ViewModels;

public sealed partial class InteractiveQuranWord : ObservableObject
{
    public int WordIndex { get; init; }
    public string Text { get; init; } = string.Empty;
    public int PageNumber { get; init; }
    public int AyahNumber { get; init; }
    public bool IsAyahEndSymbol { get; init; }

    [ObservableProperty] private string? _mistakeType;
    [ObservableProperty] private bool _isStopPoint;
    [ObservableProperty] private Brush _backgroundBrush = Brushes.Transparent;
    [ObservableProperty] private Brush _borderBrush = Brushes.Transparent;

    public bool HasMistake => !string.IsNullOrEmpty(MistakeType);

    public void SetMistake(string? mistakeType)
    {
        MistakeType = mistakeType == "إلغاء" ? null : mistakeType;
        UpdateVisuals();
    }

    public void ToggleStopPoint()
    {
        IsStopPoint = !IsStopPoint;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (IsStopPoint)
        {
            BackgroundBrush = new SolidColorBrush(Color.FromArgb(220, 200, 230, 201)); // Distinct Emerald Stop Marker
            BorderBrush = new SolidColorBrush(Color.FromRgb(46, 125, 50));
            return;
        }

        switch (MistakeType)
        {
            case "حفظ":
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(200, 255, 205, 210)); // Solid Vibrant Red Highlight
                BorderBrush = new SolidColorBrush(Color.FromRgb(211, 47, 47));
                break;
            case "تجويد":
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(200, 255, 224, 178)); // Solid Vibrant Orange Highlight
                BorderBrush = new SolidColorBrush(Color.FromRgb(245, 124, 0));
                break;
            case "تشكيل":
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(200, 255, 245, 157)); // Solid Vibrant Yellow Highlight
                BorderBrush = new SolidColorBrush(Color.FromRgb(251, 192, 45));
                break;
            case "تنبيه":
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(200, 187, 222, 251)); // Solid Vibrant Blue Highlight
                BorderBrush = new SolidColorBrush(Color.FromRgb(25, 118, 210));
                break;
            default:
                BackgroundBrush = Brushes.Transparent;
                BorderBrush = Brushes.Transparent;
                break;
        }
    }
}

public sealed partial class LiveSessionViewModel : ObservableObject
{
    private readonly IPeerMediaConnection _peerMediaConnection;
    private readonly IMushafRealtimeChannel _mushafRealtimeChannel;
    private readonly ILocalVideoRecorder _localVideoRecorder;
    private readonly SaveOfficialMushafStateUseCase _saveOfficialMushafStateUseCase;
    private readonly GetQuranPageUseCase _getQuranPageUseCase;
    private readonly GetQuranIndexUseCase _getQuranIndexUseCase;
    private readonly List<QuranSurahIndexItem> _allSurahsMaster = new();
    private readonly Dictionary<Guid, List<(int Page, int WordIndex, string Type)>> _studentMistakesIsolated = new();

    [ObservableProperty] private Guid _sessionId;
    [ObservableProperty] private Guid _taskId;
    [ObservableProperty] private string? _operationMessage;
    [ObservableProperty] private QuranPage? _quranPage;
    [ObservableProperty] private QuranAyah? _selectedAyah;
    [ObservableProperty] private string _studentName = "الطالب";
    [ObservableProperty] private string _halaqaName = "حلقة التحفيظ";
    [ObservableProperty] private string _taskType = "حفظ";
    [ObservableProperty] private int _targetPage = 1;
    [ObservableProperty] private int _mistakesCount;
    [ObservableProperty] private string _sessionRating = "ممتاز";
    [ObservableProperty] private string _sessionNotes = string.Empty;
    [ObservableProperty] private bool _isStudentSession;
    [ObservableProperty] private Guid _studentId;
    [ObservableProperty] private string _pageNumberInput = "1";
    [ObservableProperty] private bool _isQuranLoading;
    [ObservableProperty] private string? _quranMessage;
    [ObservableProperty] private string _currentSurahName = "سورة الفاتحة";
    [ObservableProperty] private string _currentJuzText = "الجزء الأول";
    [ObservableProperty] private string _callStatusLabel = "في انتظار الرد على المكالمة";
    [ObservableProperty] private string _callStatusDescription = "في انتظار قبول المكالمة من الطالب...";
    [ObservableProperty] private string _callActionButtonText = "طلب اتصال مباشر";
    [ObservableProperty] private bool _isCallActive;

    // Teacher Direct Controls over Student & Quran
    [ObservableProperty] private bool _isMushafVisibleToStudent = true;
    [ObservableProperty] private bool _isStudentMicMutedByTeacher;
    [ObservableProperty] private bool _isStudentCameraMutedByTeacher;
    [ObservableProperty] private int? _stopAyahNumber;

    // Index Dialog State
    [ObservableProperty] private bool _isIndexDialogOpen;
    [ObservableProperty] private string _selectedIndexTab = "Surahs";
    [ObservableProperty] private string _indexSearchText = string.Empty;

    public ObservableCollection<InteractiveQuranWord> InteractiveWords { get; } = new();
    public ObservableCollection<QuranSurahIndexItem> FilteredSurahs { get; } = new();
    public ObservableCollection<QuranJuzIndexItem> AllJuzs { get; } = new();
    public ObservableCollection<int> AllPages { get; } = new();

    public LiveSessionStore Store { get; }

    public event EventHandler? BackRequested;
    public event EventHandler<Guid>? SessionCompleted;

    public string ConnectionLabel => Store.ConnectionState switch
    {
        LiveSessionState.Connected => "اتصال مباشر P2P",
        LiveSessionState.DirectConnectionUnavailable => "الاتصال المباشر غير متاح",
        LiveSessionState.Negotiating or LiveSessionState.Reconnecting => "جارِ التفاوض المباشر",
        _ => "في انتظار تهيئة الجلسة"
    };

    public LiveSessionViewModel(
        LiveSessionStore store,
        IPeerMediaConnection peerMediaConnection,
        IMushafRealtimeChannel mushafRealtimeChannel,
        ILocalVideoRecorder localVideoRecorder,
        SaveOfficialMushafStateUseCase saveOfficialMushafStateUseCase,
        GetQuranPageUseCase getQuranPageUseCase,
        GetQuranIndexUseCase getQuranIndexUseCase)
    {
        Store = store;
        _peerMediaConnection = peerMediaConnection;
        _mushafRealtimeChannel = mushafRealtimeChannel;
        _localVideoRecorder = localVideoRecorder;
        _saveOfficialMushafStateUseCase = saveOfficialMushafStateUseCase;
        _getQuranPageUseCase = getQuranPageUseCase;
        _getQuranIndexUseCase = getQuranIndexUseCase;

        _peerMediaConnection.StateChanged += (_, state) =>
        {
            Store.SetConnectionState(state.State, state.Reason);
            OnPropertyChanged(nameof(ConnectionLabel));
        };
        _peerMediaConnection.RemoteMediaStateChanged += (_, state) => Store.SetPeerMedia(state.IsMicrophoneMuted, state.IsCameraEnabled);
        _mushafRealtimeChannel.PresenceReceived += (_, state) => Store.SetPeerMushafPresence(state);
        _localVideoRecorder.StateChanged += (_, state) => Store.SetRecording(state);

        for (int p = 1; p <= 604; p++)
        {
            AllPages.Add(p);
        }
    }

    public async Task InitializeForStudentAsync(
        Halaqa.Desktop.Features.FollowUp.Domain.Entities.StudentFollowUpSummary student,
        string taskType,
        int targetPage)
    {
        StudentId = student.StudentId;
        StudentName = student.StudentName;
        HalaqaName = student.HalaqaName ?? "حلقة التحفيظ";
        TaskType = taskType;
        TargetPage = targetPage;
        MistakesCount = 0;
        StopAyahNumber = null;
        SessionRating = "ممتاز";
        SessionNotes = string.Empty;
        IsStudentSession = true;
        IsCallActive = false;
        IsIndexDialogOpen = false;
        IsMushafVisibleToStudent = true;
        IsStudentMicMutedByTeacher = false;
        IsStudentCameraMutedByTeacher = false;
        CallStatusLabel = "في انتظار الرد على المكالمة";
        CallStatusDescription = $"في انتظار انضمام الطالب {student.StudentName} للمكالمة...";
        CallActionButtonText = "طلب اتصال مباشر";
        OperationMessage = $"بدأت جلسة تسميع {taskType} للطالب {student.StudentName}.";

        await EnsureIndexLoadedAsync();
        await LoadMushafPageAsync(targetPage);
    }

    [RelayCommand]
    private async Task ToggleMushafVisibilityForStudentAsync()
    {
        IsMushafVisibleToStudent = !IsMushafVisibleToStudent;
        var msg = IsMushafVisibleToStudent ? "تم إظهار المصحف لشاشة الطالب." : "تم إخفاء المصحف عن شاشة الطالب (تسميع غيبي).";
        OperationMessage = msg;
        await _mushafRealtimeChannel.SendPresenceAsync(new MushafPresenceState(1, QuranPage?.PageNumber ?? 1, null, null, IsFollowingPeer: IsMushafVisibleToStudent));
    }

    [RelayCommand]
    private async Task ToggleStudentMicAsync()
    {
        IsStudentMicMutedByTeacher = !IsStudentMicMutedByTeacher;
        OperationMessage = IsStudentMicMutedByTeacher ? "تم كتم ميكروفون الطالب من قبل المعلم." : "تم تشغيل ميكروفون الطالب.";
        await _peerMediaConnection.SetMicrophoneMutedAsync(IsStudentMicMutedByTeacher);
    }

    [RelayCommand]
    private async Task ToggleStudentCameraAsync()
    {
        IsStudentCameraMutedByTeacher = !IsStudentCameraMutedByTeacher;
        OperationMessage = IsStudentCameraMutedByTeacher ? "تم إيقاف كاميرا الطالب من قبل المعلم." : "تم تفعيل كاميرا الطالب.";
        await _peerMediaConnection.SetCameraEnabledAsync(!IsStudentCameraMutedByTeacher);
    }

    [RelayCommand]
    private void MarkAyahStopPoint(InteractiveQuranWord? word)
    {
        if (word == null) return;
        word.ToggleStopPoint();
        if (word.IsStopPoint)
        {
            StopAyahNumber = word.AyahNumber;
            OperationMessage = $"🛑 تم تعيين نقطة توقف التسميع عند نهاية الآية ({word.AyahNumber}) بنجاح.";
        }
        else
        {
            StopAyahNumber = null;
            OperationMessage = "تم إلغاء نقطة توقف التسميع.";
        }
    }

        [RelayCommand]
    private void SelectIndexTab(string? tab)
    {
        if (!string.IsNullOrEmpty(tab))
        {
            SelectedIndexTab = tab;
        }
    }

private async Task EnsureIndexLoadedAsync()
    {
        if (_allSurahsMaster.Count == 0)
        {
            var surahsResult = await _getQuranIndexUseCase.GetSurahsAsync();
            if (surahsResult.IsSuccess && surahsResult.Value != null)
            {
                _allSurahsMaster.AddRange(surahsResult.Value);
                ApplySurahFilter();
            }

            var juzResult = await _getQuranIndexUseCase.GetJuzsAsync();
            if (juzResult.IsSuccess && juzResult.Value != null)
            {
                AllJuzs.Clear();
                foreach (var j in juzResult.Value)
                {
                    AllJuzs.Add(j);
                }
            }
        }
    }

    partial void OnIndexSearchTextChanged(string value) => ApplySurahFilter();

    private void ApplySurahFilter()
    {
        FilteredSurahs.Clear();
        var query = string.IsNullOrWhiteSpace(IndexSearchText)
            ? _allSurahsMaster
            : _allSurahsMaster.Where(s => s.Name.Contains(IndexSearchText.Trim(), StringComparison.OrdinalIgnoreCase));

        foreach (var s in query)
        {
            FilteredSurahs.Add(s);
        }
    }

    [RelayCommand]
    private async Task OpenIndexDialogAsync()
    {
        await EnsureIndexLoadedAsync();
        IsIndexDialogOpen = true;
    }

    [RelayCommand]
    private void CloseIndexDialog() => IsIndexDialogOpen = false;

    [RelayCommand]
    private async Task SelectSurahAsync(QuranSurahIndexItem? surah)
    {
        if (surah == null) return;
        IsIndexDialogOpen = false;
        await LoadMushafPageAsync(surah.StartPage);
    }

    [RelayCommand]
    private async Task SelectJuzAsync(QuranJuzIndexItem? juz)
    {
        if (juz == null) return;
        IsIndexDialogOpen = false;
        await LoadMushafPageAsync(juz.StartPage);
    }

    [RelayCommand]
    private async Task SelectPageAsync(int page)
    {
        IsIndexDialogOpen = false;
        await LoadMushafPageAsync(page);
    }

    [RelayCommand]
    private void ToggleCall()
    {
        IsCallActive = !IsCallActive;
        if (IsCallActive)
        {
            CallStatusLabel = "مكالمة متصلة مباشرة";
            CallStatusDescription = "المكالمة المباشرة جارية مع الطالب.";
            CallActionButtonText = "إنهاء المكالمة";
            OperationMessage = "تم بدء الاتصال المباشر بنجاح.";
        }
        else
        {
            CallStatusLabel = "غير نشط";
            CallStatusDescription = "انتهت المكالمة المباشرة.";
            CallActionButtonText = "طلب اتصال مباشر";
            OperationMessage = "أُغلقت المكالمة المباشرة.";
        }
    }

    [RelayCommand]
    private async Task ToggleMicrophoneAsync()
    {
        var isMuted = !Store.Media.IsMicrophoneMuted;
        await _peerMediaConnection.SetMicrophoneMutedAsync(isMuted);
        Store.SetMicrophoneMuted(isMuted);
    }

    [RelayCommand]
    private async Task ToggleCameraAsync()
    {
        var isEnabled = !Store.Media.IsCameraEnabled;
        await _peerMediaConnection.SetCameraEnabledAsync(isEnabled);
        Store.SetCameraEnabled(isEnabled);
    }

    public async Task InitializeMushafAsync(CancellationToken cancellationToken = default) =>
        await LoadMushafPageAsync(1, cancellationToken: cancellationToken);

    [RelayCommand]
    private async Task LoadMushafPageAsync() =>
        await LoadMushafPageFromInputAsync();

    [RelayCommand]
    private async Task PreviousMushafPageAsync()
    {
        var page = QuranPage?.PageNumber ?? ParsePageNumber(PageNumberInput) ?? 1;
        await LoadMushafPageAsync(Math.Max(1, page - 1));
    }

    [RelayCommand]
    private async Task NextMushafPageAsync()
    {
        var page = QuranPage?.PageNumber ?? ParsePageNumber(PageNumberInput) ?? 1;
        await LoadMushafPageAsync(Math.Min(604, page + 1));
    }

    private async Task LoadMushafPageFromInputAsync()
    {
        var pageNumber = ParsePageNumber(PageNumberInput);
        if (pageNumber is null)
        {
            QuranMessage = "أدخل رقم صفحة صالحاً بين 1 و 604.";
            return;
        }

        await LoadMushafPageAsync(pageNumber.Value);
    }

    public async Task LoadMushafPageAsync(
        int pageNumber,
        int? selectedAyahId = null,
        CancellationToken cancellationToken = default)
    {
        IsQuranLoading = true;
        QuranMessage = null;
        try
        {
            var result = await _getQuranPageUseCase.ExecuteAsync(1, pageNumber, cancellationToken);
            if (!result.IsSuccess || result.Value is null)
            {
                QuranMessage = result.Error?.Message ?? "تعذر تحميل بيانات الصفحة.";
                return;
            }

            QuranPage = result.Value;
            PageNumberInput = result.Value.PageNumber.ToString();
            TargetPage = result.Value.PageNumber;

            if (result.Value.Surahs.Count > 0)
            {
                CurrentSurahName = result.Value.Surahs[0].Name;
            }
            if (result.Value.Ayahs.Count > 0 && result.Value.Ayahs[0].Juz.HasValue)
            {
                CurrentJuzText = $"الجزء {result.Value.Ayahs[0].Juz}";
            }

            InteractiveWords.Clear();
            var globalWordIdx = 0;
            foreach (var ayah in result.Value.Ayahs)
            {
                var ayahWords = ayah.PageGlyphText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ayahWords.Length; i++)
                {
                    var isEnd = i == ayahWords.Length - 1;
                    InteractiveWords.Add(new InteractiveQuranWord
                    {
                        WordIndex = globalWordIdx++,
                        Text = ayahWords[i],
                        PageNumber = result.Value.PageNumber,
                        AyahNumber = ayah.Number,
                        IsAyahEndSymbol = isEnd
                    });
                }
            }

            // Restore any isolated student mistakes previously recorded on this page
            if (_studentMistakesIsolated.TryGetValue(StudentId, out var savedList))
            {
                foreach (var (p, wIdx, type) in savedList.Where(m => m.Page == pageNumber))
                {
                    var w = InteractiveWords.FirstOrDefault(x => x.WordIndex == wIdx);
                    w?.SetMistake(type);
                }
            }

            RecalculateMistakes();
        }
        finally
        {
            IsQuranLoading = false;
        }
    }

    [RelayCommand]
    private void TagMemorizationMistake(InteractiveQuranWord? word) => TagWordDirect(word, "حفظ");

    [RelayCommand]
    private void TagTajweedMistake(InteractiveQuranWord? word) => TagWordDirect(word, "تجويد");

    [RelayCommand]
    private void TagTashkeelMistake(InteractiveQuranWord? word) => TagWordDirect(word, "تشكيل");

    [RelayCommand]
    private void TagAlertMistake(InteractiveQuranWord? word) => TagWordDirect(word, "تنبيه");

    [RelayCommand]
    private void ClearWordMistake(InteractiveQuranWord? word) => TagWordDirect(word, "إلغاء");

    public void TagWordDirect(InteractiveQuranWord? word, string mistakeType)
    {
        if (word == null) return;
        word.SetMistake(mistakeType);
        RecalculateMistakes();

        // Save isolated mistake per student
        if (!_studentMistakesIsolated.ContainsKey(StudentId))
        {
            _studentMistakesIsolated[StudentId] = new List<(int, int, string)>();
        }
        _studentMistakesIsolated[StudentId].RemoveAll(m => m.Page == word.PageNumber && m.WordIndex == word.WordIndex);
        if (word.HasMistake)
        {
            _studentMistakesIsolated[StudentId].Add((word.PageNumber, word.WordIndex, word.MistakeType!));
        }

        // Live Realtime Mistake broadcast to student
        _ = _mushafRealtimeChannel.SendRepeatRequestAsync(new PeerRepeatRequest(SessionId, TaskId, word.AyahNumber, mistakeType));

        OperationMessage = mistakeType == "إلغاء"
            ? "تم إزالة الخطأ من الكلمة ومزامنة الحالة."
            : $"تم رصد خطأ {mistakeType} في الكلمة ومزامنته فورياً مع الطالب.";
    }

    private void RecalculateMistakes()
    {
        MistakesCount = InteractiveWords.Count(w => w.HasMistake);
    }

    [RelayCommand]
    private void CompleteSession()
    {
        OperationMessage = "تم إنهاء وحفظ جلسة التسميع بنجاح.";
        SessionCompleted?.Invoke(this, StudentId);
    }

    [RelayCommand]
    private void GoBack() => BackRequested?.Invoke(this, EventArgs.Empty);

    private static int? ParsePageNumber(string? value) =>
        int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var pageNumber) &&
        pageNumber is >= 1 and <= 604
            ? pageNumber
            : null;
}
