using AbsenceNotifier.Core.DTOs.Yandex;

namespace AbsenceNotifier.Core.Configurations
{
    public sealed class YandexChatConfiguration
    {
        public const string GeneralChatUsersNotFoundError = "Users for general chat was not provided";
        public YandexUser[]? Managers { get; set; }
        public YandexUser[]? UserInGeneralChat {  get; set; }
        public string? ChatBotLogin { get; set; }
        public string GeneralChatId { 
            get 
            {
                var chatID = Environment.GetEnvironmentVariable("GeneralChatId");
                if (string.IsNullOrWhiteSpace(chatID))
                {
                    throw new ArgumentException("General yandex chat id was not provided");
                }
                return chatID;
            }
        }
    }
}
