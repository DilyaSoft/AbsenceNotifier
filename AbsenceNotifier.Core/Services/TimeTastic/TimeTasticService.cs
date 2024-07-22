using AbsenceNotifier.Core.Constants;
using AbsenceNotifier.Core.DTOs.Results.TimeTastic;
using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.Enums;
using AbsenceNotifier.Core.Interfaces.TimeOffsService;
using AbsenceNotifier.Core.Interfaces.TimeOffsService.Api;
using AbsenceNotifier.Core.Settings;
using Newtonsoft.Json;

namespace AbsenceNotifier.Core.Services.TimeTastic
{
    public class TimeTasticService : ITimeTasticService
    {
        private readonly ITimeTasticApi _timeTasticApi;

        public TimeTasticService(
            ITimeTasticApi timeTasticApi
           )
        {
            _timeTasticApi = timeTasticApi;
        }

        public async Task<EditHolidayResult> EditHoliday(long holidayId, long userId, int action,
            string bearerToken, string contentType, EditHolidayBody? body = null)
        {
            var result = new EditHolidayResult();
            try
            {
                if (string.IsNullOrEmpty(TimeTasticSettings.AuthHeader))
                {
                    throw new ArgumentException(nameof(TimeTasticSettings.AuthHeader));
                }

                var actionType = (EditHolidayActions)action;
                var editResult = await _timeTasticApi.EditHoliday(holidayId,
                    action, TimeTasticSettings.AuthHeader, RestApiConstants.ContentTypeJson,
                    new EditHolidayBody()
                    {
                        Reason = actionType == EditHolidayActions.Approve ? "Weekend request confirmation" : "Rejecting a weekend request"
                    });

                var resultMessage = "";

                if (!editResult.IsSuccessStatusCode)
                {
                    result.Message = EditHolidayResult.DefaultErrorMessage;
                    result.Success = false;
                    return result;
                }

                var response = JsonConvert.DeserializeObject<EditHolidayResult>(await editResult.Content.ReadAsStringAsync());
                if (response != null && !string.IsNullOrEmpty(response.ApprovalResult))
                {
                    result.ApprovalResult = response.ApprovalResult;
                    if (response.ApprovalResult == "Success")
                    {
                        resultMessage = actionType == EditHolidayActions.Approve ? "Request successfully confirmed" : "Request successfully rejected";
                        result.Success = true;
                        var timeTasticUserResult = await GetTimetasticUser(userId, TimeTasticSettings.AuthHeader);
                        if (!timeTasticUserResult.Success)
                        {
                            result.Message = timeTasticUserResult.Message;
                            result.Success = false;
                            return result;
                        }
                        var timeTasticUser = timeTasticUserResult.TimetasticUser;
                        if (timeTasticUser == null)
                        {
                            result.Message = GetTimeTasticUserResult.UserNotFound(userId);
                            result.Success = false;
                            return result;
                        }
                    }
                    if (response.ApprovalResult == "AlreadyActioned")
                    {
                        result.Success = true;
                        resultMessage = EditHolidayResult.AlreadyActioned;
                    }
                }
                else
                {
                    var messageAction = actionType == EditHolidayActions.Approve ? "approving" : "declining";
                    result.Message = $"Your request to TimeTastic for {messageAction} was successful, but the response from the server is not as expected";
                    result.Success = false;
                    return result;
                }
                result.Message = resultMessage;
            }
            catch (Exception ex)
            {
                return new EditHolidayResult
                {
                    ApprovalResult = "Failed",
                    Message = ex.Message,
                    Success = false
                };
            }

            return result;
        }

        public async Task<DepartmentListResult> GetAllDepartments(string token)
        {
            var departmentsResult = new DepartmentListResult();
            try
            {
                var departments = await _timeTasticApi.GetDepartments(token);
                departmentsResult.Success = true;
                departmentsResult.Departments = departments;
            }
            catch (Exception ex)
            {
                departmentsResult.Success = false;
                departmentsResult.Message = ex.Message;
                return departmentsResult;
            }

            return departmentsResult;
        }

        public async Task<GetDepartmentManagerResult> GetDepartmentManager(string departmentName)
        {
            var result = new GetDepartmentManagerResult();

            try
            {
                if (string.IsNullOrEmpty(TimeTasticSettings.AuthHeader))
                {
                    throw new ArgumentException(nameof(TimeTasticSettings.AuthHeader));
                }
                var allDepartments = await _timeTasticApi.GetDepartments(TimeTasticSettings.AuthHeader);
                var department = allDepartments.Find(x => x.Name == departmentName);
                if (department == null)
                {
                    result.Success = false;
                    result.Message = $"Department {departmentName} not found";
                    return result;
                }

                var manager = await _timeTasticApi.GetTimetasticUser(department.BossId, TimeTasticSettings.AuthHeader);
                if (manager == null)
                {
                    result.Success = false;
                    result.Message = GetTimeTasticUserResult.UserNotFound(department.BossId);
                    return result;
                }
                result.Manager = manager;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            return result;
        }

        public async Task<GetDepartmentByEmployeeResult> GetDepartmentNameByEmployee(long employeeId)
        {
            var result = new GetDepartmentByEmployeeResult();
            try
            {
                if (string.IsNullOrEmpty(TimeTasticSettings.AuthHeader))
                {
                    throw new ArgumentException(nameof(TimeTasticSettings.AuthHeader));
                }
                var allDepartments = await _timeTasticApi.GetDepartments(TimeTasticSettings.AuthHeader);
                var employeeDetails = await _timeTasticApi.GetTimetasticUser(employeeId, TimeTasticSettings.AuthHeader);
                var departmentEmployee = allDepartments.Find(x => x.Name == employeeDetails.DepartmentName);
                if (departmentEmployee == null)
                {
                    result.Success = false;
                    result.Message = $"Department with employee id {employeeId} not found";
                    return result;
                }
                result.Department = departmentEmployee;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
                return result;
            }
            return result;
        }

        public async Task<TimeOffListResponse> GetForAllTimeHolidays(string token)
        {
            var result = new TimeOffListResponse();
            try
            {
                result = await _timeTasticApi.GetTotalTimeOffList(token);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            return result;
        }

        public async Task<GetTimeOffResult> GetTimeOffDetails(long id)
        {
            var result = new GetTimeOffResult();
            try
            {
                if (string.IsNullOrEmpty(TimeTasticSettings.AuthHeader))
                {
                    throw new ArgumentException(nameof(TimeTasticSettings.AuthHeader));
                }
                var timeOff = await _timeTasticApi.GetTimeOffDetailsById(id, TimeTasticSettings.AuthHeader);
                result.Success = true;
                result.TimeOff = timeOff;
            }
            catch (Exception ex)
            {
                result.Message += ex.Message;
                result.Success = false;
            }
            return result;
        }

        public async Task<GetTimeTasticUserResult> GetTimetasticUser(long userId, string token)
        {
            var result = new GetTimeTasticUserResult();
            try
            {
                var user = await _timeTasticApi.GetTimetasticUser(userId, token);
                result.TimetasticUser = user;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                return result;
            }
            return result;
        }
    }
}
