using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.Users;

namespace AbsenceNotifier.Core.DTOs.Results.TimeTastic
{
    public sealed class GetTimeTasticUserResult : BaseResult
    {
        public static string UserNotFound(long id) => $"Timetastic user was not found with id {id}";
        public TimetasticUser? TimetasticUser { get; set; }
    }
}
