using AbsenceNotifier.Core.DTOs.Results.RocketChat;
using AbsenceNotifier.Core.DTOs.Rocket;
using Refit;

namespace AbsenceNotifier.Core.Interfaces.Messengers.Api
{
    public interface IRocketChatApi
    {
        [Post("/api/v1/login")]
        public Task<LoginToRocketResult> SignIn([Body] SignInRequest signIn);

        [Post("/api/v1/im.create")]
        public Task<CreateRoomResult> CreateRoom([Body] CreateRoomBody createRoom,
            [Header("Content-Type")] string contentType,
            [Header("X-Auth-Token")] string authToken,
            [Header("X-User-Id")] string userId);

        [Post("/api/v1/chat.sendMessage")]
        public Task<SendMessageRocketResult> SendMessageToChannel([Body] SendMessageRequest message,
            [Header("X-Auth-Token")] string authToken,
            [Header("X-User-Id")] string userId);

        [Get("/api/v1/channels.info?roomName={roomName}")]
        public Task<GetChannelInfoResult> GetChannelByName([AliasAs("roomName")] string channelName,
             [Header("X-Auth-Token")] string authToken,
             [Header("X-User-Id")] string userId);

        [Get("/api/v1/users.list")]
        public Task<GetAllRocketChatUsersResult> GetAllUsers([Header("X-Auth-Token")] string authToken,
            [Header("X-User-Id")] string userId);
    }
}
