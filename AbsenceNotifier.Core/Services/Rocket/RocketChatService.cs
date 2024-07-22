using AbsenceNotifier.Core.Configurations;
using AbsenceNotifier.Core.Constants;
using AbsenceNotifier.Core.DTOs.Results;
using AbsenceNotifier.Core.DTOs.Results.RocketChat;
using AbsenceNotifier.Core.DTOs.Rocket;
using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.Enums;
using AbsenceNotifier.Core.Helpers;
using AbsenceNotifier.Core.Interfaces;
using AbsenceNotifier.Core.Interfaces.Messengers;
using AbsenceNotifier.Core.Interfaces.Messengers.Api;
using Microsoft.Extensions.Options;

namespace AbsenceNotifier.Core.Services.Rocket
{
    public class RocketChatService : IRocketChatService
    {
        private readonly IRocketChatApi _rocketChatApi;
        private readonly RocketChatConfiguration _rocketChatSettings;
        private readonly IDataProtectionProviderHelper _dataProtectionProvider;
        private readonly ApplicationCommonConfiguration _applicationCommonSettings;

        public RocketChatService(IOptions<RocketChatConfiguration> rocketChatOptions,
            IRocketChatApi rocketChatApi,
            IDataProtectionProviderHelper dataProtectionProvider,
            IOptions<ApplicationCommonConfiguration> applicationCommonSettings)
        {
            _rocketChatApi = rocketChatApi;
            _rocketChatSettings = rocketChatOptions.Value;
            _dataProtectionProvider = dataProtectionProvider;
            _applicationCommonSettings = applicationCommonSettings.Value;
        }

        public async Task<CreateRoomResult> CreateRoom(CreateRoomBody roomBody, string authToken, string userId)
        {
            CreateRoomResult roomResult = new();
            try
            {
                roomResult = await _rocketChatApi.CreateRoom(roomBody, RestApiConstants.ContentTypeJson, authToken,
                    userId);

                roomResult.Success = true;
                roomResult.Status = "success";
            }
            catch (Exception ex)
            {
                roomResult.Success = false;
                roomResult.Message = ex.Message;
                roomResult.Status = "Failed";
            }
            return roomResult;
        }

