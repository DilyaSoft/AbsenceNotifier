using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Users
{
    public class TimetasticUser
    {
        public long Id { get; set; }
        public string? DepartmentName { get; set; }

        [JsonPropertyName("firstname")]
        public string? FirstName { get; set; }

        [JsonPropertyName("surname")]
        public string? Surname { get; set; }

        public string? Email { get; set; }
    }
}
