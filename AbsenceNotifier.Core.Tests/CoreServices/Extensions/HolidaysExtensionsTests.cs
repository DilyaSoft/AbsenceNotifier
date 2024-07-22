using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.Enums;
using AbsenceNotifier.Core.Helpers;
using System.Reflection;

namespace AbsenceNotifier.Tests.CoreServices.Extensions
{
    public class HolidaysExtensionsTests
    {
        [Fact]
        public void GetOnlyApprovedTimeOff()
        {
            // arrange
            var expected = new List<TimeOff>()
            {
                new TimeOff()
                {
                    Id = 1,
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(9),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 2,
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(8),
                    Status = RequestStatus.Approved
                },
            };
            var timeOffs = new List<TimeOff>()
            {
                 new TimeOff()
                {
                    Id = 1,
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(9),
                    Status = RequestStatus.Declined
                },
                new TimeOff()
                {
                    Id = 2,
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(8),
                    Status = RequestStatus.Cancelled
                },
            };
            timeOffs.AddRange(expected);

            // act
            var activeResult = expected.GetOnlyApproved();

            // assert
            Assert.True(activeResult.SequenceEqual(expected));
        }

        [Fact]
        public void GetActiveTimeOffTest()
        {
            // arrange
            var expected = new List<TimeOff>()
            {
                new TimeOff()
                {
                    Id = 1,
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(9),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 2,
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(8),
                    Status = RequestStatus.Pending
                },
            };
            var timeOffs = new List<TimeOff>()
            {
                 new TimeOff()
                {
                    Id = 1,
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(9),
                    Status = RequestStatus.Declined
                },
                new TimeOff()
                {
                    Id = 2,
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(8),
                    Status = RequestStatus.Cancelled
                },
            };
            timeOffs.AddRange(expected);

            // act
            var activeResult = expected.GetActive();

            // assert
            Assert.True(activeResult.SequenceEqual(expected));

        }

        [Fact]
        public void GetTimeOffForGeneralRoomBeforeWeek()
        {
            // arrange
            var expectedWeek = new List<TimeOff>()
            {
                new TimeOff()
                {
                    Id = 1,
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(9),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 2,
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(8),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 3,
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(10),
                    Status = RequestStatus.Approved
                },
            };
            var timeOffs = new List<TimeOff>()
            {
                new TimeOff()
                {
                    Id = 4,
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(3),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 5,
                    StartDate = DateTime.UtcNow.AddDays(4),
                    EndDate = DateTime.UtcNow.AddDays(6),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 6,
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(7),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 7,
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(9),
                    Status = RequestStatus.Pending
                },
            };
            timeOffs.AddRange(expectedWeek);

            // act
            var weekResult = timeOffs.GetForGeneralRoomBeforeWeek();

            // assert
            Assert.True(weekResult.SequenceEqual(expectedWeek));

        }

        [Fact]
        public void GetTimeOffForGeneralRoomSendToday()
        {
            // arrange
            var ruTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow,
              TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            var expectedWeek = new List<TimeOff>()
            {
                new TimeOff()
                {
                    Id = 1,
                    StartDate = ruTime,
                    EndDate = DateTime.Now.AddDays(9),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 2,
                    StartDate = ruTime,
                    EndDate = DateTime.Now.AddDays(8),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 3,
                    StartDate = ruTime,
                    EndDate = DateTime.Now.AddDays(10),
                    Status = RequestStatus.Approved
                },
            };
            var timeOffs = new List<TimeOff>()
            {
                new TimeOff()
                {
                    Id = 4,
                    StartDate = DateTime.Now.AddDays(2),
                    EndDate = DateTime.Now.AddDays(3),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 5,
                    StartDate = DateTime.Now.AddDays(4),
                    EndDate = DateTime.Now.AddDays(6),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 6,
                    StartDate = DateTime.Now.AddDays(2),
                    EndDate = DateTime.Now.AddDays(7),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 7,
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(9),
                    Status = RequestStatus.Pending
                },
            };
            timeOffs.AddRange(expectedWeek);

            // act
            var weekResult = timeOffs.GetForGeneralRoomSendToday();

            // assert
            Assert.True(weekResult.SequenceEqual(expectedWeek));
        }

