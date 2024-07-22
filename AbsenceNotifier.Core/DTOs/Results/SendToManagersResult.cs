using AbsenceNotifier.Core.DTOs.Results.Base;

namespace AbsenceNotifier.Core.DTOs.Results
{
    public class SendToManagersResult : BaseResult
    {
        public const string DefaultErrorMessage = "Something went wrong on server side during sending requests";
        public const string DefaultSuccessMessage = "SendTimeOffRequests were checked successfully";
    }
}
