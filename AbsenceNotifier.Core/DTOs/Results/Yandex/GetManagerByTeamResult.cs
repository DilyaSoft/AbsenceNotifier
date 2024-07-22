using AbsenceNotifier.Core.DTOs.Results.Base;
using AbsenceNotifier.Core.DTOs.Yandex;

namespace AbsenceNotifier.Core.DTOs.Results.Yandex
{
    public sealed class GetManagerByTeamResult : BaseResult
    {
        public static string ManagerOfDepartmentWasnotFound(string dep) => $"Yandex manager of departmnet {dep} was not found";
        public YandexUser? Manager { get; set; }
    }
}
