using AbsenceNotifier.Core.DTOs.Results;
using AbsenceNotifier.Core.DTOs.Results.TimeTastic;
using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.DTOs.Users;
using AbsenceNotifier.Core.Enums;
using AbsenceNotifier.Core.Interfaces.Messengers;

namespace AbsenceNotifier.Core.Services.Messengers
{
    public class MessengerContext
    {
        private IMessenger? _messenger;
        private readonly YandexMessenger _yandexMessenger;
        private readonly RockectChatMessenger _rockectChatMessenger;

        public MessengerContext(YandexMessenger yandexMessenger, RockectChatMessenger rockectChatMessenger)
        {
            _yandexMessenger = yandexMessenger;
            _rockectChatMessenger = rockectChatMessenger;
        }

        public void SetMessenger(string? messengerName) 
        {
            switch (messengerName)
            {
                case "Yandex":
                {
                    _messenger = _yandexMessenger;
                    break;
                }
                case "RocketChat":
                {
                    _messenger = _rockectChatMessenger;
                    break;
                }
                default:
                {
                    throw new ArgumentException($"Unknown messenger with provided name - {messengerName}");
                }
            }
        }

        public Task<SendToManagersResult> RunSendingRequestsToManagers(IEnumerable<TimeOff> timeOffs)
        {
            if (_messenger == null) throw new ArgumentException("Set a messenger before starting running sending");

            return _messenger.SendTimeOffRequestToManagers(timeOffs);
        }

        public Task<SendToGeneralChatResult> RunSendingApprovedTimeOffsToGeneralRoom(IEnumerable<TimeOff> timeOffs)
        {
            if (_messenger == null) throw new ArgumentException("Set a messenger before starting running sending");

            return _messenger.SendTimeOffsNotificationsToGeneralChat(timeOffs);
        }

        public Task<EditHolidayResult> SendEditHolidayResponse(TimetasticUser timetasticUser, 
             long holidayId, EditHolidayActions editHolidayAction)
        {
            if (_messenger == null) throw new ArgumentException("Set a messenger before starting running sending");

            return _messenger.SendEditHolidayResponse(timetasticUser, holidayId, editHolidayAction);
        }


    }
}
