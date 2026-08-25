using System.Net;
using System.Text.Json;
using Halaqa.Desktop.Shared.Domain.Common;

namespace Halaqa.Desktop.Config.Http;

public static class ApiErrorMapper
{
    public static async Task<AppError> MapAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        var parsed = TryParse(payload);
        var message = parsed?.Message ?? DefaultMessage(response.StatusCode);
        var kind = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => AppErrorKind.Unauthorized,
            HttpStatusCode.Forbidden => AppErrorKind.Forbidden,
            HttpStatusCode.NotFound => AppErrorKind.NotFound,
            HttpStatusCode.Conflict => AppErrorKind.Conflict,
            HttpStatusCode.UnprocessableEntity => AppErrorKind.Validation,
            _ when (int)response.StatusCode >= 500 => AppErrorKind.Server,
            _ => AppErrorKind.Unknown
        };

        return new AppError(kind, message, parsed?.FieldErrors, parsed?.Code);
    }

    private static ErrorPayload? TryParse(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var error = root.TryGetProperty("error", out var errorNode) ? errorNode : root;
            var message = root.TryGetProperty("message", out var messageNode)
                ? messageNode.GetString()
                : error.TryGetProperty("message", out var errorMessageNode) ? errorMessageNode.GetString() : null;
            var code = error.TryGetProperty("code", out var codeNode) ? codeNode.GetString() : null;
            var fieldErrors = new List<FieldError>();

            if (root.TryGetProperty("field_errors", out var fieldErrorsNode) && fieldErrorsNode.ValueKind == JsonValueKind.Array)
            {
                foreach (var fieldErrorNode in fieldErrorsNode.EnumerateArray())
                {
                    if (!fieldErrorNode.TryGetProperty("field", out var fieldNode))
                    {
                        continue;
                    }

                    var messages = fieldErrorNode.TryGetProperty("messages", out var messagesNode) && messagesNode.ValueKind == JsonValueKind.Array
                        ? messagesNode.EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToArray()
                        : Array.Empty<string>();
                    fieldErrors.Add(new FieldError(fieldNode.GetString() ?? string.Empty, messages));
                }
            }

            return new ErrorPayload(message, code, fieldErrors);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string DefaultMessage(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized => "انتهت صلاحية الجلسة أو يلزم تسجيل الدخول.",
        HttpStatusCode.Forbidden => "لا تملك الصلاحية اللازمة لهذه العملية.",
        HttpStatusCode.NotFound => "العنصر المطلوب غير متاح.",
        HttpStatusCode.Conflict => "تعارضت العملية مع الحالة الحالية للبيانات.",
        HttpStatusCode.UnprocessableEntity => "يرجى مراجعة الحقول المطلوبة.",
        _ => "تعذر إتمام الطلب حالياً."
    };

    private sealed record ErrorPayload(string? Message, string? Code, IReadOnlyList<FieldError> FieldErrors);
}
