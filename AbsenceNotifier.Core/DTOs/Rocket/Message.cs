using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Rocket
{
    public class Message
    {
        [JsonPropertyName("rid")]
        public string? ChannelId { get; set; }
        [JsonPropertyName("msg")]
        public string? SendingMessage { get; set; }
        public string? Text { get; set; }
        public List<Attachment>? Attachments { get; set; }
    }
}
