using Halaqa.Desktop.Features.Sessions.Domain.Entities;
using Halaqa.Desktop.Features.Sessions.Domain.UseCases;

namespace Halaqa.Desktop.Features.Sessions.Presentation.Services;

/// <summary>
/// تتحقق هذه الخدمة دورياً من وجود جلسات "مطلوبة" (Requested) للطالب الحالي.
/// عند اكتشاف جلسة جديدة تُطلَق SessionDetected ليعرضها الشل كأوفرلاي فوري.
/// </summary>
public sealed class IncomingSessionPollingService : IDisposable, IAsyncDisposable
{
    private readonly ListSessionsUseCase _listSessionsUseCase;
    private readonly TimeSpan _interval;
    private CancellationTokenSource? _cts;
    private Task? _pollingTask;
    private Guid _lastSeenSessionId = Guid.Empty;

    public void Dispose()
    {
        Stop();
    }

    /// <summary>يُطلَق عند اكتشاف جلسة "Requested" جديدة لم يرها الطالب بعد.</summary>
    public event EventHandler<SessionListItem>? SessionDetected;

    public IncomingSessionPollingService(
        ListSessionsUseCase listSessionsUseCase,
        TimeSpan? interval = null)
    {
        _listSessionsUseCase = listSessionsUseCase;
        _interval = interval ?? TimeSpan.FromSeconds(2); // 2 ثانية للاستجابة الفورية
    }

    /// <summary>يبدأ دورة الفحص للطالب المُسجَّل دخوله.</summary>
    public void Start(Guid? studentId = null)
    {
        Stop(); // أوقف أي دورة سابقة
        _lastSeenSessionId = Guid.Empty;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _pollingTask = Task.Run(() => PollLoopAsync(studentId, token), token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async Task PollLoopAsync(Guid? studentId, CancellationToken cancellationToken)
    {
        // فحص فوري عند البدء — بدون انتظار الـ interval
        try { await CheckForIncomingSessionAsync(studentId, cancellationToken); }
        catch (OperationCanceledException) { return; }
        catch { /* تجاهل خطأ الشبكة */ }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await CheckForIncomingSessionAsync(studentId, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // تجاهل أخطاء الشبكة — نعيد المحاولة في الدورة القادمة
            }
        }
    }

    private async Task CheckForIncomingSessionAsync(Guid? studentId, CancellationToken cancellationToken)
    {
        var query = new SessionQuery(
            HalaqaId: null,
            StudentId: studentId,
            State: OfficialSessionState.Requested,
            From: DateTimeOffset.UtcNow.AddHours(-1), // نبحث في آخر ساعة فقط
            To: null,
            Page: 1,
            PerPage: 5);

        var result = await _listSessionsUseCase.ExecuteAsync(query, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
            return;

        // نجد أحدث جلسة Requested لم يتم عرضها بعد
        var newest = result.Value.Sessions
            .OrderByDescending(s => s.RequestedAt)
            .FirstOrDefault();

        if (newest is null || newest.Id == _lastSeenSessionId)
            return;

        _lastSeenSessionId = newest.Id;

        // رفع الحدث على UI thread
        System.Windows.Application.Current?.Dispatcher.Invoke(
            () => SessionDetected?.Invoke(this, newest));
    }

    public async ValueTask DisposeAsync()
    {
        Stop();
        if (_pollingTask is not null)
        {
            try { await _pollingTask.ConfigureAwait(false); }
            catch { /* ignored */ }
        }
    }
}
