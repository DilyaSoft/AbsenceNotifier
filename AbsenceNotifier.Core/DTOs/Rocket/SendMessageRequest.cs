using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Rocket
{
    public class SendMessageRequest
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }
}
