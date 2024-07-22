using AbsenceNotifier.Core.DTOs.Results;
using AbsenceNotifier.Core.DTOs.Results.RocketChat;
using AbsenceNotifier.Core.DTOs.Results.Yandex;
using AbsenceNotifier.Core.DTOs.TimeTastic;

namespace AbsenceNotifier.Core.Interfaces.TimeOffsService
{
    public interface ITimeOffHelper
    {
        public Task<GroupedTimeOffsByRocketManagerResult> GetGroupedTimeOffsByRocketChatTeamManager(
            IEnumerable<TimeOff> timeOffs,
            string rocketChatToken,
            string rocketChatPassword);

        public Task<GroupedTimeOffsByYandexManagerResult> GetGroupedTimeOffsByYandexApiTeamManager(IEnumerable<TimeOff> timeOffs, 
            bool createChat = true);
    }
}
