using AbsenceNotifier.Core.Configurations;
using AbsenceNotifier.Core.Constants;
using AbsenceNotifier.Core.DTO.Results;
using AbsenceNotifier.Core.DTOs.Results;
using AbsenceNotifier.Core.DTOs.Results.RocketChat;
using AbsenceNotifier.Core.DTOs.Results.TimeTastic;
using AbsenceNotifier.Core.DTOs.Rocket;
using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.DTOs.Users;
using AbsenceNotifier.Core.Enums;
using AbsenceNotifier.Core.Helpers;
using AbsenceNotifier.Core.Interfaces;
using AbsenceNotifier.Core.Interfaces.Messengers;
using AbsenceNotifier.Core.Interfaces.TimeOffsService;
using AbsenceNotifier.Core.Settings;
using Microsoft.Extensions.Options;
using Serilog;

namespace AbsenceNotifier.Core.Services.Messengers
{
    public class RockectChatMessenger : IMessenger
    {
        private readonly IRocketChatService _rocketChatService;
        private readonly ITimeTasticService _timeTasticService;
        private readonly IEmailService _emailService;
        private readonly ITimeOffHelper _timeOffHelper;
        private readonly IDataProtectionProviderHelper _dataProtectionProviderHelper;
        private readonly IOptions<ApplicationCommonConfiguration> _appOptions;
        private readonly IOptions<RocketChatConfiguration> _rocketChatConfiguration;
        public RockectChatMessenger(IRocketChatService rocketChatService,
            ITimeTasticService timeTasticService,
            IEmailService emailService,
            ITimeOffHelper timeOffHelper,
            IDataProtectionProviderHelper dataProtectionProviderHelper,
            IOptions<ApplicationCommonConfiguration> options,
            IOptions<RocketChatConfiguration> rocketChatConfiguration)
        {
            _dataProtectionProviderHelper = dataProtectionProviderHelper;
            _rocketChatService = rocketChatService;
            _timeTasticService = timeTasticService;
            _emailService = emailService;
            _timeOffHelper = timeOffHelper;
            _appOptions = options;
            _rocketChatConfiguration = rocketChatConfiguration;
        }

