using AbsenceNotifier.Core.DTOs.Results.RocketChat;
using AbsenceNotifier.Core.DTOs.Users;

namespace AbsenceNotifier.Core.Helpers
{
    public static class RocketChatExtenstions
    {
        public static bool LoginToRocketChatFailed(this LoginToRocketResult? login)
        {
            return login == null || string.IsNullOrEmpty(login.Status)
                || login.Status.ToLower() != "success"
                || login.Data == null
                || !login.Success
                || string.IsNullOrEmpty(login.Data.AuthToken)
                || string.IsNullOrEmpty(login.Data.UserId);
        }

        public static bool IsRocketChatCreateRoomWithError(this CreateRoomResult? room)
        {
            return room == null || !room.Success || room.Room == null || string.IsNullOrEmpty(room.Room.Id);
        }

        public static RocketChatUser? GetRocketChatUserByTimeTastic(this List<RocketChatUser> users,
            TimetasticUser user)
        {
            return users.Find(x =>
                        (x.Name == user.FirstName + " " + user.Surname) ||
                         (x.Emails != null && x.Emails.Any(x => x.Address == user.Email)));
        }
    }
}
