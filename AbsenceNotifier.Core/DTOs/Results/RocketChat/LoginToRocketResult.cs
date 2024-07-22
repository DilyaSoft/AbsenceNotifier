using AbsenceNotifier.Core.DTOs.Results.Base;
using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Results.RocketChat
{
    public class LoginToRocketResult : BaseResult
    {
        public const string DefaultError = "Error during login to rocket chat with given credentials";
        public string? Status { get; set; }

        [JsonPropertyName("Data")]
        public RocketChatLoginResult? Data { get; set; }
    }
}