        public async Task<GetAllRocketChatUsersResult> GetAllUsers(string authToken, string userId)
        {
            var result = new GetAllRocketChatUsersResult();
            try
            {
                result = await _rocketChatApi.GetAllUsers(authToken, userId);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        public async Task<GetChannelInfoResult> GetChannelByName(string name, string authToken, string userId)
        {
            var result = new GetChannelInfoResult();
            try
            {
                result = await _rocketChatApi.GetChannelByName(name, authToken, userId);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
                return result;
            }
            result.Success = true;
            return result;
        }

        public async Task<SendMessageRocketResult> SendMessageToRoom(SendMessageRequest sendMessage,
            string authToken, string userId)
        {
            var sendMessageResult = new SendMessageRocketResult();

            try
            {
                sendMessageResult = await _rocketChatApi.SendMessageToChannel(sendMessage, authToken, userId);
            }
            catch (Exception ex)
            {
                sendMessageResult.Success = false;
                sendMessageResult.ErrorMessage = ex.Message;
                return sendMessageResult;
            }
            sendMessageResult.Success = true;
            return sendMessageResult;
        }
        public async Task<LoginToRocketResult> SignIn(SignInRequest signInRequest)
        {
            var loginResult = new LoginToRocketResult();
            try
            {
                loginResult = await _rocketChatApi.SignIn(signInRequest);
                loginResult.Success = true;
            }
            catch (Exception ex)
            {
                loginResult.Success = false;
                loginResult.Status = "error";
                loginResult.Message = ex.Message;
            }
            return loginResult;
        }

        public async Task<SendToGeneralChatResult> SendNotificationsToGeneralRoom(IEnumerable<TimeOff> timeOffs,
            string token, string userId)
        {
            var result = new SendToGeneralChatResult();
            var week = timeOffs.GetForGeneralRoomBeforeWeek();
            var today = timeOffs.GetForGeneralRoomSendToday();

            var channel = await GetChannelByName(_rocketChatSettings.GeneralRoomName ?? throw new ArgumentException(nameof(_rocketChatSettings)),
                token, userId);

            if (!channel.Success || channel.Channel?.Id == null)
            {
                result.Success = false;
                result.Message = channel.Message;
                return result;
            }

            var sendTimeOffToday = await SendTimeOffsToChat(today, TimeOffReportMessageBuilder.TodayTitle,
                channel.Channel.Id, token, userId, true);

            if (!sendTimeOffToday.Success)
            {
                result.Success = false;
                result.Message = sendTimeOffToday.Message;
                return result;
            }

            var sendTimeOffWeek = await SendTimeOffsToChat(week, TimeOffReportMessageBuilder.WeekTitle,
                channel.Channel.Id, token, userId, true);

            if (!sendTimeOffWeek.Success)
            {
                result.Success = false;
                result.Message = sendTimeOffWeek.Message;
                return result;
            }

            result.Success = true;
            return result;
        }

        public async Task<NotifyTimeOffResult> SendTimeOffsToChat(IEnumerable<TimeOff> timeOffs, string title,
            string channel, string token, string userId, bool isForGeneral)
        {
            var result = new NotifyTimeOffResult();
            var messageBuilder = new TimeOffReportMessageBuilder(_dataProtectionProvider, _applicationCommonSettings);

            foreach (var timeOff in timeOffs)
            {
                if (timeOff.LeaveType != null)
                {
                    timeOff.TimeOffType = EnumsHelper.GetEnumValueFromDescription<TimeOffType>(timeOff.LeaveType);
                }
            }
            var groupBeforeWeek = timeOffs.GroupBy(x => x.TimeOffType);
            await Task.Delay(7000);
            if (groupBeforeWeek.Any())
            {
                var sendTitleResult = await SendMessageToRoom(new SendMessageRequest()
                {
                    Message = new Message()
                    {
                        ChannelId = channel,
                        SendingMessage = title
                    }
                }, token, userId);
                if (!sendTitleResult.Success)
                {
                    result.Message = sendTitleResult.ErrorMessage;
                    result.Success = false;
                    return result;
                }
            }

            foreach (var timeOffGroup in groupBeforeWeek)
            {
                await Task.Delay(7000);
                var timeOffTypeTitle = messageBuilder.GetRocketTimeOffTypeTitle(timeOffGroup.Key);
                var resultTimeOffTitle = await SendMessageToRoom(new SendMessageRequest()
                {
                    Message = new Message()
                    {
                        ChannelId = channel,
                        SendingMessage = timeOffTypeTitle
                    }
                }, token, userId);

                if (!resultTimeOffTitle.Success)
                {
                    result.Message = resultTimeOffTitle.ErrorMessage;
                    result.Success = false;
                    return result;
                }

                var groupByStatus = timeOffGroup.GroupBy(x => x.Status);
                foreach (var byStatus in groupByStatus)
                {
                    await Task.Delay(7000);
                    var statusTitle = messageBuilder.GetRocketStatusTitle(byStatus.Key);
                    var sendStatus = await SendMessageToRoom(new SendMessageRequest()
                    {
                        Message = new Message()
                        {
                            ChannelId = channel,
                            SendingMessage = statusTitle
                        }
                    }, token, userId);

                    if (!sendStatus.Success)
                    {
                        result.Message = sendStatus.ErrorMessage;
                        result.Success = false;
                        return result;
                    }

                    foreach (var timeOff in byStatus)
                    {
                        var timeOffRow = messageBuilder.GetRocketTimeOff(timeOff, isForGeneral);
                        await Task.Delay(7000);

                        var sendMessage = new Message()
                        {
                            ChannelId = channel,
                            SendingMessage = timeOffRow
                        };

                        if (!isForGeneral)
                        {
                            var approve = messageBuilder.GenerateChatApproveLink(timeOff.Id, timeOff.UserId);
                            var decline = messageBuilder.GenerateChatDeclineLink(timeOff.Id, timeOff.UserId);
                            sendMessage.Text = "actions";
                            sendMessage.Attachments = new List<Attachment>()
                            {
                                new Attachment()
                                {
                                    Text = "",
                                    Title = "Request Actions",
                                    Collapsed = true,
                                    Fields = new List<AttachmentField>()
                                    {
                                        new AttachmentField()
                                        {
                                            Short = true,
                                            Title = "Approve request",
                                            Value = approve
                                        },
                                        new AttachmentField()
                                        {
                                            Short = true,
                                            Title = "Decline request",
                                            Value = decline
                                        }
                                    }
                                }
                            };
                        }

                        var sendTimeOff = await SendMessageToRoom(new SendMessageRequest()
                        {
                            Message = sendMessage
                        }, token, userId);
                        if (!sendTimeOff.Success)
                        {
                            result.Message = sendTimeOff.ErrorMessage;
                            result.Success = false;
                            return result;
                        }
                    }
                }
            }

            result.Success = true;
            return result;
        }

    }
}
