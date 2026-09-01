using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Config.Http;

public interface IApiClient
{
    Task<Result<TResponse>> GetAsync<TResponse>(string relativePath, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string relativePath, TRequest request, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> PostEmptyAsync<TResponse>(string relativePath, CancellationToken cancellationToken = default);
    Task<Result> PostAsync<TRequest>(string relativePath, TRequest request, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> PostMultipartAsync<TResponse>(string relativePath, MultipartFormDataContent content, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> PutAsync<TRequest, TResponse>(string relativePath, TRequest request, CancellationToken cancellationToken = default);
    Task<Result> PutAsync<TRequest>(string relativePath, TRequest request, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> PatchAsync<TRequest, TResponse>(string relativePath, TRequest request, CancellationToken cancellationToken = default);
    Task<Result> PatchAsync<TRequest>(string relativePath, TRequest request, CancellationToken cancellationToken = default);
}

public sealed class ApiClient : IApiClient
{

    private readonly HttpClient httpClient;


    public ApiClient(

        HttpClient httpClient

    )

    {

        this.httpClient = httpClient;

    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
        Converters =
        {
            new SafeBooleanConverter(),
            new SafeNullableBooleanConverter()
        }
    };

    public async Task<Result<TResponse>> GetAsync<TResponse>(string relativePath, CancellationToken cancellationToken = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient GET] {relativePath}");
            using var response = await httpClient.GetAsync(relativePath, cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient NETWORK ERROR] GET {relativePath}: {ex}");
            return Result<TResponse>.Failure(AppError.Network($"تعذر الاتصال بالخادم ({ex.Message}). تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient TIMEOUT] GET {relativePath}");
            return Result<TResponse>.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(
        string relativePath,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient POST] {relativePath}");
            using var response = await httpClient.PostAsJsonAsync(relativePath, request, JsonOptions, cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient NETWORK ERROR] POST {relativePath}: {ex}");
            return Result<TResponse>.Failure(AppError.Network($"تعذر الاتصال بالخادم ({ex.Message}). تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient TIMEOUT] POST {relativePath}");
            return Result<TResponse>.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result<TResponse>> PostEmptyAsync<TResponse>(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient POST EMPTY] {relativePath}");
            using var response = await httpClient.PostAsync(relativePath, content: null, cancellationToken: cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient NETWORK ERROR] POST {relativePath}: {ex}");
            return Result<TResponse>.Failure(AppError.Network($"تعذر الاتصال بالخادم ({ex.Message}). تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient TIMEOUT] POST {relativePath}");
            return Result<TResponse>.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result> PostAsync<TRequest>(string relativePath, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient POST] {relativePath}");
            using var response = await httpClient.PostAsJsonAsync(relativePath, request, JsonOptions, cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            System.Diagnostics.Debug.WriteLine($"[ApiClient] {(int)response.StatusCode} POST {relativePath} -> {rawContent}");
            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var error = ApiErrorMapper.Map(rawContent, response.StatusCode);
            System.Diagnostics.Debug.WriteLine($"[ApiClient API ERROR] {error.Kind}: {error.Message}");
            return Result.Failure(error);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient NETWORK ERROR] POST {relativePath}: {ex}");
            return Result.Failure(AppError.Network($"تعذر الاتصال بالخادم ({ex.Message}). تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient TIMEOUT] POST {relativePath}");
            return Result.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result<TResponse>> PostMultipartAsync<TResponse>(
        string relativePath,
        MultipartFormDataContent content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient POST MULTIPART] {relativePath}");
            using var response = await httpClient.PostAsync(relativePath, content, cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient NETWORK ERROR] POST {relativePath}: {ex}");
            return Result<TResponse>.Failure(AppError.Network($"تعذر الاتصال بالخادم ({ex.Message}). تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient TIMEOUT] POST {relativePath}");
            return Result<TResponse>.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result> DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient DELETE] {relativePath}");
            using var response = await httpClient.DeleteAsync(relativePath, cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            System.Diagnostics.Debug.WriteLine($"[ApiClient] {(int)response.StatusCode} DELETE {relativePath} -> {rawContent}");
            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var error = ApiErrorMapper.Map(rawContent, response.StatusCode);
            System.Diagnostics.Debug.WriteLine($"[ApiClient API ERROR] {error.Kind}: {error.Message}");
            return Result.Failure(error);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient NETWORK ERROR] DELETE {relativePath}: {ex}");
            return Result.Failure(AppError.Network($"تعذر الاتصال بالخادم ({ex.Message}). تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient TIMEOUT] DELETE {relativePath}");
            return Result.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result<TResponse>> PutAsync<TRequest, TResponse>(
        string relativePath,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient PUT] {relativePath}");
            using var response = await httpClient.PutAsJsonAsync(relativePath, request, JsonOptions, cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient NETWORK ERROR] PUT {relativePath}: {ex}");
            return Result<TResponse>.Failure(AppError.Network($"تعذر الاتصال بالخادم ({ex.Message}). تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient TIMEOUT] PUT {relativePath}");
            return Result<TResponse>.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result> PutAsync<TRequest>(string relativePath, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient PUT] {relativePath}");
            using var response = await httpClient.PutAsJsonAsync(relativePath, request, JsonOptions, cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            System.Diagnostics.Debug.WriteLine($"[ApiClient] {(int)response.StatusCode} PUT {relativePath} -> {rawContent}");
            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var error = ApiErrorMapper.Map(rawContent, response.StatusCode);
            System.Diagnostics.Debug.WriteLine($"[ApiClient API ERROR] {error.Kind}: {error.Message}");
            return Result.Failure(error);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient NETWORK ERROR] PUT {relativePath}: {ex}");
            return Result.Failure(AppError.Network($"تعذر الاتصال بالخادم ({ex.Message}). تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient TIMEOUT] PUT {relativePath}");
            return Result.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result<TResponse>> PatchAsync<TRequest, TResponse>(
        string relativePath,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient PATCH] {relativePath}");
            using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, relativePath)
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient NETWORK ERROR] PATCH {relativePath}: {ex}");
            return Result<TResponse>.Failure(AppError.Network($"تعذر الاتصال بالخادم ({ex.Message}). تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient TIMEOUT] PATCH {relativePath}");
            return Result<TResponse>.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result> PatchAsync<TRequest>(string relativePath, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient PATCH] {relativePath}");
            using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, relativePath)
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            System.Diagnostics.Debug.WriteLine($"[ApiClient] {(int)response.StatusCode} PATCH {relativePath} -> {rawContent}");
            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var error = ApiErrorMapper.Map(rawContent, response.StatusCode);
            System.Diagnostics.Debug.WriteLine($"[ApiClient API ERROR] {error.Kind}: {error.Message}");
            return Result.Failure(error);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient NETWORK ERROR] PATCH {relativePath}: {ex}");
            return Result.Failure(AppError.Network($"تعذر الاتصال بالخادم ({ex.Message}). تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient TIMEOUT] PATCH {relativePath}");
            return Result.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    private static async Task<Result<TResponse>> DeserializeAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        System.Diagnostics.Debug.WriteLine($"[ApiClient] {(int)response.StatusCode} {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri} -> {rawContent}");

        if (!response.IsSuccessStatusCode)
        {
            var error = ApiErrorMapper.Map(rawContent, response.StatusCode);
            System.Diagnostics.Debug.WriteLine($"[ApiClient API ERROR] {error.Kind}: {error.Message}");
            return Result<TResponse>.Failure(error);
        }

        try
        {
            var model = JsonSerializer.Deserialize<TResponse>(rawContent, JsonOptions);
            return model is null
                ? Result<TResponse>.Failure(new AppError(AppErrorKind.Unknown, "أعاد الخادم استجابة فارغة أو غير متوقعة."))
                : Result<TResponse>.Success(model);
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient JSON ERROR] Deserialization failed for {typeof(TResponse).Name}: {ex.Message}");
            return Result<TResponse>.Failure(new AppError(AppErrorKind.Unknown, $"أعاد الخادم استجابة غير متوقعة: {ex.Message}"));
        }
    }
}
