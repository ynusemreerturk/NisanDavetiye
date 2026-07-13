using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NisanDavetiye.BLL.Options;
using NisanDavetiye.BLL.Services;

namespace NisanDavetiye.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBll(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GaleriStorageOptions>(configuration.GetSection(GaleriStorageOptions.SectionName));
        services.Configure<TurnstileOptions>(configuration.GetSection(TurnstileOptions.SectionName));
        services.Configure<MediaSigningOptions>(configuration.GetSection(MediaSigningOptions.SectionName));

        services.AddHttpClient<ICaptchaService, TurnstileCaptchaService>();

        services.AddScoped<IDavetiyeService, DavetiyeService>();
        services.AddScoped<IInviteAuthService, InviteAuthService>();
        services.AddScoped<IPanelAuthService, PanelAuthService>();
        services.AddScoped<IRsvpService, RsvpService>();
        services.AddScoped<IGaleriService, GaleriService>();
        services.AddSingleton<IMediaUrlSigner, MediaUrlSigner>();
        return services;
    }
}
