using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Yandex.Responses
{
    public sealed class SendTextMessageResponse
    {
        public const string DefaultError = "Error during sending text message to yandex api";
        [JsonPropertyName("message_id")]
        public long MessageId { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }
        public bool Ok { get; set; }
    }
}
