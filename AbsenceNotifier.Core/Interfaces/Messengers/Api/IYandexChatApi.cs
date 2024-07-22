using AbsenceNotifier.Core.DTOs.Yandex;
using AbsenceNotifier.Core.DTOs.Yandex.Responses;
using Refit;

namespace AbsenceNotifier.Core.Interfaces.Messengers.Api
{
    public interface IYandexChatApi
    {
        [Post("/bot/v1/chats/create/")]
        public Task<CreateChatResponse> CreateYandexChat([Body] CreateChatBody body,
            [Header("Content-Type")] string contentType,
            [Header("Authorization")] string authToken);

        [Post("/bot/v1/messages/sendText")]
        public Task<SendTextMessageResponse> SendTextMessageToChat([Body] SendTextMessageBody body,
            [Header("Content-Type")] string contentType,
            [Header("Authorization")] string authToken);

        [Post("/bot/v1/chats/updateMembers")]
        public Task<UpdateMembersInChatResponse> UpdateMembersInChat([Body] UpdateMembersInChatBody body,
            [Header("Content-Type")] string contentType,
            [Header("Authorization")] string authToken);

        [Get("/bot/v1/users/getUserLink?login={login}")]
        public Task<GetUserLinkResponse> GetUserLinkFromChat([AliasAs("login")] string login,
            [Header("Authorization")] string authToken);
    }
}