        [Fact]
        public void GetTimeOffToSendTodayToApprover()
        {
            // arrange
            var ruTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow,
              TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            var expectedWeek = new List<TimeOff>()
            {
                new TimeOff()
                {
                    Id = 1,
                    StartDate = ruTime,
                    EndDate = DateTime.Now.AddDays(9),
                    Status = RequestStatus.Pending
                },
                new TimeOff()
                {
                    Id = 2,
                    StartDate = ruTime,
                    EndDate = DateTime.Now.AddDays(8),
                    Status = RequestStatus.Pending
                },
                new TimeOff()
                {
                    Id = 3,
                    StartDate = ruTime,
                    EndDate = DateTime.Now.AddDays(10),
                    Status = RequestStatus.Pending
                },
            };
            var timeOffs = new List<TimeOff>()
            {
                new TimeOff()
                {
                    Id = 4,
                    StartDate = DateTime.Now.AddDays(2),
                    EndDate = DateTime.Now.AddDays(3),
                    Status = RequestStatus.Declined
                },
                new TimeOff()
                {
                    Id = 5,
                    StartDate = DateTime.Now.AddDays(4),
                    EndDate = DateTime.Now.AddDays(6),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 6,
                    StartDate = DateTime.Now.AddDays(2),
                    EndDate = DateTime.Now.AddDays(7),
                    Status = RequestStatus.Approved
                },
            };
            timeOffs.AddRange(expectedWeek);

            // act
            var weekResult = timeOffs.GetToApprover();

            // assert
            Assert.True(weekResult.SequenceEqual(expectedWeek));
        }

        [Fact]
        public void GetTimeOffToSendBeforeWeekToApprover()
        {
            // arrange
            var expectedWeek = new List<TimeOff>()
            {
                new TimeOff()
                {
                    Id = 1,
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(9),
                    Status = RequestStatus.Pending
                },
                new TimeOff()
                {
                    Id = 2,
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(8),
                    Status = RequestStatus.Pending
                },
                new TimeOff()
                {
                    Id = 3,
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(10),
                    Status = RequestStatus.Pending
                },
            };
            var timeOffs = new List<TimeOff>()
            {
                new TimeOff()
                {
                    Id = 4,
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(3),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 5,
                    StartDate = DateTime.UtcNow.AddDays(4),
                    EndDate = DateTime.UtcNow.AddDays(6),
                    Status = RequestStatus.Approved
                },
                new TimeOff()
                {
                    Id = 6,
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(7),
                    Status = RequestStatus.Cancelled
                },
                new TimeOff()
                {
                    Id = 7,
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(9),
                    Status = RequestStatus.Approved
                },
            };
            timeOffs.AddRange(expectedWeek);

            // act
            var weekResult = timeOffs.GetToSendBeforeWeekToApprover();

            // assert
            Assert.True(weekResult.SequenceEqual(expectedWeek));
        }


        [Fact]
        public void IsSequenceTimeOffsTest()
        {
            // arrange
            var startOfTheCurrent = StartOfWeek(DateTime.UtcNow, DayOfWeek.Monday);

            var notLastBeforeHolidays = new TimeOff()
            {
                StartDate = startOfTheCurrent,
                EndDate = startOfTheCurrent.AddDays(1)
            };

            var nextHoliday = new TimeOff()
            {
                StartDate = notLastBeforeHolidays.EndDate.AddDays(1)
            };

            var fridayDate = startOfTheCurrent.AddDays(4);
            var timeOffThroughTheHolidaysDate = fridayDate.AddDays(3);

            var friday = new TimeOff()
            {
                EndDate = fridayDate,
            };
            var holidaysNext = new TimeOff()
            {
                StartDate = timeOffThroughTheHolidaysDate
            };

            // act
            var resultNextDay = HolidaysExtensions.IsSequence(notLastBeforeHolidays, nextHoliday);
            var resultThroughHolidays = HolidaysExtensions.IsSequence(friday, holidaysNext);

            // assert
            Assert.True(resultNextDay);
            Assert.True(resultThroughHolidays);
        }

