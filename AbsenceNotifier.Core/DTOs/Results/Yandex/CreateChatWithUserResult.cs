using AbsenceNotifier.Core.DTOs.Results.Base;

namespace AbsenceNotifier.Core.DTOs.Results.Yandex
{
    public sealed class CreateChatWithUserResult : BaseResult
    {
        public string? ChatId { get; set; }
    }
}
