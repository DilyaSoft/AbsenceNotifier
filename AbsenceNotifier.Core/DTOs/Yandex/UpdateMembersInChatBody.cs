using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Yandex
{
    public sealed class UpdateMembersInChatBody
    {
        [JsonPropertyName("chat_id")]
        public string? ChatId { get; set; }

        /// <summary>
        /// list users to add to chat
        /// </summary>
        public YandexUser[]? Members { get; set; }
    }
}
