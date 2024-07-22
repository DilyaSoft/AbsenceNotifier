using AbsenceNotifier.Core.DTOs.Results;
using AbsenceNotifier.Core.DTOs.Results.TimeTastic;
using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.DTOs.Users;
using AbsenceNotifier.Core.Enums;

namespace AbsenceNotifier.Core.Interfaces.Messengers
{
    public interface IMessenger
    {
        public Task<SendToManagersResult> SendTimeOffRequestToManagers(IEnumerable<TimeOff> timeOffs);
        public Task<SendToGeneralChatResult> SendTimeOffsNotificationsToGeneralChat(IEnumerable<TimeOff> timeOffs);
        public Task<EditHolidayResult> SendEditHolidayResponse(TimetasticUser timetasticUser, long holidayId,
            EditHolidayActions holidayAction);
    }
}
