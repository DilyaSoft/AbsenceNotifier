using AbsenceNotifier.Core.Configurations;
using AbsenceNotifier.Core.Constants;
using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.Enums;
using AbsenceNotifier.Core.Interfaces;
using AbsenceNotifier.Core.Settings;
using System.Text;

namespace AbsenceNotifier.Core.Helpers
{
    public class TimeOffReportMessageBuilder
    {
        private readonly IDataProtectionProviderHelper _dataProtectionProvider;
        private readonly ApplicationCommonConfiguration _applicationCommonSettings;
        private readonly string _vacationTimeOffRocketIcon = ":bell:";
        private readonly string _vacationTimeOffYandexIcon = "🔔";
        private readonly string _leaveYandexIcon = "📣";
        private readonly string _leaveRocketIcon = ":mega:";

        public const string WeekTitle = "List of time-offs starting in a week";
        public const string TodayTitle = "List of time-offs starting from today";

        public TimeOffReportMessageBuilder(IDataProtectionProviderHelper protectionProvider,
          ApplicationCommonConfiguration applicationCommonSettings)
        {
            _dataProtectionProvider = protectionProvider;
            _applicationCommonSettings = applicationCommonSettings;
        }

        public string GetReportMessageForEmail(IEnumerable<TimeOff> timeOffs, bool withLinks)
        {
            var sb = new StringBuilder();

            var beforeWeekTimeOffs =
                timeOffs.Where(x => x.Status != RequestStatus.Cancelled && x.Status != RequestStatus.Declined
                && x.LeaveType != null
                && x.StartDate.IsBeforeWeekStartDate());

            if (beforeWeekTimeOffs.Any())
            {
                if (withLinks)
                {
                    var statusIcon = TimeOffConstants.RequestStatusUnicodeIcons[RequestStatus.Pending];
                    sb.Append($"{statusIcon} Time-off requests from Timetastic which starts in a week");
                }
                else
                {
                    var statusIcon = TimeOffConstants.RequestStatusUnicodeIcons[RequestStatus.Approved];
                    sb.Append($"{statusIcon} Approved time-off from Timetastic which starts in a week");
                }
                sb.Append("<br>");
            }

            var groupByType = beforeWeekTimeOffs.GroupBy(x => x.TimeOffType);

            foreach (var typeG in groupByType)
            {

                var typeIcon = TimeOffConstants.TimeOffUnicodeTypesIconsDict[typeG.Key];
                var ruTypeName = TimeOffConstants.TimeOffTypesNamesDict[typeG.Key];

                sb.Append($"{ruTypeName} {typeIcon} : ");
                sb.Append("<br>");

                var groupBeforeWeek = typeG.GroupBy(x => x.Status);

                foreach (var emailTimeOff in groupBeforeWeek)
                {
                    var ruTypeDescription = TimeOffConstants.TimeOffTypesRuDescriptionDict[typeG.Key];
                    sb.Append("<br>");
                    var appendNextLine = emailTimeOff.Count() > 1;
                    foreach (var timeOff in emailTimeOff)
                    {
                        var approve = GenerateEmailApproveLink(timeOff.Id, timeOff.UserId);
                        var decline = GenerateEmailDeclineLink(timeOff.Id, timeOff.UserId);
                        if (timeOff.TimeOffType == TimeOffType.Vacation)
                        {
                            sb.Append($" {_vacationTimeOffYandexIcon}");
                        }
                        else
                        {
                            sb.Append($" {_leaveYandexIcon}");
                        }

                        DateTime endDate = timeOff.Sequences != null && timeOff.Sequences.Any() ? timeOff.Sequences.Last().EndDate : timeOff.EndDate;

                        if (endDate.DayOfWeek == DayOfWeek.Friday)
                        {
                            endDate = endDate.AddDays(2);
                        }

                        sb.Append($" У {timeOff.UserName} {ruTypeDescription} from {timeOff.StartDate:dd.MM.yyyy} to {endDate:dd.MM.yyyy}");
                        if (withLinks)
                        {
                            sb.Append($" {approve} | {decline}");
                        }
                        if (appendNextLine)
                        {
                            sb.Append("<br>");
                        }
                    }
                    sb.Append("<br>");
                }
            }

            return sb.ToString();
        }

