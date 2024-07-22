using AbsenceNotifier.Core.Configurations;
using AbsenceNotifier.Core.Constants;
using AbsenceNotifier.Core.DTOs.Results.Yandex;
using AbsenceNotifier.Core.DTOs.Yandex;
using AbsenceNotifier.Core.DTOs.Yandex.Responses;
using AbsenceNotifier.Core.Interfaces;
using AbsenceNotifier.Core.Interfaces.Messengers;
using AbsenceNotifier.Core.Interfaces.Messengers.Api;
using AbsenceNotifier.Core.Settings;
using Microsoft.Extensions.Options;

namespace AbsenceNotifier.Core.Services.Yandex
{
    public class YandexApiService : IYandexApiService
    {
        private readonly IYandexChatApi _yandexChatApi;
        private readonly YandexChatConfiguration _yandexOptions;

        public YandexApiService(IYandexChatApi yandexChatApi, IOptions<YandexChatConfiguration> options,
            IDataProtectionProviderHelper dataProtectionProvider,
            IOptions<ApplicationCommonConfiguration> appOptions)
        {
            _yandexChatApi = yandexChatApi;
            _yandexOptions = options?.Value ?? throw new ArgumentException(nameof(YandexChatConfiguration));
        }

        public async Task<CreateChatWithUserResult> CreateChatWithUsers(IEnumerable<string?> logins, string name, 
            string description)
        {
            var result = new CreateChatWithUserResult();
            try
            {
                if (string.IsNullOrWhiteSpace(_yandexOptions.ChatBotLogin))
                {
                    throw new ArgumentException("Bot sender was not added to application configuration");
                }
                var users = new List<YandexUser>();
                foreach (var user in logins)
                {
                    if (!string.IsNullOrEmpty(user))
                    {
                        users.Add(new YandexUser(user));
                    }
                }
                users.Add(new YandexUser(_yandexOptions.ChatBotLogin));
                var created = await _yandexChatApi.CreateYandexChat(new CreateChatBody()
                {
                    ChatMembers = users.ToArray(),
                    ChatName = name,
                    ChatDescription = description
                }, RestApiConstants.ContentTypeJson, YandexApiSettings.AuthHeader());
                if (!created.Ok)
                {
                    result.Message = CreateChatResponse.DefaultError;
                    return result;
                }
                result.ChatId = created.ChatId;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                return result;
            }
            result.Success = true;
            return result;
        }

        public async Task<GetManagerByTeamResult> GetManagerByTeam(string team)
        {
            var result = new GetManagerByTeamResult();
            var managersYandex = _yandexOptions?.Managers?.ToList();
            if (managersYandex is null || !managersYandex.Any())
            {
                result.Message = "Managers list is empty in application settings configration file!";
                return result;
            }

            var assignedManager = managersYandex.Find(x => x.Team == team);

            if (assignedManager == null || string.IsNullOrWhiteSpace(assignedManager.Login))
            {
                result.Message = $"Not found a manager which manages team with name {team}";
                return result;
            }

            try
            {
                var checkManagerInYandexChatResult = await _yandexChatApi.GetUserLinkFromChat(assignedManager.Login,
                     YandexApiSettings.AuthHeader());

                if (!checkManagerInYandexChatResult.Ok)
                {
                    result.Message = checkManagerInYandexChatResult.Description
                        ?? $"Provided manager '{assignedManager.Login}' in configuration application settings " +
                        $"file for team name {team} was not found in yandex chats";
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                return result;
            }

            result.Success = true;
            result.Manager = assignedManager;
            return result;
        }

        public async Task<SendTextMessageResult> SendTextMessage(string text, string chatId,
            bool disableWebPageLinkPreview = true)
        {
            var result = new SendTextMessageResult();
            try
            {
                var sendResult = await _yandexChatApi.SendTextMessageToChat(new SendTextMessageBody()
                {
                    Text = text,
                    ChatId = chatId,
                    DisableWebPagePreview = disableWebPageLinkPreview,
                    DisableNotification = true
                }, RestApiConstants.ContentTypeJson, YandexApiSettings.AuthHeader());
                if (!sendResult.Ok)
                {
                    result.Message = sendResult.Description ?? SendTextMessageResponse.DefaultError;
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                return result;
            }
            result.Success = true;
            return result;
        }

        public async Task<UpdateUsersInChatResult> UpdateMembersInChat(IEnumerable<string?> membersToAdd, string chatId)
        {
            var result = new UpdateUsersInChatResult();

            try
            {
                var users = new List<YandexUser>();
                foreach(var member in membersToAdd)
                {
                    if (!string.IsNullOrWhiteSpace(member))
                    {
                        users.Add(new YandexUser(member));
                    }
                }
                var updateResult = await _yandexChatApi.UpdateMembersInChat(new UpdateMembersInChatBody()
                {
                    ChatId = chatId,
                    Members = users.ToArray()
                }, RestApiConstants.ContentTypeJson, YandexApiSettings.AuthHeader());
                if (!updateResult.Ok)
                {
                    result.Message = updateResult.Description ?? UpdateMembersInChatResponse.DefaultError;
                    result.Success = false;
                    return result;
                }
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
                return result;
            }

            result.Success = true;
            return result;
        }
    }
}
