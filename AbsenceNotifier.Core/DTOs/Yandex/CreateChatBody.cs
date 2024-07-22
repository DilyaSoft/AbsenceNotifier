using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Yandex
{
    public sealed class CreateChatBody
    {
        [JsonPropertyName("name")]
        public string? ChatName { get; set; }
        [JsonPropertyName("description")]
        public string? ChatDescription { get; set; }
        [JsonPropertyName("members")]
        public YandexUser[]? ChatMembers { get; set; }
    }
}
