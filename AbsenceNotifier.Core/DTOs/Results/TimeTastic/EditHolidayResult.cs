using AbsenceNotifier.Core.DTOs.Results.Base;

namespace AbsenceNotifier.Core.DTOs.Results.TimeTastic
{

    public class EditHolidayResult : BaseResult
    {
        public string? ApprovalResult { get; set; }
        public const string NotFoundUsersMessage = "Users in general chat were not found!";
        public const string DefaultErrorMessage = "Cannot send approved to manager";
        public const string EmptyUserEmailOrNotValid = "Timetastic user email is empty or not valid!";
        public const string AlreadyActioned = "The request has already been rejected or has been confirmed";
    }
}
