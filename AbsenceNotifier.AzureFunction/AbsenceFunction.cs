using AbsenceNotifier.Core.Interfaces.TimeOffsService;
using AbsenceNotifier.Core.Services.Messengers;
using AbsenceNotifier.Core.Settings;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AbsenceNotifier.AzureFunction
{
    public class AbsenceFunction
    {

        private readonly MessengerContext _messengerContext;
        private readonly ITimeTasticService _timeTasticService;

        public AbsenceFunction(MessengerContext messengerContext, ITimeTasticService timeTasticService)
        {
            _messengerContext = messengerContext;
            _timeTasticService = timeTasticService;
        }

        /// <summary>
        /// before running it needs to add list off env variables
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("AbsenceFunction")]
        public async Task Run([TimerTrigger("17 18 * * *")] TimerInfo myTimer, ILogger log)
        {
            _messengerContext.SetMessenger(ApplicationSettings.CurrentMessengerName);
            var allHolidaysForAllTime = await _timeTasticService.GetForAllTimeHolidays(TimeTasticSettings.AuthHeader);
            if (!allHolidaysForAllTime.Success)
            {
                log.LogError(allHolidaysForAllTime.ErrorMessage ?? "Get all timetastic holidays unknown error");
                return;
            }
            if (allHolidaysForAllTime.Holidays == null || allHolidaysForAllTime.Holidays.Length == 0)
            {
                log.LogError("Timetastic service works but holidays not found");
                return;
            }
            var sw = new Stopwatch();
            sw.Start();
            log.LogInformation("SendTimeOffRequests in Program | Starts");
            var resultSendRequest = await _messengerContext.RunSendingRequestsToManagers(allHolidaysForAllTime.Holidays);
            if (resultSendRequest.Success)
            {
                log.LogInformation("SendTimeOffRequests were checked successfully");
            }
            else
            {
                log.LogError(resultSendRequest.Message ?? "SendTimeOffRequests unknown error");
            }
            log.LogInformation($"SendTimeOffRequests in Program, took {sw.Elapsed} | Ends");
            sw.Reset();
            sw.Start();
            log.LogInformation("SendTimeOffToGeneral in Program | Starts");
            var sendToGeneral = await _messengerContext.RunSendingApprovedTimeOffsToGeneralRoom(allHolidaysForAllTime.Holidays);
            if (sendToGeneral.Success)
            {
                log.LogInformation("SendTimeOffToGeneral were checked successfully");
            }
            else
            {
                log.LogError(sendToGeneral.Message ?? "SendTimeOffToGeneral unknown error");
            }
            log.LogInformation($"SendTimeOffToGeneral in Program, took {sw.Elapsed} | Ends");
            sw.Stop();
        }
    }
}
