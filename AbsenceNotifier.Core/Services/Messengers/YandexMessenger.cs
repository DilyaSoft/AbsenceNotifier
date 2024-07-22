using AbsenceNotifier.Core.Configurations;
using AbsenceNotifier.Core.Constants;
using AbsenceNotifier.Core.DTO.Results;
using AbsenceNotifier.Core.DTOs.Results;
using AbsenceNotifier.Core.DTOs.Results.TimeTastic;
using AbsenceNotifier.Core.DTOs.Results.Yandex;
using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.DTOs.Users;
using AbsenceNotifier.Core.Enums;
using AbsenceNotifier.Core.Helpers;
using AbsenceNotifier.Core.Interfaces;
using AbsenceNotifier.Core.Interfaces.Messengers;
using AbsenceNotifier.Core.Interfaces.TimeOffsService;
using AbsenceNotifier.Core.Settings;
using Microsoft.Extensions.Options;

namespace AbsenceNotifier.Core.Services.Messengers
{
    public class YandexMessenger : IMessenger
    {
        private readonly ITimeOffHelper _timeOffHelper;
        private readonly ITimeTasticService _timeTasticService;
        private readonly IYandexApiService _yandexApiService;
        private readonly IEmailService _emailService;
        private readonly IDataProtectionProviderHelper _dataProtectionProviderHelper;
        private readonly ISavedChatsManager _savedChatsManager;
        private readonly IOptions<ApplicationCommonConfiguration> _appOptions;
        private readonly IOptions<YandexChatConfiguration> _yandexChatConfig;

        public YandexMessenger(IDataProtectionProviderHelper dataProtectionProviderHelper,
            IEmailService emailService,
            IYandexApiService yandexApiService,
            ITimeTasticService timeTasticService,
            ISavedChatsManager savedChatsManager,
            IOptions<ApplicationCommonConfiguration> appOptions,
            IOptions<YandexChatConfiguration> yandexOptions,
            ITimeOffHelper timeOffHelper)
        {
            _dataProtectionProviderHelper = dataProtectionProviderHelper;
            _appOptions = appOptions;
            _yandexChatConfig = yandexOptions;
            _yandexApiService = yandexApiService;
            _timeTasticService = timeTasticService;
            _timeOffHelper = timeOffHelper;
            _savedChatsManager = savedChatsManager;
            _emailService = emailService;
        }

