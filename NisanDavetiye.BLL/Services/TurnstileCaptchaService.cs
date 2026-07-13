using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NisanDavetiye.BLL.Options;

namespace NisanDavetiye.BLL.Services;

public class TurnstileCaptchaService : ICaptchaService
{
    private readonly HttpClient _http;
    private readonly TurnstileOptions _options;

    public TurnstileCaptchaService(HttpClient http, IOptions<TurnstileOptions> options)
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

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("CAPTCHA doğrulaması gerekli.");

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"] = _options.SecretKey,
            ["response"] = token,
            ["remoteip"] = remoteIp ?? string.Empty,
        });

        using var response = await _http.PostAsync(
            "https://challenges.cloudflare.com/turnstile/v0/siteverify",
            content,
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<TurnstileVerifyResponse>(cancellationToken);
        if (result?.Success != true)
            throw new ArgumentException("CAPTCHA doğrulaması başarısız. Lütfen tekrar deneyin.");
    }

    private sealed class TurnstileVerifyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
