using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.Users;

namespace AbsenceNotifier.Core.DTOs.Results.TimeTastic
{
    public class GetDepartmentManagerResult : BaseResult
    {
        public TimetasticUser? Manager { get; set; }
    }
}