        public async Task<EditHolidayResult> SendEditHolidayResponse(TimetasticUser timetasticUser, long holidayId,
            EditHolidayActions holidayAction)
        {
            var result = new EditHolidayResult();

            if (string.IsNullOrWhiteSpace(timetasticUser.Email))
            {
                result.Message = EditHolidayResult.EmptyUserEmailOrNotValid;
                return result;
            }

            var timeOff = await _timeTasticService.GetTimeOffDetails(holidayId);
            if (!timeOff.Success || timeOff.TimeOff == null)
            {
                result.Success = false;
                result.Message = GetTimeOffResult.DefaultError;
                return result;
            }

            var allHolidays = await _timeTasticService.GetForAllTimeHolidays(TimeTasticSettings.AuthHeader);

            if (!allHolidays.Success || allHolidays.Holidays == null)
            {
                result.Success = false;
                result.Message = allHolidays.ErrorMessage ?? TimeOffListResponse.DefaultErrorMessage;
                return result;
            }

            var allUsersHolidays = allHolidays.Holidays.Where(x => x.UserId == timetasticUser.Id)
                .GetToApprover()
                .OrderBy(x => x.StartDate).ToList();

            var allAfterCurrnetEvent = allUsersHolidays.Where(x => x.StartDate >= timeOff.TimeOff.EndDate);

            if (allAfterCurrnetEvent.Any())
            {
                var allAfter = new List<TimeOff>() { timeOff.TimeOff };
                allAfter.AddRange(allAfterCurrnetEvent);

                foreach (var t in allAfter)
                {
                    if (t.LeaveType != null)
                    {
                        t.TimeOffType = EnumsHelper.GetEnumValueFromDescription<TimeOffType>(t.LeaveType);
                    }
                }

                var merged = allAfter.MergeSequentialByDatesWithWeeklyHolidays();
                timeOff.TimeOff.Sequences = merged.ToList();
                foreach (var holiday in merged)
                {
                    var editHolidayResult = await _timeTasticService.EditHoliday(holidayId, timetasticUser.Id, (int)holidayAction,
                        TimeTasticSettings.AuthHeader,
                        RestApiConstants.ContentTypeJson);

                    if (!editHolidayResult.Success)
                    {
                        result.Success = false;
                        result.Message = editHolidayResult.Message;
                        return result;
                    }
                }
            }


            if (timeOff.TimeOff.LeaveType == null)
            {
                result.Success = false;
                result.Message = GetTimeOffResult.LeaveTypeNotValidError;
                return result;
            }

            timeOff.TimeOff.TimeOffType = EnumsHelper.GetEnumValueFromDescription<TimeOffType>(timeOff.TimeOff.LeaveType);

            var savedChat = "";

            var createdChatResult = await _yandexApiService.CreateChatWithUsers(new string[] { timetasticUser.Email },
                YandexApiMessengerConstants.CallBackMessageToRequesterAfterManagerAction,
                YandexApiMessengerConstants.CallBackMessageToRequesterAfterManagerAction);

            if (!createdChatResult.Success || string.IsNullOrWhiteSpace(createdChatResult.ChatId))
            {
                result.Success = false;
                result.Message = createdChatResult.Message;
                return result;
            }
            savedChat = createdChatResult.ChatId;

            var actionTypeMessage = holidayAction == EditHolidayActions.Approve ? "approved" : "declined";

            DateTime endTime = timeOff.TimeOff.Sequences != null && timeOff.TimeOff.Sequences.Any() ? timeOff.TimeOff.Sequences.Last().EndDate : timeOff.TimeOff.EndDate;

            if (endTime.DayOfWeek == DayOfWeek.Friday)
            {
                endTime = endTime.AddDays(2);
            }

            var actionSent = await _yandexApiService.SendTextMessage(
                $"Your time-off request for '{TimeOffConstants.TimeOffTypesNamesDict[timeOff.TimeOff.TimeOffType]}' with start date {timeOff.TimeOff.StartDate:dd.MM.yyyy} and end date {endTime:dd.MM.yyyy} was {actionTypeMessage}",
                savedChat);

            if (!actionSent.Success)
            {
                result.Message = actionSent.Message;
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
                if (isTodayEvent)
                {
                    var getFroGeneralMessage = timeOffBuilder.GetYandexTimeOff(timeOff.TimeOff, true);

                    savedChat = _yandexChatConfig.Value.GeneralChatId ?? "";

                    if (_yandexChatConfig.Value.UserInGeneralChat is null)
                    {
                        result.Success = false;
                        result.Message = EditHolidayResult.NotFoundUsersMessage;
                        return result;
                    }

                    var addUserToChatResult = await _yandexApiService.UpdateMembersInChat(_yandexChatConfig.Value.UserInGeneralChat
                                .Select(x => x.Login), savedChat);
                    if (!addUserToChatResult.Success)
                    {
                        result.Message = addUserToChatResult.Message;
                        result.Success = false;
                        return result;
                    }
                    var sentToGeneral = await _yandexApiService.SendTextMessage(TimeOffReportMessageBuilder.TodayTitle + " " + "\n " + getFroGeneralMessage,
                        savedChat);

                    if (!sentToGeneral.Success)
                    {
                        result.Success = false;
                        result.Message = sentToGeneral.Message;
                        return result;
                    }
                }
                if (isBeforeWeekEvent)
                {
                    var timeOffEmailMessage = timeOffBuilder.GetReportMessageForEmail(new TimeOff[] { timeOff.TimeOff }, false);
                    var groupedToEmail = await _timeOffHelper.GetGroupedTimeOffsByYandexApiTeamManager(
                        new TimeOff[] { timeOff.TimeOff });
                    if (!groupedToEmail.Success || groupedToEmail.DataResult is null || !groupedToEmail.DataResult.Any())
                    {
                        result.Message = groupedToEmail.Message;
                        return result;
                    }
                    foreach (var email in groupedToEmail.DataResult.Select(x => x.emailToSend))
                    {
                        if (!string.IsNullOrEmpty(timeOffEmailMessage) && EmailHelper.IsEmailValid(email))
                        {
                            var sendEmailResult = await _emailService.SendEmailAsync(email,
                                TimeOffConstants.EmailSubject,
                                timeOffEmailMessage);

                            if (sendEmailResult == null || !sendEmailResult.Success)
                            {
                                result.Success = false;
                                result.Message = sendEmailResult?.Message ?? SendEmailResult.DefaultErrorMessage;
                                return result;
                            }
                        }
                        else
                        {
                            result.Success = false;
                            result.Message = EmailHelper.IsEmailValid(email) ? SendEmailResult.NotValidEmailMessage : SendEmailResult.DefaultErrorMessage;
                            return result;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(_appOptions.Value.GeneralManagerEmail) &&
                         !string.IsNullOrWhiteSpace(timeOffEmailMessage)
                         && EmailHelper.IsEmailValid(_appOptions.Value.GeneralManagerEmail))
                    {
                        var sendEmailResult = await _emailService.SendEmailAsync(_appOptions.Value.GeneralManagerEmail,
                            TimeOffConstants.EmailSubject,
                            timeOffEmailMessage);

                        if (sendEmailResult == null || !sendEmailResult.Success)
                        {
                            result.Success = false;
                            result.Message = sendEmailResult?.Message ?? EditHolidayResult.DefaultErrorMessage;
                            return result;
                        }
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = ApplicationCommonConfiguration.EmptyGeneralManagerMessege;
                        return result;
                    }
                }
            }

            result.Success = true;
            return result;
        }

        public async Task<SendToManagersResult> SendTimeOffRequestToManagers(IEnumerable<TimeOff> timeOffs)
        {
            var result = new SendToManagersResult();

            var messageBuilder = new TimeOffReportMessageBuilder(_dataProtectionProviderHelper,
               _appOptions.Value);

            foreach (var t in timeOffs)
            {
                if (t.LeaveType != null)
                {
                    t.TimeOffType = EnumsHelper.GetEnumValueFromDescription<TimeOffType>(t.LeaveType);
                }
            }

            var onlyActive = timeOffs.GetActive().ToArray();
            var timeOffForApproverForChat = onlyActive.GetToApprover();

            var sequencesFilter = timeOffForApproverForChat.MergeSequentialByDatesWithWeeklyHolidays();

            var sendToEmailTimeOff = sequencesFilter.GetToSendBeforeWeekToApprover();

            if (!sequencesFilter.Any() && !sendToEmailTimeOff.Any())
            {
                result.Success = true;
                return result;
            }

            var groupedResult = await _timeOffHelper.GetGroupedTimeOffsByYandexApiTeamManager(sequencesFilter);
            if (!groupedResult.Success)
            {
                result.Message = groupedResult.Message;
                return result;
            }
            if (groupedResult.DataResult is null || !groupedResult.DataResult.Any())
            {
                result.Message = GroupedTimeOffsByYandexManagerResult.ErrorInGroupingResult;
                return result;
            }

            foreach (var group in groupedResult.DataResult)
            {
                var statusIcon = TimeOffConstants.RequestStatusUnicodeIcons[RequestStatus.Pending];
                var sendTitleResult = await _yandexApiService.SendTextMessage(
                    TimeOffConstants.TitleForManagerTimeOffs + $" {statusIcon}",
                    group.chatId, true);

                if (!sendTitleResult.Success)
                {
                    result.Message = sendTitleResult.Message;
                    return result;
                }

                foreach (var timeOff in group.times)
                {
                    if (timeOff.LeaveType != null)
                    {
                        timeOff.TimeOffType = EnumsHelper.GetEnumValueFromDescription<TimeOffType>(timeOff.LeaveType);
                    }
                    if (timeOff.Sequences != null && timeOff.Sequences.Any())
                    {
                        timeOff.Sequences.ForEach(x =>
                        {
                            x.TimeOffType = EnumsHelper.GetEnumValueFromDescription<TimeOffType>(x.LeaveType);
                        });
                    }
                }

                var groupedByType = group.times.GroupBy(x => x.TimeOffType);

                foreach (var timeOff in groupedByType)
                {
                    await Task.Delay(7000);
                    var timeOffTypeTitle = messageBuilder.GetYandexTimeOffTypeTitle(timeOff.Key);
                    var sendTimeOffTitleResult = await _yandexApiService.SendTextMessage(
                        timeOffTypeTitle,
                        group.chatId, true);

                    if (!sendTimeOffTitleResult.Success)
                    {
                        result.Message = sendTimeOffTitleResult.Message;
                        result.Success = false;
                        return result;
                    }

                    foreach (var timeOffStatus in timeOff)
                    {
                        var timeOffRow = messageBuilder.GetYandexTimeOff(timeOffStatus, false);
                        await Task.Delay(7000);

                        var sendStatusTimeOffResult = await _yandexApiService.SendTextMessage(
                            timeOffRow, group.chatId, true);

                        if (!sendStatusTimeOffResult.Success)
                        {
                            result.Message = sendStatusTimeOffResult.Message;
                            return result;
                        }

                        var approve = messageBuilder.GenerateChatApproveLink(timeOffStatus.Id, timeOffStatus.UserId, true);
                        var decline = messageBuilder.GenerateChatDeclineLink(timeOffStatus.Id, timeOffStatus.UserId, true);

                        var sendApprove = await _yandexApiService.SendTextMessage(
                            approve, group.chatId, true);

                        if (!sendApprove.Success)
                        {
                            result.Message = sendApprove.Message;
                            return result;
                        }

                        var sendDecline = await _yandexApiService.SendTextMessage(decline, group.chatId, true);

                        if (!sendDecline.Success)
                        {
                            result.Message = sendDecline.Message;
                            return result;
                        }
                    }
                }
            }

            if (!sendToEmailTimeOff.Any())
            {
                result.Success = true;
                return result;
            }

            var groupedToEmail = await _timeOffHelper.GetGroupedTimeOffsByYandexApiTeamManager(sendToEmailTimeOff, false);
            if (!groupedToEmail.Success || groupedToEmail.DataResult is null || !groupedToEmail.DataResult.Any())
            {
                result.Message = groupedToEmail.Message;
                return result;
            }
            foreach (var week in groupedToEmail.DataResult)
            {
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

        public async Task<SendToGeneralChatResult> SendTimeOffsNotificationsToGeneralChat
            (IEnumerable<TimeOff> timeOffs)
        {
            var result = new SendToGeneralChatResult();
            foreach (var timeOff in timeOffs)
            {
                if (timeOff.LeaveType != null)
                {
                    timeOff.TimeOffType = EnumsHelper.GetEnumValueFromDescription<TimeOffType>(timeOff.LeaveType);
                }
            }
            var sequences = timeOffs.MergeSequentialByDatesWithWeeklyHolidays();

            var today = sequences.GetForGeneralRoomSendToday();
            var sendToEmailTimeOff = sequences.GetForGeneralRoomBeforeWeek();

            if (!today.Any() && !sendToEmailTimeOff.Any())
            {
                result.Success = true;
                return result;
            }

            var messageBuilder = new TimeOffReportMessageBuilder(_dataProtectionProviderHelper, _appOptions.Value);

            if (_yandexChatConfig.Value.UserInGeneralChat is null)
            {
                result.Success = false;
                result.Message = YandexChatConfiguration.GeneralChatUsersNotFoundError;
                return result;
            }

            var savedChat = _yandexChatConfig.Value.GeneralChatId ?? "";

            var addUserToChatResult = await _yandexApiService.UpdateMembersInChat(_yandexChatConfig.Value.UserInGeneralChat
                          .Select(x => x.Login), savedChat);
            if (!addUserToChatResult.Success)
            {
                result.Message = addUserToChatResult.Message;
                result.Success = false;
                return result;
            }

            var groupToday = today.GroupBy(x => x.TimeOffType);

            await Task.Delay(7000);
            if (groupToday.Any())
            {
                var statusIcon = TimeOffConstants.RequestStatusUnicodeIcons[RequestStatus.Approved];
                var sendTitleResult = await _yandexApiService.SendTextMessage(TimeOffReportMessageBuilder.TodayTitle
                    + $" {statusIcon}",
                    savedChat);

                if (!sendTitleResult.Success)
                {
                    result.Message = sendTitleResult.Message;
                    return result;
                }
            }

            foreach (var timeOffGroup in groupToday)
            {
                await Task.Delay(7000);
                var timeOffTypeTitle = messageBuilder.GetYandexTimeOffTypeTitle(timeOffGroup.Key);
                var resultTimeOffTitle = await _yandexApiService.SendTextMessage(timeOffTypeTitle,
                    savedChat);

                if (!resultTimeOffTitle.Success)
                {
                    result.Message = resultTimeOffTitle.Message;
                    return result;
                }

                foreach (var timeOff in timeOffGroup)
                {
                    var timeOffRow = messageBuilder.GetYandexTimeOff(timeOff, true);

                    var sendRowResult = await _yandexApiService.SendTextMessage(timeOffRow,
                        savedChat);

                    if (!sendRowResult.Success)
                    {
                        result.Message = sendRowResult.Message;
                        return result;
                    }
                }

            }

            if (!sendToEmailTimeOff.Any())
            {
                result.Success = true;
                return result;
            }

            var groupedToEmail = await _timeOffHelper.GetGroupedTimeOffsByYandexApiTeamManager(sendToEmailTimeOff);
            if (!groupedToEmail.Success || groupedToEmail.DataResult is null || !groupedToEmail.DataResult.Any())
            {
                result.Message = groupedToEmail.Message;
                return result;
            }
            foreach (var week in groupedToEmail.DataResult)
            {
                var timeOffEmailMessage = messageBuilder.GetReportMessageForEmail(week.times, false);
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
                else
                {
                    result.Success = false;
                    result.Message = EmailHelper.IsEmailValid(week.emailToSend) ? SendEmailResult.NotValidEmailMessage : TimeOff.EmptyTimeOffMessage;
                    return result;
                }

                timeOffEmailMessage = messageBuilder.GetReportMessageForEmail(week.times, false);
                if (!string.IsNullOrWhiteSpace(_appOptions.Value.GeneralManagerEmail) &&
                     !string.IsNullOrWhiteSpace(timeOffEmailMessage)
                     && EmailHelper.IsEmailValid(_appOptions.Value.GeneralManagerEmail))
                {
                    var sendEmailResult = await _emailService.SendEmailAsync(_appOptions.Value.GeneralManagerEmail,
                        TimeOffConstants.EmailSubject,
                        timeOffEmailMessage);

                    if (sendEmailResult == null || !sendEmailResult.Success)
                    {
                        result.Success = false;
                        result.Message = sendEmailResult?.Message ?? SendEmailResult.DefaultErrorMessage;
                        return result;
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = ApplicationCommonConfiguration.EmptyGeneralManagerMessege;
                    return result;
                }
            }

            result.Success = true;
            return result;
        }
    }
}
