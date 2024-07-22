using AbsenceNotifier.Core;
using Microsoft.Extensions.Hosting;
using Serilog.Events;
using Serilog;
using AbsenceNotifier.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using AbsenceNotifier.Core.Interfaces.TimeOffsService;
using AbsenceNotifier.Core.Services.Messengers;
using AbsenceNotifier.Core.Settings;
using System.Diagnostics;
using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.DTOs.Results;

namespace AbsenceNotifier.YandexFunction
{
    public class Handler
    {
        public async Task<Response> FunctionHandler(Request r)
        {
            Console.Write("Request ID :" + r.request_id + "\n");
            Console.Write("Request source:" + r.source + "\n");
            Console.Write("Version: " + r.version_id + "\n");

            var tokenSource = new CancellationTokenSource();
            var cancelToken = tokenSource.Token;
            var successRun = true;
            var messageRun = "";

            var runAppTask = Task.Run(() => Host.CreateDefaultBuilder([]).ConfigureAppConfiguration((hostContext, config) =>
                {}).ConfigureServices(async (hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services.AddJobServices(configuration);
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
                        Console.WriteLine(allHolidaysForAllTime.ErrorMessage ?? TimeOffListResponse.DefaultErrorMessage);
                        successRun = false;
                        messageRun = allHolidaysForAllTime.ErrorMessage ?? TimeOffListResponse.DefaultErrorMessage;
                        return;
                    }
                    if (allHolidaysForAllTime.Holidays == null || allHolidaysForAllTime.Holidays.Length == 0)
                    {
                        Console.WriteLine(TimeOffListResponse.NotFoundMessage);
                        successRun = false;
                        messageRun = allHolidaysForAllTime.ErrorMessage ?? TimeOffListResponse.NotFoundMessage;
                        return;
                    }
                    var sw = new Stopwatch();
                    sw.Start();
                    Console.WriteLine("Send TimeOff Requests in Program | Starts");
                    var resultSendRequest = await messengerCxt.RunSendingRequestsToManagers(allHolidaysForAllTime.Holidays);
                    if (resultSendRequest.Success)
                    {
                        Console.WriteLine(SendToManagersResult.DefaultSuccessMessage);
                        successRun = true;
                        messageRun = $"{SendToManagersResult.DefaultSuccessMessage} \n";
                    }
                    else
                    {
                        Console.WriteLine(resultSendRequest.Message ?? SendToManagersResult.DefaultErrorMessage);
                        successRun = false;
                        messageRun = resultSendRequest.Message ?? SendToManagersResult.DefaultErrorMessage;
                    }
                    Console.WriteLine($"Send TimeOff Requests in Program, took {sw.Elapsed} | Ends");
                    sw.Reset();
                    sw.Start();
                    Console.WriteLine("Send TimeOff To General in Program | Starts");
                    var sendToGeneral = await messengerCxt.RunSendingApprovedTimeOffsToGeneralRoom(allHolidaysForAllTime.Holidays);
                    if (sendToGeneral.Success)
                    {
                        Console.WriteLine(SendToGeneralChatResult.DefaultSuccessSendMessage);
                        successRun = true;
                        messageRun += SendToGeneralChatResult.DefaultSuccessSendMessage;
                    }
                    else
                    {
                        Console.WriteLine(sendToGeneral.Message ?? SendToGeneralChatResult.DefaultErrorSendMessage);
                        successRun= false;
                        messageRun = sendToGeneral.Message ?? SendToGeneralChatResult.DefaultErrorSendMessage;
                    }
                    Console.WriteLine($"Send TimeOff To General in Program, took {sw.Elapsed} | Ends");
                    sw.Stop();
                    tokenSource.Cancel();
                })
            .RunConsoleAsync(tokenSource.Token), tokenSource.Token);

            await runAppTask;

            return new Response()
            {
                body = string.IsNullOrWhiteSpace(messageRun) ? "Request was received and will be proccessed" : messageRun,
                statusCode = successRun ? 200 : 500,
                isBase64Encoded = false,
            };
        }
    }

    public class Request
    {
        public string? request_id { get; set; }
        public string? source { get; set; }
        public string? version_id { get; set; }
    }

    public class Response
    {
        public int statusCode { get; set; }
        public Dictionary<string, string>? headers { get; set; }
        public Dictionary<string, string>? multiValueHeaders { get; set; }
        public string? body { get; set; }
        public bool isBase64Encoded { get; set; }
    }
}
