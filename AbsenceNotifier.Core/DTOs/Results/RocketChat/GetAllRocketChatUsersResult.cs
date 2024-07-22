using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.Users;

namespace AbsenceNotifier.Core.DTOs.Results.RocketChat
{
    public class GetAllRocketChatUsersResult : BaseResult
    {
        public const string UserNotFound = "Rocket chat users not found";
        public List<RocketChatUser>? Users { get; set; }
    }
}
