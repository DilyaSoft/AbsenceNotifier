using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.TimeTastic;

namespace AbsenceNotifier.Core.DTOs.Results.TimeTastic
{
    public class GetTimeOffResult : BaseResult
    {
        public const string DefaultError = "Cannot get the time off";
        public const string LeaveTypeNotValidError = "Leave type of time off was null or not valid";
        public TimeOff? TimeOff { get; set; }
    }
}
