using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Rocket
{
    public class CreateRoomBody
    {
        [JsonPropertyName("username")]
        public string? UserName { get; set; }
    }
}