        [Fact]
        public void IsNotSequenceTimeOffTest()
        {
            // arrange
            var startOfTheCurrent = StartOfWeek(DateTime.UtcNow, DayOfWeek.Monday);

            var notLastBeforeHolidays = new TimeOff()
            {
                StartDate = startOfTheCurrent,
                EndDate = startOfTheCurrent.AddDays(1)
            };

            var nextHoliday = new TimeOff()
            {
                StartDate = notLastBeforeHolidays.EndDate.AddDays(2)
            };

            var fridayDate = startOfTheCurrent.AddDays(4);
            var timeOffThroughTheHolidaysDate = fridayDate.AddDays(5);

            var friday = new TimeOff()
            {
                EndDate = fridayDate,
            };
            var holidaysNext = new TimeOff()
            {
                StartDate = timeOffThroughTheHolidaysDate
            };

            // act
            var resultNextDay = HolidaysExtensions.IsSequence(notLastBeforeHolidays, nextHoliday);
            var resultThroughHolidays = HolidaysExtensions.IsSequence(friday, holidaysNext);

            // assert
            Assert.False(resultNextDay);
            Assert.False(resultThroughHolidays);
        }

        [Fact]
        public void MergeSequenceTimeOffTest()
        {
            // arrange
            var startOfTheCurrent = StartOfWeek(DateTime.UtcNow, DayOfWeek.Monday);

            var notLastBeforeHolidays = new TimeOff()
            {
                Id = 1,
                LeaveType = "Vacation",
                TimeOffType = TimeOffType.Vacation,
                StartDate = startOfTheCurrent,
                EndDate = startOfTheCurrent.AddDays(1)
            };

            var nextHoliday = new TimeOff()
            {
                Id = 2,
                LeaveType = "Vacation",
                TimeOffType = TimeOffType.Vacation,
                StartDate = notLastBeforeHolidays.EndDate.AddDays(1),
                EndDate = notLastBeforeHolidays.EndDate.AddDays(2)
            };

            var nextHoliday2 = new TimeOff()
            {
                Id = 3,
                LeaveType = "Vacation",
                TimeOffType = TimeOffType.Vacation,
                StartDate = nextHoliday.EndDate.AddDays(1)
            };

            var fridayDate = startOfTheCurrent.AddDays(4);
            var timeOffThroughTheHolidaysDate = fridayDate.AddDays(3);

            var friday = new TimeOff()
            {
                Id = 4,
                EndDate = fridayDate,
            };
            var holidaysNext = new TimeOff()
            {
                Id = 5,
                LeaveType = "Vacation",
                TimeOffType = TimeOffType.Vacation,
                StartDate = timeOffThroughTheHolidaysDate,
                EndDate = timeOffThroughTheHolidaysDate.AddDays(1),
            };

            var holidaysNext2 = new TimeOff()
            {
                Id = 6,
                LeaveType = "Vacation",
                TimeOffType = TimeOffType.Vacation,
                StartDate = holidaysNext.EndDate.AddDays(1)
            };

            var testNextDay = new List<TimeOff>() { notLastBeforeHolidays, nextHoliday, nextHoliday2 };
            var testFridayAndHolidays = new List<TimeOff>() { friday, holidaysNext, holidaysNext2 };

            // act
            var mergedNextDays = testNextDay.MergeSequentialByDatesWithWeeklyHolidays();
            var mergedFridayHolidays = testFridayAndHolidays.MergeSequentialByDatesWithWeeklyHolidays();


            // assert
            Assert.True(mergedNextDays.Count() == 1);
            Assert.True(mergedNextDays.First().Sequences.Count == testNextDay.Count);

            Assert.True(mergedFridayHolidays.Count() == 1);
            Assert.True(mergedFridayHolidays.First().Sequences.Count == testFridayAndHolidays.Count);
        }

        public DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
