using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.Rocket;

namespace AbsenceNotifier.Core.DTOs.Results.RocketChat
{
    public class GetChannelInfoResult : BaseResult
    {
        public Channel? Channel { get; set; }
    }
}
