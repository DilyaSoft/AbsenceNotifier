using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.TimeTastic;

namespace AbsenceNotifier.Core.DTOs.Results.TimeTastic
{
    public class GetDepartmentByEmployeeResult : BaseResult
    {
        public Department? Department { get; set; }
    }
}
