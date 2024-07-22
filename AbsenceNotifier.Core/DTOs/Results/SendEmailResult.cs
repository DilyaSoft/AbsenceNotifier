using AbsenceNotifier.Core.DTOs.Results.Base;

namespace AbsenceNotifier.Core.DTO.Results
{
    public class SendEmailResult : BaseResult
    {
        public const string DefaultErrorMessage = "Error during sending message to email";
        public const string NotValidEmailMessage = "Not valid provided email address";
    }
}
