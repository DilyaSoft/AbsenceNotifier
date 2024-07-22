using AbsenceNotifier.Core;
using AbsenceNotifier.Core.Configurations;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AbsenceNotifier.AzureFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.GetContext().Configuration;

            services.AddConsoleAppServices(configuration);
            services.Configure<ApplicationCommonConfiguration>(configuration.GetSection("ApplicationCommonConfiguration"));
            services.Configure<RocketChatConfiguration>(configuration.GetSection("RocketChatConfiguration"));
            services.Configure<SmtpConfiguration>(configuration.GetSection("SmtpConfiguration"));
            services.Configure<YandexChatConfiguration>(configuration.GetSection("YandexChatConfiguration"));
        }
    }
}