        public async Task<EditHolidayResult> SendEditHolidayResponse(TimetasticUser timetasticUser, long holidayId,
            EditHolidayActions holidayAction)
        {

            var result = new EditHolidayResult();

            var editHolidayResult = await _timeTasticService.EditHoliday(holidayId, timetasticUser.Id, (int)holidayAction,
                TimeTasticSettings.AuthHeader,
                RestApiConstants.ContentTypeJson);

            if (!editHolidayResult.Success)
            {
                result.Success = false;
                result.Message = editHolidayResult.Message;
                return result;
            }

            var loginToChat = await _rocketChatService.SignIn(new SignInRequest()
            {
                User = RocketChatSettings.RocketChatSenderReportEmail,
                Password = RocketChatSettings.RocketChatSenderReportPassword
            });
            if (loginToChat.LoginToRocketChatFailed() ||
                             string.IsNullOrEmpty(loginToChat.Data?.AuthToken)
                             || string.IsNullOrEmpty(loginToChat.Data.UserId))
            {
                result.Message = loginToChat.Message;
                result.Success = false;
            }
            else
            {
                var allRocketChatUsersResponse = await _rocketChatService.GetAllUsers(loginToChat.Data.AuthToken,
                    loginToChat.Data.UserId);

                if (allRocketChatUsersResponse.Users == null || !allRocketChatUsersResponse.Success)
                {
                    result.Message = allRocketChatUsersResponse.Message;
                    result.Success = false;
                }
                else
                {
                    var rocketChatUsers = allRocketChatUsersResponse.Users.ToList();
                    var rocketChatToSend = rocketChatUsers.GetRocketChatUserByTimeTastic(timetasticUser);

                    if (rocketChatToSend == null)
                    {
                        result.Success = false;
                        result.Message = RocketChatUser.UserWasNotFoundInTimeTastic;
                        return result;
                    }

                    var room = await _rocketChatService.CreateRoom(new CreateRoomBody()
                    {
                        UserName = rocketChatToSend.UserName,
                    }, loginToChat.Data.AuthToken, loginToChat.Data.UserId);
                    if (!room.Success || room.Room == null)
                    {
                        result.Success = false;
                        result.Message = CreateRoomResult.DefaultError;
                        return result;
                    }
                    var timeOff = await _timeTasticService.GetTimeOffDetails(holidayId);
                    if (!timeOff.Success || timeOff.TimeOff == null)
                    {
                        result.Success = false;
                        result.Message = GetTimeOffResult.DefaultError;
                        return result;
                    }

                    if (timeOff.TimeOff.LeaveType == null)
                    {
                        result.Success = false;
                        result.Message = GetTimeOffResult.LeaveTypeNotValidError;
                        return result;
                    }

                    timeOff.TimeOff.TimeOffType = EnumsHelper.GetEnumValueFromDescription<TimeOffType>(timeOff.TimeOff.LeaveType);

                    var actionTypeMessage = holidayAction == EditHolidayActions.Approve ? "approved" : "declined";
                    var approveSend = await _rocketChatService.SendMessageToRoom(new SendMessageRequest()
                    {
                        Message = new Message()
                        {
                            ChannelId = room.Room.Id,
                            SendingMessage = $"Your time-off request for '{TimeOffConstants.TimeOffTypesNamesDict[timeOff.TimeOff.TimeOffType]}' with start date {timeOff.TimeOff.StartDate:dd.MM.yyyy} and end date {timeOff.TimeOff.EndDate:dd.MM.yyyy} was {actionTypeMessage}"
                        }
                    }, loginToChat.Data.AuthToken, loginToChat.Data.UserId);

                    if (!approveSend.Success)
                    {
                        result.Success = false;
                        result.Message = SendMessageRocketResult.DefaultError;
                        return result;
                    }

                    var isTodayEvent = timeOff.TimeOff.StartDate.IsTodayEventStartDate() 
                        && holidayAction == EditHolidayActions.Approve;

                    var isBeforeWeekEvent = timeOff.TimeOff.StartDate.IsBeforeWeekStartDate() 
                        && holidayAction == EditHolidayActions.Approve;

                    if (isTodayEvent || isBeforeWeekEvent)
                    {
                        var timeOffBuilder = new TimeOffReportMessageBuilder(_dataProtectionProviderHelper, 
                            _appOptions.Value);
                        var getFroGeneralMessage = timeOffBuilder.GetRocketTimeOff(timeOff.TimeOff, true);
                        var channel = await _rocketChatService.GetChannelByName(
                            _rocketChatConfiguration.Value.GeneralRoomName ?? "",
                            loginToChat.Data.AuthToken, loginToChat.Data.UserId);
                        if (!channel.Success || channel.Channel?.Id == null)
                        {
                            result.Success = false;
                            result.Message = channel.Message;
                            return result;
                        }
                        var titleMessage = isTodayEvent ? TimeOffReportMessageBuilder.TodayTitle : TimeOffReportMessageBuilder.WeekTitle;
                        var approveSendToGeneral = await _rocketChatService.SendMessageToRoom(new SendMessageRequest()
                        {
                            Message = new Message()
                            {
                                ChannelId = channel.Channel.Id,
                                SendingMessage = titleMessage + " " + "\n " + getFroGeneralMessage
                            }
                        }, loginToChat.Data.AuthToken, loginToChat.Data.UserId);

                        if (!approveSendToGeneral.Success)
                        {
                            result.Success = false;
                            result.Message = approveSendToGeneral.Message;
                            return result;
                        }
                    }
                }
            }

            result.Success = true;
            return result;
        }

