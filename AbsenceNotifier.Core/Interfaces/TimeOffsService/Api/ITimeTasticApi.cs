using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.DTOs.Users;
using Refit;

namespace AbsenceNotifier.Core.Interfaces.TimeOffsService.Api
{
    public interface ITimeTasticApi
    {
        [Get("/api/holidays")]
        public Task<TimeOffListResponse> GetTotalTimeOffList([Header("Authorization")] string bearerToken);

        [Get("/api/users/{id}")]
        public Task<TimetasticUser> GetTimetasticUser([AliasAs("id")] long userId, [Header("Authorization")] string bearerToken);
        [Get("/api/holidays/{id}")]
        public Task<TimeOff> GetTimeOffDetailsById([AliasAs("id")] long id, [Header("Authorization")] string bearerToken);

        [Get("/api/departments")]
        public Task<List<Department>> GetDepartments([Header("Authorization")] string bearerToken);

        [Post("/api/holidays/{id}?holidayUpdateAction={action}")]
        public Task<HttpResponseMessage> EditHoliday([AliasAs("id")] long holidayId,
            [AliasAs("action")] int action,
            [Header("Authorization")] string bearerToken,
            [Header("Content-Type")] string contentType,
            [Body] EditHolidayBody? body = null);
    }
}