        public string GenerateChatApproveLink(long id, long userId, bool skipFirstPreviewLinkRequest = false)
        {
            var currentMessenger = ApplicationSettings.CurrentMessengerName;
            switch (currentMessenger)
            {
                case "Yandex":
                {
                    return $"[Click here to approve]({YandexApiSettings.GateWayUrl}/{id}/0)";
                }
                default:
                {
                    var protectedHoliday = _dataProtectionProvider.GetEncryptedValue(id.ToString());
                    var protectedUser = _dataProtectionProvider.GetEncryptedValue(userId.ToString());
                    return $"[Click here to approve]({_applicationCommonSettings.RequestActionsApiUrl}/TimeOff/Approve?id={protectedHoliday.Value}" +
                        $"&userId={protectedUser.Value}&skipFirstRequest={skipFirstPreviewLinkRequest})";
                }
            }
        }

        public string GenerateChatDeclineLink(long id, long userId, bool skipFirstPreviewLinkRequest = false)
        {
            var currentMessenger = ApplicationSettings.CurrentMessengerName;
            switch (currentMessenger)
            {
                case "Yandex":
                {
                    return $"[Click here to decline]({YandexApiSettings.GateWayUrl}/{id}/1)";
                }
                default:
                {
                    var protectedHoliday = _dataProtectionProvider.GetEncryptedValue(id.ToString());
                    var protectedUser = _dataProtectionProvider.GetEncryptedValue(userId.ToString());
                    return $"[Click here to decline]({_applicationCommonSettings.RequestActionsApiUrl}/TimeOff/Decline?id={protectedHoliday.Value}" +
                        $"&userId={protectedUser.Value}&skipFirstRequest={skipFirstPreviewLinkRequest})";
                }
            }
        }

        public string GetRocketTimeOff(TimeOff timeOff, bool isForGeneral)
        {
            var ruTypeDescription = TimeOffConstants.TimeOffTypesRuDescriptionDict[timeOff.TimeOffType];
            if (timeOff.TimeOffType == TimeOffType.Vacation)
            {
                if (!isForGeneral && timeOff.Status == RequestStatus.Pending)
                {
                    return $"{_vacationTimeOffRocketIcon} {timeOff.UserName} {ruTypeDescription} from {timeOff.StartDate:dd.MM.yyyy} to {timeOff.EndDate:dd.MM.yyyy}";
                }
                return $"{_vacationTimeOffRocketIcon} {timeOff.UserName} {ruTypeDescription} from {timeOff.StartDate:dd.MM.yyyy} to {timeOff.EndDate:dd.MM.yyyy}";
            }
            else
            {
                if (!isForGeneral && timeOff.Status == RequestStatus.Pending)
                {
                    return $"{_leaveRocketIcon} {timeOff.UserName} {ruTypeDescription} from  {timeOff.StartDate:dd.MM.yyyy} to {timeOff.EndDate:dd.MM.yyyy}";
                }
                return $"{_leaveRocketIcon} {timeOff.UserName} {ruTypeDescription} from {timeOff.StartDate:dd.MM.yyyy} to {timeOff.EndDate:dd.MM.yyyy}";
            }
        }

