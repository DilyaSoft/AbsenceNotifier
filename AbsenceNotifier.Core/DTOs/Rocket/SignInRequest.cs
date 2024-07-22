namespace AbsenceNotifier.Core.DTOs.Rocket
{
    public class SignInRequest
    {
        public string? User { get; set; }
        public string? Password { get; set; }
        public SignInRequest()
        {
        }
        public SignInRequest(string user, string password)
        {
            User = user;
            Password = password;
        }
    }
}
