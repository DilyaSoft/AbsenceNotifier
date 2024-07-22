using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.Enums;

namespace AbsenceNotifier.Core.Helpers
{
    public static class HolidaysExtensions
    {
        private readonly static string _defaultTimeZone = "Central European Standard Time";
        public static bool IsTodayEventStartDate(this DateTime date)
        {
            var ruTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById(_defaultTimeZone));

            return date.Year == ruTime.Year && date.Month == ruTime.Month
              && date.Day == ruTime.Day;
        }

        public static bool IsBeforeWeekStartDate(this DateTime date)
        {
            var ruTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow,
               TimeZoneInfo.FindSystemTimeZoneById(_defaultTimeZone));

            return (date - ruTime).Days == 6;
        }

        public static IEnumerable<TimeOff> GetToSendBeforeWeekToApprover(this IEnumerable<TimeOff> timeOffs)
        {
            return timeOffs.Where(x => x.StartDate.IsBeforeWeekStartDate() &&
                x.Status == RequestStatus.Pending);
        }

        public static IEnumerable<TimeOff> GetToApprover(this IEnumerable<TimeOff> timeOffs)
        {
            var ruTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow,
               TimeZoneInfo.FindSystemTimeZoneById(_defaultTimeZone));

            return timeOffs.Where(x => (x.Status == RequestStatus.Pending) && ((x.EndDate.Year > ruTime.Year)
            || (x.EndDate.Year == ruTime.Year && x.EndDate.Month > ruTime.Month) || (x.EndDate.Year == ruTime.Year
                  && x.EndDate.Month == ruTime.Month && x.EndDate.Day >= ruTime.Day)));
        }

        public static IEnumerable<TimeOff> GetForGeneralRoomBeforeWeek(this IEnumerable<TimeOff> timeOffs)
        {
            return timeOffs.Where(x => x.StartDate.IsBeforeWeekStartDate() && x.Status == RequestStatus.Approved);
        }

        public static IEnumerable<TimeOff> GetForGeneralRoomSendToday(this IEnumerable<TimeOff> timeOffs)
        {
            return timeOffs.Where(x => x.StartDate.IsTodayEventStartDate() && x.Status == RequestStatus.Approved);
        }

        public static IEnumerable<TimeOff> GetActive(this IEnumerable<TimeOff> timeOffs)
        {
            return timeOffs.Where(x => x.Status != RequestStatus.Cancelled &&
                x.Status != RequestStatus.Declined);
        }

        public static IEnumerable<TimeOff> GetOnlyApproved(this IEnumerable<TimeOff> timeOffs)
        {
            return timeOffs.Where(x => x.Status == RequestStatus.Approved);
        }

        public static IEnumerable<TimeOff> GetByUserName(this IEnumerable<TimeOff> offs, string name)
        {
            return offs.Where(x => x.UserName == name);
        }

        public static IEnumerable<TimeOff> MergeSequentialByDatesWithWeeklyHolidays(
            this IEnumerable<TimeOff> timeOffs)
        {
            var merged = new List<TimeOff>();
            var ordered = timeOffs.OrderBy(x => x.StartDate);

            var mergedIds = new Stack<(long, List<TimeOff>)>();
            bool isSequence = false;

            foreach (var timeOff in ordered)
            {
                var timeOffToMerge = ordered.FirstOrDefault(x =>

                    IsSequence(timeOff, x)
                );

                if (timeOffToMerge != null)
                {
                    if (isSequence)
                    {
                        var last = mergedIds.Pop();
                        last.Item2.Add(timeOffToMerge);
                        mergedIds.Push(last);
                    }
                    else
                    {
                        var added = mergedIds.FirstOrDefault(x => x.Item1 == timeOff.Id);
                        if (added != default)
                        {
                            added.Item2.Add(timeOffToMerge);
                        }
                        else
                        {
                            mergedIds.Push((timeOff.Id, new List<TimeOff> { timeOff, timeOffToMerge }));
                        }
                    }
                    isSequence = true;
                }
                else
                {
                    if (!isSequence)
                    {
                        merged.Add(timeOff);
                    }
                    isSequence = false;
                }

            }

            foreach (var timeOff in mergedIds)
            {
                var origin = timeOffs.FirstOrDefault(x => x.Id == timeOff.Item1);
                origin.Sequences = new List<TimeOff>();
                origin.Sequences.AddRange(timeOff.Item2);
                merged.Add(origin);
            }

            return merged;
        }

        public static bool IsSequence(TimeOff first, TimeOff second)
        {
            var diff = second.StartDate - first.EndDate;
            var diffDays = diff.Days;

            var sameType = first.TimeOffType == second.TimeOffType || first.LeaveType == second.LeaveType;

            if (sameType && first.EndDate.DayOfWeek == DayOfWeek.Friday && diffDays == 3)
            {
                return true;
            }

            return sameType && diffDays == 1;
        }

    }
}
