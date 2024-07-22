using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.Rocket;

namespace AbsenceNotifier.Core.DTOs.Results.RocketChat
{
    public class CreateRoomResult : BaseResult
    {
        public const string DefaultError = "Cannot create a room with user to send message";
        public string? Status { get; set; }
        public Room? Room { get; set; }
    }
}
