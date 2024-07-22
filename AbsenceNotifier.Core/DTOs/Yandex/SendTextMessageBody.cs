using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Yandex
{
    public sealed class SendTextMessageBody
    {
        [JsonPropertyName("chat_id")]
        public string? ChatId { get; set; }
        public string? Text { get; set; }

        [JsonPropertyName("disable_web_page_preview")]
        public bool DisableWebPagePreview { get; set; }

        [JsonPropertyName("disable_notification")]
        public bool DisableNotification { get; set; }
    }
}
