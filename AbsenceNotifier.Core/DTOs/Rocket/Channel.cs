using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Rocket
{
    public class Channel
    {
        [JsonPropertyName("_id")]
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
}
