using System.Text.Json.Serialization;

namespace AbsenceNotifier.Core.DTOs.Yandex
{
    public sealed class YandexUser
    {
        public YandexUser(string login) 
        {
            Login = login;
        }

        public YandexUser()
        {

        }

        [JsonPropertyName("login")]
        public string? Login { get; set; }
        public string? Team { get; set; }
    }
}
