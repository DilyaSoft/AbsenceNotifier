using AbsenceNotifier.Core.Enums;

namespace AbsenceNotifier.Core.DTOs.TimeTastic
{
    public class TimeOff
    {
        public const string EmptyTimeOffMessage = "Time off message is empty";
        public long Id { get; set; }

        public List<TimeOff> Sequences {  get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? UserName { get; set; }
        public string? LeaveType { get; set; }
        public TimeOffType TimeOffType { get; set; }
        public string? Reason { get; set; }
        public RequestStatus Status { get; set; }
        public long UserId { get; set; }
    }
}