        public string GetYandexTimeOff(TimeOff timeOff, bool isForGeneralRoom)
        {
            var ruTypeDescription = TimeOffConstants.TimeOffTypesRuDescriptionDict[timeOff.TimeOffType];
            if (timeOff.TimeOffType == TimeOffType.Vacation)
            {
                DateTime endDate = timeOff.EndDate;
                if (timeOff.Sequences != null && timeOff.Sequences.Any())
                {
                    endDate = timeOff.Sequences.Last().EndDate;
                }

                if (endDate.DayOfWeek == DayOfWeek.Friday)
                {
                    endDate = endDate.AddDays(2);
                }

                if (!isForGeneralRoom && timeOff.Status == RequestStatus.Pending)
                {
                    return $"{_vacationTimeOffYandexIcon} {timeOff.UserName} {ruTypeDescription} from {timeOff.StartDate:dd.MM.yyyy} to {endDate:dd.MM.yyyy}";
                }
                return $"{_vacationTimeOffYandexIcon} {timeOff.UserName} {ruTypeDescription} from {timeOff.StartDate:dd.MM.yyyy} to {endDate:dd.MM.yyyy}";
            }
            else
            {
                DateTime endDate = timeOff.EndDate;
                if (timeOff.Sequences != null && timeOff.Sequences.Any())
                {
                    endDate = timeOff.Sequences.Last().EndDate;
                }
                if (endDate.DayOfWeek == DayOfWeek.Friday)
                {
                    endDate = endDate.AddDays(2);
                }

                if (!isForGeneralRoom && timeOff.Status == RequestStatus.Pending)
                {
                    return $"{_leaveYandexIcon} {timeOff.UserName} {ruTypeDescription} from {timeOff.StartDate:dd.MM.yyyy} to {endDate:dd.MM.yyyy}";
                }
                return $"{_leaveYandexIcon} {timeOff.UserName} {ruTypeDescription} from {timeOff.StartDate:dd.MM.yyyy} to {endDate:dd.MM.yyyy}";
            }
        }

        public string GetRocketTimeOffTypeTitle(TimeOffType timeOffType)
        {
            var typeIcon = TimeOffConstants.TimeOffRocketChatTypesIconsDict[timeOffType];
            var ruTypeName = TimeOffConstants.TimeOffTypesNamesDict[timeOffType];
            return $"{ruTypeName} {typeIcon} : " + "\n";
        }

        public string GetYandexTimeOffTypeTitle(TimeOffType type)
        {
            var typeIcon = TimeOffConstants.TimeOffUnicodeTypesIconsDict[type];
            var ruTypeName = TimeOffConstants.TimeOffTypesNamesDict[type];
            return $"{ruTypeName} {typeIcon} : " + "\n";
        }

        public string GetRocketStatusTitle(RequestStatus requestStatus)
        {
            var ruStatus = TimeOffConstants.RequestStatusNameDict[requestStatus];
            var statusIcon = TimeOffConstants.RequestRocketChatStatusIcons[requestStatus];
            return $"{ruStatus} {statusIcon} : " + "\n";
        }

        public string GetYandexStatusTitle(RequestStatus requestStatus)
        {
            var ruStatus = TimeOffConstants.RequestStatusNameDict[requestStatus];
            var statusIcon = TimeOffConstants.RequestStatusUnicodeIcons[requestStatus];
            return $"{ruStatus} {statusIcon} : " + "\n";
        }

        private string GenerateEmailApproveLink(long id, long userId)
        {
            var protectedHoliday = _dataProtectionProvider.GetEncryptedValue(id.ToString());
            var protectedUser = _dataProtectionProvider.GetEncryptedValue(userId.ToString());
            return $"<a href='{_applicationCommonSettings.RequestActionsApiUrl}/TimeOff/Approve?ids={protectedHoliday.Value}&userId={protectedUser.Value}'>Approve</a>";
        }

        private string GenerateEmailDeclineLink(long id, long userId)
        {
            var protectedHoliday = _dataProtectionProvider.GetEncryptedValue(id.ToString());
            var protectedUser = _dataProtectionProvider.GetEncryptedValue(userId.ToString());
            return $"<a href='{_applicationCommonSettings.RequestActionsApiUrl}/TimeOff/Decline?ids={protectedHoliday.Value}&userId={protectedUser.Value}'>Decline</a>";
        }
    }
}