        public async Task<SendToManagersResult> SendTimeOffRequestToManagers(IEnumerable<TimeOff> timeOffs)
        {
            var result = new SendToManagersResult()
            {
                Success = true,
            };
            var login = await _rocketChatService.SignIn(new SignInRequest()
            {
                Password = RocketChatSettings.RocketChatSenderReportPassword,
                User = RocketChatSettings.RocketChatSenderReportEmail
            });

            if (login.LoginToRocketChatFailed() || login?.Data?.AuthToken is null || login.Data?.UserId is null)
            {
                result.Success = false;
                result.Message = login?.Message ?? LoginToRocketResult.DefaultError;
                return result;
            }

            var messageBuilder = new TimeOffReportMessageBuilder(_dataProtectionProviderHelper,
                _appOptions.Value);

            var onlyActive = timeOffs.GetActive().ToArray();

            var timeOffForApproverForChat = onlyActive.GetToApprover();

            var timeOffManagersChat = await _timeOffHelper
                .GetGroupedTimeOffsByRocketChatTeamManager(timeOffForApproverForChat, login.Data.AuthToken,
                login.Data.UserId);

            if (!timeOffManagersChat.Success || timeOffManagersChat.DataResult is null)
            {
                result.Message = timeOffManagersChat.Message ?? GroupedTimeOffsByRocketManagerResult.DefaultError;
                return result;
            }

            foreach (var timeOff in timeOffManagersChat.DataResult)
            {
                var room = await _rocketChatService.CreateRoom(new CreateRoomBody()
                {
                    UserName = timeOff.userNameToSend,
                }, login.Data.AuthToken, login.Data.UserId);

                if (room.IsRocketChatCreateRoomWithError() || room?.Room?.Id is null)
                {
                    result.Success = false;
                    result.Message = room?.Message;
                    return result;
                }

                var sendResult = await _rocketChatService.SendTimeOffsToChat(timeOff.times,
                    TimeOffConstants.TitleForManagerTimeOffs, room.Room.Id,
                    login.Data.AuthToken, login.Data.UserId, false);

                if (!sendResult.Success)
                {
                    result.Message = sendResult.Message;
                    result.Success = false;
                    return result;
                }
            }

            var forEmailToApprove = onlyActive.GetToSendBeforeWeekToApprover();

            var timeOffManagersEmail = await _timeOffHelper
                .GetGroupedTimeOffsByRocketChatTeamManager(forEmailToApprove, login.Data.AuthToken,
                login.Data.UserId);

            if (!timeOffManagersEmail.Success || timeOffManagersEmail.DataResult is null)
            {
                result.Message = timeOffManagersEmail.Message;
                return result;
            }

            foreach (var week in timeOffManagersEmail.DataResult)
            {
                if (!week.times.Any())
                {
                    continue;
                }
                var timeOffEmailMessage = messageBuilder.GetReportMessageForEmail(week.times, true);
                if (!string.IsNullOrEmpty(timeOffEmailMessage) && EmailHelper.IsEmailValid(week.emailToSend))
                {
                    var sendEmailResult = await _emailService.SendEmailAsync(week.emailToSend,
                        TimeOffConstants.EmailSubject,
                        timeOffEmailMessage);

                    if (sendEmailResult == null || !sendEmailResult.Success)
                    {
                        result.Success = false;
                        result.Message = sendEmailResult?.Message ?? SendEmailResult.DefaultErrorMessage;
                        return result;
                    }
                }
            }

            result.Success = true;
            return result;
        }

        public async Task<SendToGeneralChatResult> SendTimeOffsNotificationsToGeneralChat(IEnumerable<TimeOff> timeOffs)
        {
            var result = new SendToGeneralChatResult()
            {
                Success = true,
            };
            var login = await _rocketChatService.SignIn(new SignInRequest()
            {
                Password = RocketChatSettings.RocketChatSenderReportPassword,
                User = RocketChatSettings.RocketChatSenderReportEmail
            });

            if (login.LoginToRocketChatFailed() || login.Data == null || login.Data.AuthToken == null ||
                login.Data.UserId == null)
            {
                result.Success = false;
                result.Message = login?.Message ?? LoginToRocketResult.DefaultError;
                return result;
            }

            Log.Logger.Information($"Time off report service started sending to general, current date");

            result = await _rocketChatService.SendNotificationsToGeneralRoom(
                timeOffs.GetOnlyApproved(),
                login.Data.AuthToken,
                login.Data.UserId);

            Log.Logger.Information($"Time off report service ended sending to general, current date");

            return result;
        }

    }
}
