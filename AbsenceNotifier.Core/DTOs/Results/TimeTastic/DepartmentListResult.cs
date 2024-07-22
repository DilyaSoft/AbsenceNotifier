using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.TimeTastic;

namespace AbsenceNotifier.Core.DTOs.Results.TimeTastic
{
    public class DepartmentListResult : BaseResult
    {
        public const string DepartmentsWasNotFound = "Timetastic departments not found";
        public IEnumerable<Department>? Departments { get; set; }
    }
}
