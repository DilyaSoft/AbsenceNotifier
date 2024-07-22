using System.ComponentModel;

namespace AbsenceNotifier.Core.Enums
{
    public enum TimeOffType
    {
        [Description("Vacation")]
        Vacation = 0,
        [Description("Unpaid Leave")]
        UnpaidLeave = 1,
        [Description("Sick Leave")]
        SickLeave = 2,
        [Description("Maternity")]
        Maternity = 3,
        [Description("Paternity")]
        Paternity = 4,
        [Description("Meeting")]
        Meeting = 5
    }
}
