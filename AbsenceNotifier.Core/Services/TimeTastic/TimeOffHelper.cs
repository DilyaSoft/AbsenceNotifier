using AbsenceNotifier.Core.Configurations;
using AbsenceNotifier.Core.DTOs.Results.RocketChat;
using AbsenceNotifier.Core.DTOs.Results.TimeTastic;
using AbsenceNotifier.Core.DTOs.Results.Yandex;
using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.DTOs.Users;
using AbsenceNotifier.Core.Helpers;
using AbsenceNotifier.Core.Interfaces;
using AbsenceNotifier.Core.Interfaces.Messengers;
using AbsenceNotifier.Core.Interfaces.TimeOffsService;
using AbsenceNotifier.Core.Settings;

namespace AbsenceNotifier.Core.Services.TimeTastic
{
    public class TimeOffHelper : ITimeOffHelper
    {
        private readonly IRocketChatService _rocketChatService;
        private readonly IYandexApiService _yandexApiService;
        private readonly ITimeTasticService _timeTasticService;
        private readonly ISavedChatsManager _savedChatsManager;
        private readonly YandexChatConfiguration _yandexChatConfiguration;

        public TimeOffHelper(IRocketChatService rocketChatService,
            ITimeTasticService timeTasticService,
            ISavedChatsManager savedChatsManager,
            IYandexApiService yandexApiService,
            YandexChatConfiguration yandexChatConfiguration)
        {
            _rocketChatService = rocketChatService;
            _timeTasticService = timeTasticService;
            _yandexApiService = yandexApiService;
            _savedChatsManager = savedChatsManager;
            _yandexChatConfiguration = yandexChatConfiguration;
        }

