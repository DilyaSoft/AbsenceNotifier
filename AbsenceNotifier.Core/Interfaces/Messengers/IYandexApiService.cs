using AbsenceNotifier.Core.DTOs.Results.Yandex;

namespace AbsenceNotifier.Core.Interfaces.Messengers
{
    public interface IYandexApiService
    {
        public Task<SendTextMessageResult> SendTextMessage(string text, string chatId, bool disableWebPageLinkPreview = true);
        public Task<GetManagerByTeamResult> GetManagerByTeam(string team);
        public Task<CreateChatWithUserResult> CreateChatWithUsers(IEnumerable<string?> logins, string name, string description);
        public Task<UpdateUsersInChatResult> UpdateMembersInChat(IEnumerable<string?> membersToAdd, string chatId);
    }
}
