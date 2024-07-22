using AbsenceNotifier.Core.DTOs.Results;
using AbsenceNotifier.Core.DTOs.Results.RocketChat;
using AbsenceNotifier.Core.DTOs.Rocket;
using AbsenceNotifier.Core.DTOs.TimeTastic;

namespace AbsenceNotifier.Core.Interfaces.Messengers
{
    public interface IRocketChatService
    {
        public Task<GetChannelInfoResult> GetChannelByName(string name, string authToken, string userId);
        public Task<GetAllRocketChatUsersResult> GetAllUsers(string authToken, string userId);
        public Task<LoginToRocketResult> SignIn(SignInRequest signInRequest);
        public Task<CreateRoomResult> CreateRoom(CreateRoomBody roomBody, string authToken, string userId);
        public Task<SendMessageRocketResult> SendMessageToRoom(SendMessageRequest sendMessage, string authToken,
            string userId);
        public Task<SendToGeneralChatResult> SendNotificationsToGeneralRoom(IEnumerable<TimeOff> timeOffs,
            string token, string userId);
        public Task<NotifyTimeOffResult> SendTimeOffsToChat(IEnumerable<TimeOff> timeOffs, string title,
            string channel, string token, string userId, bool isForGeneral);
     
    }
}
