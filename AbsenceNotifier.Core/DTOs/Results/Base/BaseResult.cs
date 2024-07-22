using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Results.Base
{
    public abstract class BaseResult
    {
        [JsonIgnore]
        public bool Success { get; set; }
        [JsonIgnore]
        public string? Message { get; set; }
    }
}
