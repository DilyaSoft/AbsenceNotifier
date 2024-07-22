using AbsenceNotifier.Core.DTOs.Results.Base;

namespace AbsenceNotifier.Core.DTOs.Results
{
    public class SendToGeneralChatResult : BaseResult
    {
        public const string DefaultSuccessSendMessage = "Send time-Off to general were checked successfully";
        public const string DefaultErrorSendMessage = "Send Time-off to general unknown error";

    }
}
