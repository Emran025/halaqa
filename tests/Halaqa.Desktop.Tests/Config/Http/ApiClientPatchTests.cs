using System.Net;
using System.Text;
using System.Text.Json;
using Halaqa.Desktop.Config.Http;
using Xunit;

namespace Halaqa.Desktop.Tests.Config.Http;

public sealed class ApiClientPatchTests
{
    [Fact]
    public async Task PatchAsync_SendsJsonPatchRequestAndDeserializesSuccessfulResponse()
    {
        var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/api/v1/")
        };
        var apiClient = new ApiClient(httpClient);

        var result = await apiClient.PatchAsync<ProfilePatchRequest, ProfilePatchResponse>(
            "me",
            new ProfilePatchRequest("طالب اختبار"));

        Assert.True(result.IsSuccess);
        Assert.Equal("updated", result.Value?.Status);
        Assert.Equal(HttpMethod.Patch, handler.Method);
        Assert.Equal("/api/v1/me", handler.PathAndQuery);
        using var payload = JsonDocument.Parse(handler.Body!);
        Assert.Equal("طالب اختبار", payload.RootElement.GetProperty("name").GetString());
        Assert.Equal("application/json", handler.ContentType);
    }

    private sealed record ProfilePatchRequest(string Name);
    private sealed record ProfilePatchResponse(string Status);

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public string? PathAndQuery { get; private set; }
        public string? Body { get; private set; }
        public string? ContentType { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            PathAndQuery = request.RequestUri?.PathAndQuery;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"updated\"}", Encoding.UTF8, "application/json")
            };
        }
    }
}
