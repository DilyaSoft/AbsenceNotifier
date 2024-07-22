using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.TimeTastic;

namespace AbsenceNotifier.Core.DTOs.Results.RocketChat
{
    public sealed class GroupedTimeOffsByRocketManagerResult : BaseResult
    {
        public const string DefaultError = "Not found managers to send time offs to rocket chat!";
        public IEnumerable<(string userNameToSend, string emailToSend, List<TimeOff> times)>? DataResult { get; set; }
    }
}
