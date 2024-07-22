using AbsenceNotifier.Core.DTOs.TimeTastic;
using System.Collections;

namespace AbsenceNotifier.Core.Tests.TestData
{
    public class SendOneSuccessRequestToManagerTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { 
                new TimeOff() 
               {
                  TimeOffType = Enums.TimeOffType.Vacation,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(2),
                    Status = Enums.RequestStatus.Pending
               },
               new TimeOff()
               {
                  TimeOffType = Enums.TimeOffType.Vacation,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(3),
                    Status = Enums.RequestStatus.Pending
               }
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
