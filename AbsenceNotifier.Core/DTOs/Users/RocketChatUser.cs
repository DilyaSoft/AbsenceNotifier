using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Users
{
    public class RocketChatUser
    {
        public const string UserWasNotFoundInTimeTastic = "Cannot find a rocket chat user with provided email, firstname, lastname of timetastic user";

        [JsonPropertyName("_id")]
        public string? Id { get; set; }
        [JsonPropertyName("username")]
        public string? UserName { get; set; }
        public IEnumerable<RocketChatUserEmail>? Emails { get; set; }
        public string? Name { get; set; }
    }
}
