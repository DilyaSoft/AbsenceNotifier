using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Yandex.Responses
{
    public sealed class CreateChatResponse
    {
        public const string DefaultError = "Something went wrong in creation yandex chat!";
        [JsonPropertyName("chat_id")]
        public string? ChatId { get; set; }
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }
    }
}
