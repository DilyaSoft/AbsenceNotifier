using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.TimeTastic;

namespace AbsenceNotifier.Core.DTOs.Results.Yandex
{
    public sealed class GroupedTimeOffsByYandexManagerResult : BaseResult
    {
        public IEnumerable<(string userNameToSend, string emailToSend, 
            string chatId, List<TimeOff> times)>? DataResult { get; set; }

        public const string ErrorInGroupingResult = "Grouped time offs were not found";
    }
}
