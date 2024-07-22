using AbsenceNotifier.Core.DTOs.Results.TimeTastic;
using AbsenceNotifier.Core.DTOs.TimeTastic;

namespace AbsenceNotifier.Core.Interfaces.TimeOffsService
{
    public interface ITimeTasticService
    {
        public Task<DepartmentListResult> GetAllDepartments(string token);
        public Task<TimeOffListResponse> GetForAllTimeHolidays(string token);
        public Task<GetTimeTasticUserResult> GetTimetasticUser(long userId, string token);
        public Task<EditHolidayResult> EditHoliday(long holidayId, long userId, int action,
            string bearerToken, string contentType, EditHolidayBody? body = null);
        public Task<GetTimeOffResult> GetTimeOffDetails(long id);
        public Task<GetDepartmentByEmployeeResult> GetDepartmentNameByEmployee(long employeeId);
        public Task<GetDepartmentManagerResult> GetDepartmentManager(string departmentName);
    }
}
