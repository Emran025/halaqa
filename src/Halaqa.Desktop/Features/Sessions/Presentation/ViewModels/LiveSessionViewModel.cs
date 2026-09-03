using System.Collections.ObjectModel;
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
        MistakeType = mistakeType == "\u0625\u0644\u063a\u0627\u0621" ? null : mistakeType;
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
            BackgroundBrush = new SolidColorBrush(Color.FromArgb(220, 200, 230, 201));
            BorderBrush = new SolidColorBrush(Color.FromRgb(46, 125, 50));
            return;
        }

        switch (MistakeType)
        {
            case "\u062d\u0641\u0638":
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(200, 255, 205, 210));
                BorderBrush = new SolidColorBrush(Color.FromRgb(211, 47, 47));
                break;
            case "\u062a\u062c\u0648\u064a\u062f":
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(200, 255, 224, 178));
                BorderBrush = new SolidColorBrush(Color.FromRgb(245, 124, 0));
                break;
            case "\u062a\u0634\u0643\u064a\u0644":
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(200, 255, 245, 157));
                BorderBrush = new SolidColorBrush(Color.FromRgb(251, 192, 45));
                break;
            case "\u062a\u0646\u0628\u064a\u0647":
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(200, 187, 222, 251));
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
    private readonly CreateLiveSessionUseCase _createLiveSessionUseCase;
    private readonly CreateSessionTaskUseCase _createSessionTaskUseCase;
    private readonly PrepareLiveSessionUseCase _prepareLiveSessionUseCase;
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
    [ObservableProperty] private string _studentName = "\u0627\u0644\u0637\u0627\u0644\u0628";
    [ObservableProperty] private string _halaqaName = "\u062d\u0644\u0642\u0629 \u0627\u0644\u062a\u062d\u0641\u064a\u0638";
    [ObservableProperty] private string _taskType = "\u062d\u0641\u0638";
    [ObservableProperty] private int _targetPage = 1;
    [ObservableProperty] private int _mistakesCount;
    [ObservableProperty] private bool _isStudentSession;
    [ObservableProperty] private Guid _studentId;
    [ObservableProperty] private string _pageNumberInput = "1";
    [ObservableProperty] private bool _isQuranLoading;
    [ObservableProperty] private string? _quranMessage;
    [ObservableProperty] private string _currentSurahName = "\u0633\u0648\u0631\u0629 \u0627\u0644\u0641\u0627\u062a\u062d\u0629";
    [ObservableProperty] private string _currentJuzText = "\u0627\u0644\u062c\u0632\u0621 \u0627\u0644\u0623\u0648\u0644";
    [ObservableProperty] private string _callStatusLabel = "\u0641\u064a \u0627\u0646\u062a\u0638\u0627\u0631 \u0627\u0644\u0631\u062f \u0639\u0644\u0649 \u0627\u0644\u0645\u0643\u0627\u0644\u0645\u0629";
    [ObservableProperty] private string _callStatusDescription = "\u0641\u064a \u0627\u0646\u062a\u0638\u0627\u0631 \u0642\u0628\u0648\u0644 \u0627\u0644\u0645\u0643\u0627\u0644\u0645\u0629 \u0645\u0646 \u0627\u0644\u0637\u0627\u0644\u0628...";
    [ObservableProperty] private string _callActionButtonText = "\u0637\u0644\u0628 \u0627\u062a\u0635\u0627\u0644 \u0645\u0628\u0627\u0634\u0631";
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

    // Evaluation Overlay State
    [ObservableProperty] private bool _isEvaluationPanelOpen;
    [ObservableProperty] private int _evaluationScore = 4;
    [ObservableProperty] private string _evaluationNotes = string.Empty;
    [ObservableProperty] private bool _isScore1;
    [ObservableProperty] private bool _isScore2;
    [ObservableProperty] private bool _isScore3;
    [ObservableProperty] private bool _isScore4 = true;
    [ObservableProperty] private bool _isScore5;

    // Evaluation mistake breakdown (computed before showing overlay)
    [ObservableProperty] private int _evalMemorizationMistakes;
    [ObservableProperty] private int _evalTajweedMistakes;
    [ObservableProperty] private int _evalTashkeelMistakes;
    [ObservableProperty] private int _evalAlertMistakes;

    public string EvaluationRatingLabel => EvaluationScore switch
    {
        5 => "\u0645\u0645\u062a\u0627\u0632 (5/5)",
        4 => "\u062c\u064a\u062f \u062c\u062f\u0627\u064b (4/5)",
        3 => "\u062c\u064a\u062f (3/5)",
        2 => "\u0645\u0642\u0628\u0648\u0644 (2/5)",
        1 => "\u0636\u0639\u064a\u0641 (1/5)",
        _ => "\u062c\u064a\u062f \u062c\u062f\u0627\u064b (4/5)"
    };

    public ObservableCollection<InteractiveQuranWord> InteractiveWords { get; } = new();
    public ObservableCollection<QuranSurahIndexItem> FilteredSurahs { get; } = new();
    public ObservableCollection<QuranJuzIndexItem> AllJuzs { get; } = new();
    public ObservableCollection<int> AllPages { get; } = new();

    public LiveSessionStore Store { get; }

    public event EventHandler? BackRequested;
    public event EventHandler<SessionReport>? SessionCompleted;

    public string ConnectionLabel => Store.ConnectionState switch
    {
        LiveSessionState.Connected => "\u0627\u062a\u0635\u0627\u0644 \u0645\u0628\u0627\u0634\u0631 P2P",
        LiveSessionState.DirectConnectionUnavailable => "\u0627\u0644\u0627\u062a\u0635\u0627\u0644 \u0627\u0644\u0645\u0628\u0627\u0634\u0631 \u063a\u064a\u0631 \u0645\u062a\u0627\u062d",
        LiveSessionState.Negotiating or LiveSessionState.Reconnecting => "\u062c\u0627\u0631\u0650 \u0627\u0644\u062a\u0641\u0627\u0648\u0636 \u0627\u0644\u0645\u0628\u0627\u0634\u0631",
        _ => "\u0641\u064a \u0627\u0646\u062a\u0638\u0627\u0631 \u062a\u0647\u064a\u0626\u0629 \u0627\u0644\u062c\u0644\u0633\u0629"
    };

    public LiveSessionViewModel(
        LiveSessionStore store,
        IPeerMediaConnection peerMediaConnection,
        IMushafRealtimeChannel mushafRealtimeChannel,
        ILocalVideoRecorder localVideoRecorder,
        CreateLiveSessionUseCase createLiveSessionUseCase,
        CreateSessionTaskUseCase createSessionTaskUseCase,
        PrepareLiveSessionUseCase prepareLiveSessionUseCase,
        SaveOfficialMushafStateUseCase saveOfficialMushafStateUseCase,
        GetQuranPageUseCase getQuranPageUseCase,
        GetQuranIndexUseCase getQuranIndexUseCase)
    {
        Store = store;
        _peerMediaConnection = peerMediaConnection;
        _mushafRealtimeChannel = mushafRealtimeChannel;
        _localVideoRecorder = localVideoRecorder;
        _createLiveSessionUseCase = createLiveSessionUseCase;
        _createSessionTaskUseCase = createSessionTaskUseCase;
        _prepareLiveSessionUseCase = prepareLiveSessionUseCase;
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
            AllPages.Add(p);
    }

    public async Task InitializeForStudentAsync(
        Halaqa.Desktop.Features.FollowUp.Domain.Entities.StudentFollowUpSummary student,
        string taskType,
        int targetPage)
    {
        StudentId = student.StudentId;
        StudentName = student.StudentName;
        HalaqaName = student.HalaqaName ?? "\u062d\u0644\u0642\u0629 \u0627\u0644\u062a\u062d\u0641\u064a\u0638";
        TaskType = taskType;
        TargetPage = targetPage;
        MistakesCount = 0;
        StopAyahNumber = null;
        EvaluationNotes = string.Empty;
        EvaluationScore = 4;
        IsEvaluationPanelOpen = false;
        IsStudentSession = true;
        IsCallActive = false;
        IsIndexDialogOpen = false;
        IsMushafVisibleToStudent = true;
        IsStudentMicMutedByTeacher = false;
        IsStudentCameraMutedByTeacher = false;
        CallStatusLabel = "\u0641\u064a \u0627\u0646\u062a\u0638\u0627\u0631 \u0627\u0644\u0631\u062f \u0639\u0644\u0649 \u0627\u0644\u0645\u0643\u0627\u0644\u0645\u0629";
        CallStatusDescription = $"\u0641\u064a \u0627\u0646\u062a\u0638\u0627\u0631 \u0627\u0646\u0636\u0645\u0627\u0645 \u0627\u0644\u0637\u0627\u0644\u0628 {student.StudentName} \u0644\u0644\u0645\u0643\u0627\u0644\u0645\u0629...";
        CallActionButtonText = "\u0637\u0644\u0628 \u0627\u062a\u0635\u0627\u0644 \u0645\u0628\u0627\u0634\u0631";
        OperationMessage = $"جاري إنشاء جلسة تسميع {taskType} للطالب {student.StudentName} على الخادم...";
        SetScoreSelected(4);

        var taskTypeValue = ParseTaskType(taskType);
        var sessionResult = await _createLiveSessionUseCase.ExecuteAsync(new CreateLiveSessionCommand(
            student.HalaqaId ?? Guid.Empty,
            student.StudentId,
            FollowUpItemId: null,
            TaskType: taskTypeValue,
            ScheduledAt: null,
            ClientOperationId: Guid.NewGuid()));
        if (!sessionResult.IsSuccess || sessionResult.Value is null)
        {
            Store.SetConnectionState(LiveSessionState.DirectConnectionUnavailable, sessionResult.Error?.Message);
            CallStatusLabel = "تعذر إنشاء جلسة رسمية";
            CallStatusDescription = sessionResult.Error?.Message ?? "لم يتم إنشاء جلسة التسميع في الخادم.";
            OperationMessage = CallStatusDescription;
            await EnsureIndexLoadedAsync();
            await LoadMushafPageAsync(targetPage);
            return;
        }

        SessionId = sessionResult.Value.Id;
        var taskResult = await _createSessionTaskUseCase.ExecuteAsync(new CreateSessionTaskCommand(
            SessionId,
            taskTypeValue,
            Guid.NewGuid(),
            StartPage: targetPage));
        if (!taskResult.IsSuccess || taskResult.Value is null)
        {
            Store.SetConnectionState(LiveSessionState.DirectConnectionUnavailable, taskResult.Error?.Message);
            CallStatusLabel = "تعذر إنشاء مهمة الجلسة";
            CallStatusDescription = taskResult.Error?.Message ?? "تم إنشاء الجلسة دون مهمة تسميع رسمية.";
            OperationMessage = CallStatusDescription;
            await EnsureIndexLoadedAsync();
            await LoadMushafPageAsync(targetPage);
            return;
        }

        TaskId = taskResult.Value.Id;
        await PrepareRealtimeSessionAsync();

        await EnsureIndexLoadedAsync();
        await LoadMushafPageAsync(targetPage);
    }

    private async Task<bool> PrepareRealtimeSessionAsync()
    {
        if (SessionId == Guid.Empty)
            return false;

        var prepareResult = await _prepareLiveSessionUseCase.ExecuteAsync(
            SessionId,
            clientConnectionId: Guid.NewGuid().ToString("N"));
        if (!prepareResult.IsSuccess)
        {
            Store.SetConnectionState(LiveSessionState.DirectConnectionUnavailable, prepareResult.Error?.Message);
            CallStatusLabel = "في انتظار قبول الطالب";
            CallStatusDescription = prepareResult.Error?.Message ?? "تم إنشاء الجلسة، ويجب قبولها من الطالب قبل الاتصال المباشر.";
            OperationMessage = CallStatusDescription;
            return false;
        }

        var prepared = prepareResult.Value;
        await _peerMediaConnection.InitializeAsync(prepared.Config);
        if (Store.ConnectionState == LiveSessionState.DirectConnectionUnavailable)
        {
            CallStatusLabel = "الاتصال المباشر غير متاح";
            CallStatusDescription = Store.ConnectionMessage ?? "تعذر تشغيل محول الوسائط المباشر.";
            OperationMessage = CallStatusDescription;
            return false;
        }

        Store.SetConnectionState(LiveSessionState.Negotiating, "تم تفويض قناة الجلسة، وجارِ انتظار تفاوض الاتصال المباشر.");
        CallStatusLabel = "الجلسة الرسمية جاهزة";
        CallStatusDescription = "تم إنشاء الجلسة والمهمة وتفويض قناة الاتصال. يبدأ الفيديو بعد قبول الطالب والتفاوض المباشر.";
        OperationMessage = CallStatusDescription;
        return true;
    }

    [RelayCommand]
    private async Task ToggleMushafVisibilityForStudentAsync()
    {
        IsMushafVisibleToStudent = !IsMushafVisibleToStudent;
        OperationMessage = IsMushafVisibleToStudent
            ? "\u062a\u0645 \u0625\u0638\u0647\u0627\u0631 \u0627\u0644\u0645\u0635\u062d\u0641 \u0644\u0634\u0627\u0634\u0629 \u0627\u0644\u0637\u0627\u0644\u0628."
            : "\u062a\u0645 \u0625\u062e\u0641\u0627\u0621 \u0627\u0644\u0645\u0635\u062d\u0641 \u0639\u0646 \u0634\u0627\u0634\u0629 \u0627\u0644\u0637\u0627\u0644\u0628 (\u062a\u0633\u0645\u064a\u0639 \u063a\u064a\u0628\u064a).";
        await _mushafRealtimeChannel.SendPresenceAsync(new MushafPresenceState(QuranPage?.EditionId ?? 1, QuranPage?.PageNumber ?? 1, null, null, IsFollowingPeer: IsMushafVisibleToStudent));
    }

    [RelayCommand]
    private async Task ToggleStudentMicAsync()
    {
        IsStudentMicMutedByTeacher = !IsStudentMicMutedByTeacher;
        OperationMessage = IsStudentMicMutedByTeacher
            ? "\u062a\u0645 \u0643\u062a\u0645 \u0645\u064a\u0643\u0631\u0648\u0641\u0648\u0646 \u0627\u0644\u0637\u0627\u0644\u0628 \u0645\u0646 \u0642\u0628\u0644 \u0627\u0644\u0645\u0639\u0644\u0645."
            : "\u062a\u0645 \u062a\u0634\u063a\u064a\u0644 \u0645\u064a\u0643\u0631\u0648\u0641\u0648\u0646 \u0627\u0644\u0637\u0627\u0644\u0628.";
        await _peerMediaConnection.SetMicrophoneMutedAsync(IsStudentMicMutedByTeacher);
    }

    [RelayCommand]
    private async Task ToggleStudentCameraAsync()
    {
        IsStudentCameraMutedByTeacher = !IsStudentCameraMutedByTeacher;
        OperationMessage = IsStudentCameraMutedByTeacher
            ? "\u062a\u0645 \u0625\u064a\u0642\u0627\u0641 \u0643\u0627\u0645\u064a\u0631\u0627 \u0627\u0644\u0637\u0627\u0644\u0628 \u0645\u0646 \u0642\u0628\u0644 \u0627\u0644\u0645\u0639\u0644\u0645."
            : "\u062a\u0645 \u062a\u0641\u0639\u064a\u0644 \u0643\u0627\u0645\u064a\u0631\u0627 \u0627\u0644\u0637\u0627\u0644\u0628.";
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
            OperationMessage = $"\u0646\u0642\u0637\u0629 \u062a\u0648\u0642\u0641 \u0639\u0646\u062f \u0646\u0647\u0627\u064a\u0629 \u0627\u0644\u0622\u064a\u0629 ({word.AyahNumber}).";
        }
        else
        {
            StopAyahNumber = null;
            OperationMessage = "\u062a\u0645 \u0625\u0644\u063a\u0627\u0621 \u0646\u0642\u0637\u0629 \u062a\u0648\u0642\u0641 \u0627\u0644\u062a\u0633\u0645\u064a\u0639.";
        }
    }

    [RelayCommand]
    private void SelectIndexTab(string? tab)
    {
        if (!string.IsNullOrEmpty(tab))
            SelectedIndexTab = tab;
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
                    AllJuzs.Add(j);
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
            FilteredSurahs.Add(s);
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
    private async Task ToggleCallAsync()
    {
        if (IsCallActive)
        {
            IsCallActive = false;
            CallStatusLabel = "غير نشط";
            CallStatusDescription = "انتهت المكالمة المباشرة.";
            CallActionButtonText = "طلب اتصال مباشر";
            return;
        }

        if (SessionId == Guid.Empty || TaskId == Guid.Empty)
        {
            CallStatusLabel = "الجلسة غير جاهزة";
            CallStatusDescription = "لا يمكن طلب اتصال قبل إنشاء الجلسة والمهمة في الخادم.";
            OperationMessage = CallStatusDescription;
            return;
        }

        if (Store.ConnectionState != LiveSessionState.Negotiating && Store.ConnectionState != LiveSessionState.Connected &&
            !await PrepareRealtimeSessionAsync())
        {
            return;
        }

        await _peerMediaConnection.CreateOfferAsync();
        IsCallActive = Store.ConnectionState == LiveSessionState.Connected;
        CallStatusLabel = IsCallActive ? "مكالمة متصلة مباشرة" : "تم إرسال طلب الاتصال";
        CallStatusDescription = IsCallActive
            ? "المكالمة المباشرة جارية مع الطالب."
            : "تم إرسال طلب التفاوض المباشر، وتبقى الوسائط غير متصلة حتى يكتمل offer/answer.";
        CallActionButtonText = IsCallActive ? "إنهاء المكالمة" : "إعادة طلب الاتصال";
        OperationMessage = CallStatusDescription;
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
    private async Task LoadMushafPageAsync() => await LoadMushafPageFromInputAsync();

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
            QuranMessage = "\u0623\u062f\u062e\u0644 \u0631\u0642\u0645 \u0635\u0641\u062d\u0629 \u0635\u0627\u0644\u062d\u0627\u064b \u0628\u064a\u0646 1 \u0648 604.";
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
                QuranMessage = result.Error?.Message ?? "\u062a\u0639\u0630\u0631 \u062a\u062d\u0645\u064a\u0644 \u0628\u064a\u0627\u0646\u0627\u062a \u0627\u0644\u0635\u0641\u062d\u0629.";
                return;
            }

            QuranPage = result.Value;
            PageNumberInput = result.Value.PageNumber.ToString();
            TargetPage = result.Value.PageNumber;

            var localPresence = new MushafPresenceState(
                result.Value.EditionId,
                result.Value.PageNumber,
                selectedAyahId,
                null,
                IsFollowingPeer: true);
            Store.SetLocalMushafPresence(localPresence);
            await _mushafRealtimeChannel.SendPresenceAsync(localPresence, cancellationToken);

            if (SessionId != Guid.Empty)
            {
                var saveStateResult = await _saveOfficialMushafStateUseCase.ExecuteAsync(
                    SessionId,
                    result.Value.EditionId,
                    result.Value.PageNumber,
                    selectedAyahId,
                    Guid.NewGuid(),
                    cancellationToken);
                if (!saveStateResult.IsSuccess)
                    OperationMessage = saveStateResult.Error?.Message ?? "تعذر حفظ حالة المصحف الرسمية.";
            }

            if (result.Value.Surahs.Count > 0)
                CurrentSurahName = result.Value.Surahs[0].Name;
            if (result.Value.Ayahs.Count > 0 && result.Value.Ayahs[0].Juz.HasValue)
                CurrentJuzText = $"\u0627\u0644\u062c\u0632\u0621 {result.Value.Ayahs[0].Juz}";

            InteractiveWords.Clear();
            var globalWordIdx = 0;
            foreach (var ayah in result.Value.Ayahs)
            {
                var ayahWords = ayah.PageGlyphText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ayahWords.Length; i++)
                {
                    InteractiveWords.Add(new InteractiveQuranWord
                    {
                        WordIndex = globalWordIdx++,
                        Text = ayahWords[i],
                        PageNumber = result.Value.PageNumber,
                        AyahNumber = ayah.Number,
                        IsAyahEndSymbol = i == ayahWords.Length - 1
                    });
                }
            }

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
    private void TagMemorizationMistake(InteractiveQuranWord? word) => TagWordDirect(word, "\u062d\u0641\u0638");

    [RelayCommand]
    private void TagTajweedMistake(InteractiveQuranWord? word) => TagWordDirect(word, "\u062a\u062c\u0648\u064a\u062f");

    [RelayCommand]
    private void TagTashkeelMistake(InteractiveQuranWord? word) => TagWordDirect(word, "\u062a\u0634\u0643\u064a\u0644");

    [RelayCommand]
    private void TagAlertMistake(InteractiveQuranWord? word) => TagWordDirect(word, "\u062a\u0646\u0628\u064a\u0647");

    [RelayCommand]
    private void ClearWordMistake(InteractiveQuranWord? word) => TagWordDirect(word, "\u0625\u0644\u063a\u0627\u0621");

    public void TagWordDirect(InteractiveQuranWord? word, string mistakeType)
    {
        if (word == null) return;
        word.SetMistake(mistakeType);
        RecalculateMistakes();

        if (!_studentMistakesIsolated.ContainsKey(StudentId))
            _studentMistakesIsolated[StudentId] = new List<(int, int, string)>();

        _studentMistakesIsolated[StudentId].RemoveAll(m => m.Page == word.PageNumber && m.WordIndex == word.WordIndex);
        if (word.HasMistake)
            _studentMistakesIsolated[StudentId].Add((word.PageNumber, word.WordIndex, word.MistakeType!));

        _ = _mushafRealtimeChannel.SendRepeatRequestAsync(new PeerRepeatRequest(SessionId, TaskId, word.AyahNumber, mistakeType));

        OperationMessage = mistakeType == "\u0625\u0644\u063a\u0627\u0621"
            ? "\u062a\u0645 \u0625\u0632\u0627\u0644\u0629 \u0627\u0644\u062e\u0637\u0623 \u0645\u0646 \u0627\u0644\u0643\u0644\u0645\u0629."
            : $"\u062a\u0645 \u0631\u0635\u062f \u062e\u0637\u0623 {mistakeType} \u0641\u064a \u0627\u0644\u0643\u0644\u0645\u0629.";
    }

    private void RecalculateMistakes()
    {
        MistakesCount = InteractiveWords.Count(w => w.HasMistake);
    }

    // ── Evaluation Overlay ───────────────────────────────────────────────────

    [RelayCommand]
    private void OpenEvaluationPanel()
    {
        EvalMemorizationMistakes = InteractiveWords.Count(w => w.MistakeType == "\u062d\u0641\u0638");
        EvalTajweedMistakes      = InteractiveWords.Count(w => w.MistakeType == "\u062a\u062c\u0648\u064a\u062f");
        EvalTashkeelMistakes     = InteractiveWords.Count(w => w.MistakeType == "\u062a\u0634\u0643\u064a\u0644");
        EvalAlertMistakes        = InteractiveWords.Count(w => w.MistakeType == "\u062a\u0646\u0628\u064a\u0647");
        EvaluationNotes = string.Empty;
        SetScoreSelected(4);
        IsEvaluationPanelOpen = true;
    }

    [RelayCommand]
    private void CloseEvaluationPanel() => IsEvaluationPanelOpen = false;

    [RelayCommand]
    private void SetEvaluationScore(string? scoreStr)
    {
        if (int.TryParse(scoreStr, out var s) && s >= 1 && s <= 5)
            SetScoreSelected(s);
    }

    private void SetScoreSelected(int score)
    {
        EvaluationScore = score;
        IsScore1 = score == 1;
        IsScore2 = score == 2;
        IsScore3 = score == 3;
        IsScore4 = score == 4;
        IsScore5 = score == 5;
        OnPropertyChanged(nameof(EvaluationRatingLabel));
    }

    [RelayCommand]
    private void ConfirmEvaluation()
    {
        var report = new SessionReport(
            StudentId: StudentId,
            StudentName: StudentName,
            TaskType: TaskType,
            TargetPage: TargetPage,
            StopAyahNumber: StopAyahNumber,
            Mistakes: new SessionMistakeSummary(
                EvalMemorizationMistakes,
                EvalTajweedMistakes,
                EvalTashkeelMistakes,
                EvalAlertMistakes),
            Score: EvaluationScore,
            Rating: EvaluationRatingLabel,
            Notes: EvaluationNotes,
            CompletedAt: DateTimeOffset.Now);

        IsEvaluationPanelOpen = false;
        SessionCompleted?.Invoke(this, report);
    }

    [RelayCommand]
    private void GoBack() => BackRequested?.Invoke(this, EventArgs.Empty);

    private static SessionTaskType ParseTaskType(string taskType) => taskType switch
    {
        "حفظ" => SessionTaskType.Memorization,
        "مراجعة" => SessionTaskType.Review,
        _ => SessionTaskType.Recitation
    };

    private static int? ParsePageNumber(string? value) =>
        int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var pageNumber) &&
        pageNumber is >= 1 and <= 604
            ? pageNumber
            : null;
}
