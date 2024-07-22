using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Rocket
{
    public class Room
    {
        [JsonPropertyName("rid")]
        public string? Id { get; set; }
    }
}
