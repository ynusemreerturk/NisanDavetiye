using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NisanDavetiye.BLL.Options;

namespace NisanDavetiye.BLL.Services;

public class RecaptchaCaptchaService : ICaptchaService
{
    private readonly HttpClient _http;
    private readonly RecaptchaOptions _options;

    public RecaptchaCaptchaService(HttpClient http, IOptions<RecaptchaOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task ValidateOrThrowAsync(
        string? token,
        string? remoteIp,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return;

        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new InvalidOperationException("CAPTCHA yapılandırması eksik.");

        if (string.IsNullOrWhiteSpace(token) || token is "disabled")
            throw new ArgumentException("CAPTCHA doğrulaması gerekli.");

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"] = _options.SecretKey,
            ["response"] = token,
            ["remoteip"] = remoteIp ?? string.Empty,
        });

        using var response = await _http.PostAsync(
            "https://www.google.com/recaptcha/api/siteverify",
            content,
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<RecaptchaVerifyResponse>(cancellationToken);
        if (result?.Success != true)
            throw new ArgumentException("CAPTCHA doğrulaması başarısız. Lütfen tekrar deneyin.");
    }

    private sealed class RecaptchaVerifyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
