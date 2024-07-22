namespace AbsenceNotifier.Core.DTOs.TimeTastic
{
    public class TimeOffListResponse
    {
        public const string DefaultErrorMessage = "Get all timetastic holidays unknown error";
        public const string NotFoundMessage = "Timetastic service works but holidays not found";
        public TimeOff[]? Holidays { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
