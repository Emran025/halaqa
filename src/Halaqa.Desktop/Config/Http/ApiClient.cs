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
        PropertyNameCaseInsensitive = false
    };

    public async Task<Result<TResponse>> GetAsync<TResponse>(string relativePath, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(relativePath, cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Result<TResponse>.Failure(AppError.Network("تعذر الاتصال بالخادم. تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
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
            using var response = await httpClient.PostAsJsonAsync(relativePath, request, JsonOptions, cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Result<TResponse>.Failure(AppError.Network("تعذر الاتصال بالخادم. تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<TResponse>.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result<TResponse>> PostEmptyAsync<TResponse>(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.PostAsync(relativePath, content: null, cancellationToken: cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Result<TResponse>.Failure(AppError.Network("تعذر الاتصال بالخادم. تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<TResponse>.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result> PostAsync<TRequest>(string relativePath, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(relativePath, request, JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            return Result.Failure(await ApiErrorMapper.MapAsync(response, cancellationToken));
        }
        catch (HttpRequestException)
        {
            return Result.Failure(AppError.Network("تعذر الاتصال بالخادم. تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
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
            using var response = await httpClient.PostAsync(relativePath, content, cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Result<TResponse>.Failure(AppError.Network("تعذر الاتصال بالخادم. تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<TResponse>.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result> DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.DeleteAsync(relativePath, cancellationToken);
            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(await ApiErrorMapper.MapAsync(response, cancellationToken));
        }
        catch (HttpRequestException)
        {
            return Result.Failure(AppError.Network("تعذر الاتصال بالخادم. تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
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
            using var response = await httpClient.PutAsJsonAsync(relativePath, request, JsonOptions, cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Result<TResponse>.Failure(AppError.Network("تعذر الاتصال بالخادم. تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<TResponse>.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result> PutAsync<TRequest>(string relativePath, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.PutAsJsonAsync(relativePath, request, JsonOptions, cancellationToken);
            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(await ApiErrorMapper.MapAsync(response, cancellationToken));
        }
        catch (HttpRequestException)
        {
            return Result.Failure(AppError.Network("تعذر الاتصال بالخادم. تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
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
            using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, relativePath)
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
            return await DeserializeAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Result<TResponse>.Failure(AppError.Network("تعذر الاتصال بالخادم. تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<TResponse>.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    public async Task<Result> PatchAsync<TRequest>(string relativePath, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, relativePath)
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(await ApiErrorMapper.MapAsync(response, cancellationToken));
        }
        catch (HttpRequestException)
        {
            return Result.Failure(AppError.Network("تعذر الاتصال بالخادم. تحقق من الشبكة ثم أعد المحاولة."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Failure(AppError.Network("انتهت مهلة الاتصال بالخادم."));
        }
    }

    private static async Task<Result<TResponse>> DeserializeAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            return Result<TResponse>.Failure(await ApiErrorMapper.MapAsync(response, cancellationToken));
        }

        try
        {
            var model = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
            return model is null
                ? Result<TResponse>.Failure(new AppError(AppErrorKind.Unknown, "أعاد الخادم استجابة فارغة أو غير متوقعة."))
                : Result<TResponse>.Success(model);
        }
        catch (JsonException)
        {
            return Result<TResponse>.Failure(new AppError(AppErrorKind.Unknown, "أعاد الخادم استجابة غير متوقعة."));
        }
    }
}
