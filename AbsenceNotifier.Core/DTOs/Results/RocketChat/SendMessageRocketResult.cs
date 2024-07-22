using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.Rocket;
using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Results.RocketChat
{
    public class SendMessageRocketResult : BaseResult
    {
        public const string DefaultError = "Cannot send message";
        public string? ErrorMessage { get; set; }
        [JsonPropertyName("message")]
        public Message? MessageToSend { get; set; }
    }
}
