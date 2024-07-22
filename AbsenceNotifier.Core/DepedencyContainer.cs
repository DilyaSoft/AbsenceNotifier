using AbsenceNotifier.Core.Configurations;
using AbsenceNotifier.Core.Constants;
using AbsenceNotifier.Core.Helpers;
using AbsenceNotifier.Core.Interfaces;
using AbsenceNotifier.Core.Interfaces.Messengers;
using AbsenceNotifier.Core.Interfaces.Messengers.Api;
using AbsenceNotifier.Core.Interfaces.TimeOffsService;
using AbsenceNotifier.Core.Interfaces.TimeOffsService.Api;
using AbsenceNotifier.Core.Services;
using AbsenceNotifier.Core.Services.Messengers;
using AbsenceNotifier.Core.Services.Rocket;
using AbsenceNotifier.Core.Services.TimeTastic;
using AbsenceNotifier.Core.Services.Yandex;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace AbsenceNotifier.Core
{
    public static class DependencyContainer
    {
        public static void AddWebApiServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<MessengerContext>();
            services.AddScoped<YandexMessenger>();
            services.AddScoped<YandexChatConfiguration>();
            services.AddScoped<RockectChatMessenger>();
            services.AddScoped<IRocketChatService, RocketChatService>();
            services.AddScoped<ITimeTasticService, TimeTasticService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IYandexApiService, YandexApiService>();
            services.AddScoped<ITimeOffHelper, TimeOffHelper>();
            services.AddScoped<ISavedChatsManager, SavedChatsManager>();
            services.AddScoped<IDataProtectionProviderHelper, DataProtectionProviderHelper>();

            var yandexConfig = configuration.GetSection("TimeTasticConfiguration")
                .GetSection("BaseTimeTasticUrl").Value;

            if (yandexConfig == null)
            {
                throw new ArgumentException("Yandex config was not added");
            }

            var yandexUrl = configuration.GetSection("YandexChatConfiguration").GetSection("BotApiUrl").Value;

            if (string.IsNullOrWhiteSpace(yandexUrl))
            {
                throw new ArgumentException("Yandex chat base url was not added");
            }

            var timetasticConfig = configuration.GetSection("TimeTasticConfiguration")
                .GetSection("BaseTimeTasticUrl").Value;
            if (timetasticConfig == null)
            {
                throw new ArgumentException("TimeTastic base url was not added");
            }
            var rocketChat = configuration.GetSection("RocketChatConfiguration").GetSection("BaseRocketUrl").Value;
            if (rocketChat == null)
            {
                throw new ArgumentException("Rocket chat base url was not added");
            }
            services.AddRefitClient<ITimeTasticApi>()
                .ConfigureHttpClient(b => b.BaseAddress = new Uri(timetasticConfig));
            services.AddRefitClient<IRocketChatApi>()
                .ConfigureHttpClient(b => b.BaseAddress = new Uri(rocketChat));
            services.AddRefitClient<IYandexChatApi>()
                .ConfigureHttpClient(b => b.BaseAddress = new Uri(yandexUrl));
        }

        public static void AddConsoleAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            var timetasticConfig = configuration.GetSection("TimeTasticConfiguration")
                .GetSection("BaseTimeTasticUrl").Value;
            if (timetasticConfig == null)
            {
                throw new ArgumentException("TimeTastic base url was not added");
            }
            var rocketChat = configuration.GetSection("RocketChatConfiguration")
                .GetSection("BaseRocketUrl").Value;
            if (rocketChat == null)
            {
                throw new ArgumentException("Rocket chat base url was not added");
            }
            var yandexConfig = configuration.GetSection("TimeTasticConfiguration")
               .GetSection("BaseTimeTasticUrl").Value;

            if (yandexConfig == null)
            {
                throw new ArgumentException("Yandex config was not added");
            }

            var yandexUrl = configuration.GetSection("YandexChatConfiguration").GetSection("BotApiUrl").Value;

            if (string.IsNullOrWhiteSpace(yandexUrl))
            {
                throw new ArgumentException("Yandex chat base url was not added");
            }

            services.AddScoped<MessengerContext>();
            services.AddScoped<YandexMessenger>();
            services.AddScoped<RockectChatMessenger>();
            services.AddScoped<YandexChatConfiguration>();

            services.AddScoped<IRocketChatService, RocketChatService>();
            services.AddScoped<IYandexApiService, YandexApiService>();
            services.AddScoped<ITimeOffHelper, TimeOffHelper>();
            services.AddScoped<ITimeTasticService, TimeTasticService>();
            services.AddScoped<ISavedChatsManager, SavedChatsManager>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IDataProtectionProviderHelper, DataProtectionProviderHelper>();
            services.AddRefitClient<ITimeTasticApi>()
                .ConfigureHttpClient(b => b.BaseAddress = new Uri(timetasticConfig));
            services.AddRefitClient<IRocketChatApi>()
                .ConfigureHttpClient(b => b.BaseAddress = new Uri(rocketChat));
            services.AddRefitClient<IYandexChatApi>()
                .ConfigureHttpClient(b => b.BaseAddress = new Uri(yandexUrl));
        }
    }
}
