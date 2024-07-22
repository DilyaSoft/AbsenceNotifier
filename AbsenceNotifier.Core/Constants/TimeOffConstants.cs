using AbsenceNotifier.Core.Enums;
using System.Collections.ObjectModel;

namespace AbsenceNotifier.Core.Constants
{
    public static class TimeOffConstants
    {
        public const string TitleForManagerTimeOffs = "List of active and pending requests";
        private static readonly ReadOnlyDictionary<TimeOffType, string> _timeOffTypesNames = new(new Dictionary<TimeOffType, string>()
        {
            {TimeOffType.Vacation, "Vacation" },
            {TimeOffType.UnpaidLeave, "Unpaid leave" },
            {TimeOffType.SickLeave, "Sick leave" },
            {TimeOffType.Maternity, "Maternity" },
            {TimeOffType.Paternity, "Maternity" },
            {TimeOffType.Meeting, "Meeting" }
        });

        private static readonly ReadOnlyDictionary<TimeOffType, string> _timeOffMarkupTypesIcons = new(new Dictionary<TimeOffType, string>()
        {
            {TimeOffType.Vacation, ":airplane:" },
            {TimeOffType.UnpaidLeave, ":mega:" },
            {TimeOffType.SickLeave, ":pill:" },
            {TimeOffType.Maternity, ":family_woman_girl:" },
            {TimeOffType.Paternity, ":family_man_girl:" },
            {TimeOffType.Meeting, ":e-mail:" }
        });

        private static readonly ReadOnlyDictionary<TimeOffType, string> _timeOffUnicodeTypesIcons = new(new Dictionary<TimeOffType, string>()
        {
            {TimeOffType.Vacation, "✈️" },
            {TimeOffType.UnpaidLeave, "📣" },
            {TimeOffType.SickLeave, "💊" },
#pragma warning disable S2479 // Whitespace and control characters in string literals should be explicit
            {TimeOffType.Maternity,"👩‍👧" },
#pragma warning restore S2479 // Whitespace and control characters in string literals should be explicit
#pragma warning disable S2479 // Whitespace and control characters in string literals should be explicit
            {TimeOffType.Paternity, "👨‍👧" },
#pragma warning restore S2479 // Whitespace and control characters in string literals should be explicit
            {TimeOffType.Meeting, "📧" }
        });

        private static readonly ReadOnlyDictionary<TimeOffType, string> _timeOffTypesRuDescriptions = new(new Dictionary<TimeOffType, string>()
        {
            {TimeOffType.Vacation, "will have a vacation" },
            {TimeOffType.UnpaidLeave, "unpaid leave" },
            {TimeOffType.SickLeave, "will have a sick leave" },
            {TimeOffType.Maternity, "women's maternity leave" },
            {TimeOffType.Paternity, "men's maternity leave" },
            {TimeOffType.Meeting, "will have a meeting" }
        });

        private static readonly ReadOnlyDictionary<RequestStatus, string> _ruRequestStatusNames = new(new Dictionary<RequestStatus, string>()
        {
            {RequestStatus.Approved, "Approved" },
            {RequestStatus.Pending, "Panding" },
            {RequestStatus.Cancelled, "Cancelled" },
            {RequestStatus.Declined, "Declined" },
        });

        private static readonly ReadOnlyDictionary<RequestStatus, string> _ruRequestMarkupStatusIcons = new(new Dictionary<RequestStatus, string>()
        {
            {RequestStatus.Approved, ":white_check_mark:" },
            {RequestStatus.Pending, ":stopwatch:" },
            {RequestStatus.Cancelled, ":multiply:" },
            {RequestStatus.Declined, ":cross_mark:" },
        });

        private static readonly ReadOnlyDictionary<RequestStatus, string> _ruRequestStatusUnicodeIcons = new(new Dictionary<RequestStatus, string>()
        {
            {RequestStatus.Approved, "✅" },
            {RequestStatus.Pending, "⏱️" },
            {RequestStatus.Declined, "✖️" },
            {RequestStatus.Cancelled, "❌" },
        });

        public static ReadOnlyDictionary<TimeOffType, string> TimeOffTypesNamesDict
        {
            get
            {
                return _timeOffTypesNames;
            }
        }

        public static ReadOnlyDictionary<TimeOffType, string> TimeOffRocketChatTypesIconsDict
        {
            get
            {
                return _timeOffMarkupTypesIcons;
            }
        }

        public static ReadOnlyDictionary<TimeOffType, string> TimeOffUnicodeTypesIconsDict
        {
            get
            {
                return _timeOffUnicodeTypesIcons;
            }
        }

        public static ReadOnlyDictionary<TimeOffType, string> TimeOffTypesRuDescriptionDict
        {
            get
            {
                return _timeOffTypesRuDescriptions;
            }
        }

        public static ReadOnlyDictionary<RequestStatus, string> RequestStatusNameDict
        {
            get
            {
                return _ruRequestStatusNames;
            }
        }

        public static ReadOnlyDictionary<RequestStatus, string> RequestRocketChatStatusIcons
        {
            get
            {
                return _ruRequestMarkupStatusIcons;
            }
        }

        public static ReadOnlyDictionary<RequestStatus, string> RequestStatusUnicodeIcons
        {
            get
            {
                return _ruRequestStatusUnicodeIcons;
            }
        }

        public const string EmailSubject = "Vacation notice";
    }
}