        public async Task<GroupedTimeOffsByRocketManagerResult>
            GetGroupedTimeOffsByRocketChatTeamManager(IEnumerable<TimeOff> timeOffs,
            string rocketChatToken, string rocketChatPassword)
        {
            var result = new GroupedTimeOffsByRocketManagerResult();
            var grouped = new List<(string userNameToSend, string emailToSend, List<TimeOff> times)>();
            try
            {
                var allDepartments = await _timeTasticService.GetAllDepartments(TimeTasticSettings.AuthHeader ??
                    throw new ArgumentException(nameof(TimeTasticSettings.AuthHeader)));
                if (allDepartments == null || !allDepartments.Success || allDepartments.Departments == null)
                {
                    result.Message = allDepartments?.Message ?? DepartmentListResult.DepartmentsWasNotFound;
                    return result;
                }

                var allRocketChatUsers = await _rocketChatService.GetAllUsers(rocketChatToken, rocketChatPassword);
                if (!allRocketChatUsers.Success || allRocketChatUsers.Users == null)
                {
                    result.Message = allRocketChatUsers.Message ?? GetAllRocketChatUsersResult.UserNotFound;
                    return result;
                }

                foreach (var time in timeOffs)
                {
                    var timeTasticUserResult = await _timeTasticService.GetTimetasticUser(time.UserId, TimeTasticSettings.AuthHeader);
                    var timeTasticRequester = timeTasticUserResult.TimetasticUser;
                    if (!timeTasticUserResult.Success || timeTasticRequester == null || string.IsNullOrWhiteSpace(timeTasticRequester.DepartmentName))
                    {
                        result.Message = timeTasticUserResult.Message ?? GetTimeTasticUserResult.UserNotFound(time.UserId);
                        return result;
                    }
                    var managerId = allDepartments.Departments
                        .FirstOrDefault(x => x.Name == timeTasticRequester.DepartmentName)?.BossId;

                    if (managerId == null)
                    {
                        result.Message = $"Manager Id of {timeTasticRequester.DepartmentName} in Timetastic was not found";
                        return result;
                    }

                    var timeTasticManagerResult = await _timeTasticService.GetTimetasticUser((long)managerId,
                        TimeTasticSettings.AuthHeader);
                    var timeTastucManager = timeTasticManagerResult.TimetasticUser;

                    if (!timeTasticManagerResult.Success || timeTastucManager == null)
                    {
                        result.Message = timeTasticManagerResult.Message ?? GetTimeTasticUserResult.UserNotFound((long)managerId);
                        return result;
                    }

                    var rocketChatManager = allRocketChatUsers.Users.GetRocketChatUserByTimeTastic(timeTastucManager);

                    if (rocketChatManager == null || string.IsNullOrEmpty(rocketChatManager.UserName))
                    {
                        result.Message = RocketChatUser.UserWasNotFoundInTimeTastic;
                        return result;
                    }

                    var teamToAdd = grouped.Find(x => x.userNameToSend == rocketChatManager.UserName ||
                     rocketChatManager.Emails != null && rocketChatManager.Emails.Any(e => e.Address == x.emailToSend));

                    if (teamToAdd.userNameToSend == null && rocketChatManager.UserName != null
                        && timeTastucManager.Email != null)
                    {
                        grouped.Add((rocketChatManager.UserName, timeTastucManager.Email, new List<TimeOff>() { time }));
                    }
                    else
                    {
                        teamToAdd.times.Add(time);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                return result;
            }
            result.DataResult = grouped;
            result.Success = true;
            return result;
        }

        public async Task<GroupedTimeOffsByYandexManagerResult> GetGroupedTimeOffsByYandexApiTeamManager(
            IEnumerable<TimeOff> timeOffs, bool createChat = true)
        {
            var result = new GroupedTimeOffsByYandexManagerResult();
            var grouped = new List<(string userNameToSend, string emailToSend, string chatId, List<TimeOff> times)>();
            try
            {
                var allDepartments = await _timeTasticService.GetAllDepartments(TimeTasticSettings.AuthHeader ??
                   throw new ArgumentException(nameof(TimeTasticSettings.AuthHeader)));

                if (allDepartments == null || !allDepartments.Success || allDepartments.Departments == null)
                {
                    result.Message = allDepartments?.Message ?? DepartmentListResult.DepartmentsWasNotFound;
                    return result;
                }

                foreach (var time in timeOffs)
                {
                    var timeTasticRequesterResult = await _timeTasticService.GetTimetasticUser(time.UserId, TimeTasticSettings.AuthHeader);
                    var timeOffRequester = timeTasticRequesterResult.TimetasticUser;
                    if (!timeTasticRequesterResult.Success || timeOffRequester == null
                        || timeOffRequester.DepartmentName == null)
                    {
                        result.Message = timeTasticRequesterResult.Message ?? GetTimeTasticUserResult.UserNotFound(time.UserId);
                        return result;
                    }
                    var managerId = allDepartments.Departments.FirstOrDefault(x => x.Name == timeOffRequester.DepartmentName)?.BossId;

                    if (managerId == null)
                    {
                        result.Message = $"Manager Id of {timeOffRequester.DepartmentName} in Timetastic was not found";
                        return result;
                    }

                    var timeTasticManagerResult = await _timeTasticService.GetTimetasticUser((long)managerId,
                        TimeTasticSettings.AuthHeader);

                    var timeTasticManager = timeTasticManagerResult?.TimetasticUser;

                    if (timeTasticManagerResult == null || timeTasticManager == null || !timeTasticManagerResult.Success ||
                         string.IsNullOrWhiteSpace(timeTasticManager.DepartmentName))
                    {
                        result.Message = timeTasticManagerResult?.Message ?? GetTimeTasticUserResult.UserNotFound((long)managerId);
                        return result;
                    }

                    var yandexManager = await _yandexApiService.GetManagerByTeam(timeOffRequester.DepartmentName);

                    if (!yandexManager.Success || yandexManager.Manager is null
                        || string.IsNullOrWhiteSpace(yandexManager.Manager.Login))
                    {
                        result.Message = yandexManager.Message ?? GetManagerByTeamResult.ManagerOfDepartmentWasnotFound(timeOffRequester.DepartmentName);
                        return result;
                    }

                    var teamToAdd = grouped.Find(x => x.userNameToSend == yandexManager.Manager.Login
                    || x.emailToSend == yandexManager.Manager.Login);

                    if (teamToAdd.userNameToSend == null && !string.IsNullOrWhiteSpace(yandexManager.Manager.Login)
                          && timeTasticManager.Email != null)
                    {
                        var foramtedEmail = timeTasticManager.Email.Replace("@", "");
                        foramtedEmail = foramtedEmail.Replace(".com", "");
                        foramtedEmail = foramtedEmail.Replace(".pro", "");


                        grouped.Add((yandexManager.Manager.Login,
                            timeTasticManager.Email,
                            _yandexChatConfiguration.GeneralChatId,
                            new List<TimeOff>() { time }));
                    }
                    else
                    {
                        teamToAdd.times.Add(time);
                    }
                }
                foreach (var team in grouped)
                {
                    var addUserToChatResult = await _yandexApiService.UpdateMembersInChat(new string[]
                        { team.userNameToSend }
                         , team.chatId);
                    if (!addUserToChatResult.Success)
                    {
                        result.Message = addUserToChatResult.Message;
                        result.Success = false;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                return result;
            }
            result.DataResult = grouped;
            result.Success = true;
            return result;
        }
    }
}
