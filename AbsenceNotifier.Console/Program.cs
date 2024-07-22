using AbsenceNotifier.Core;
using AbsenceNotifier.Core.Configurations;
using AbsenceNotifier.Core.DTOs.Results;
using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.Interfaces.TimeOffsService;
using AbsenceNotifier.Core.Services.Messengers;
using AbsenceNotifier.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace AbsenceNotifier.Console
{
    public static class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args).ConfigureAppConfiguration((hostContext, config) =>
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (env == "Development")
                {
                    var pathBin = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.FullName;
                    if (pathBin != null)
                    {
                        var pathDirectory = Directory.GetParent(pathBin)?.Parent?.FullName;
                        config.SetBasePath(pathDirectory + "\\AbsenceNotifier").AddJsonFile("appsettings.json",
                            optional: false);
                    }
                }
                else
                {
                    var current = Directory.GetCurrentDirectory();
                    config.SetBasePath(current).AddJsonFile("appsettings.json", optional: false);
                }
            })
            .ConfigureServices(async (hostContext, services) =>
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .WriteTo.Console()
                    .WriteTo.File("Jobs-Console/AbsenceNotifier.Jobs.Log")
                        .CreateLogger();

                var configuration = hostContext.Configuration;
                services.AddConsoleAppServices(configuration);
                services.Configure<ApplicationCommonConfiguration>(configuration.GetSection("ApplicationCommonConfiguration"));
                services.Configure<RocketChatConfiguration>(configuration.GetSection("RocketChatConfiguration"));
                services.Configure<SmtpConfiguration>(configuration.GetSection("SmtpConfiguration"));
                services.Configure<YandexChatConfiguration>(configuration.GetSection("YandexChatConfiguration"));

                var provider = services.BuildServiceProvider();
                var messengerCxt = provider.GetRequiredService<MessengerContext>();
                var timeTasticService = provider.GetRequiredService<ITimeTasticService>();

                messengerCxt.SetMessenger(ApplicationSettings.CurrentMessengerName);
                var allHolidaysForAllTime = await timeTasticService.GetForAllTimeHolidays(TimeTasticSettings.AuthHeader);
                if (!allHolidaysForAllTime.Success)
                {
                    Log.Logger.Error(allHolidaysForAllTime.ErrorMessage ?? TimeOffListResponse.DefaultErrorMessage);
                    return;
                }
                if (allHolidaysForAllTime.Holidays == null || allHolidaysForAllTime.Holidays.Length == 0)
                {
                    Log.Logger.Error(TimeOffListResponse.NotFoundMessage);
                    return;
                }
                var sw = new Stopwatch();
                sw.Start();
                Log.Logger.Information("SendTimeOffRequests in Program | Starts");
                var resultSendRequest = await messengerCxt.RunSendingRequestsToManagers(allHolidaysForAllTime.Holidays);
                if (resultSendRequest.Success)
                {
                    Log.Logger.Information(SendToManagersResult.DefaultSuccessMessage);
                }
                else
                {
                    Log.Logger.Error(resultSendRequest.Message ?? SendToManagersResult.DefaultErrorMessage);
                }
                Log.Logger.Information($"SendTimeOffRequests in Program, took {sw.Elapsed} | Ends");
                sw.Reset();
                sw.Start();
                Log.Logger.Information("SendTimeOffToGeneral in Program | Starts");
                var sendToGeneral = await messengerCxt.RunSendingApprovedTimeOffsToGeneralRoom(allHolidaysForAllTime.Holidays);
                if (sendToGeneral.Success)
                {
                    Log.Logger.Information(SendToGeneralChatResult.DefaultSuccessSendMessage);
                }
                else
                {
                    Log.Logger.Error(sendToGeneral.Message ?? SendToGeneralChatResult.DefaultErrorSendMessage);
                }
                Log.Logger.Information($"SendTimeOffToGeneral in Program, took {sw.Elapsed} | Ends");
                sw.Stop();

                Environment.Exit(0);
            })
            .RunConsoleAsync();

        }
    }
}
