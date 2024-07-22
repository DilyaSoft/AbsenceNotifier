using AbsenceNotifier.Core.Configurations;
using AbsenceNotifier.Core.Constants;
using AbsenceNotifier.Core.DTOs;
using AbsenceNotifier.Core.DTOs.TimeTastic;
using AbsenceNotifier.Core.Enums;
using AbsenceNotifier.Core.Helpers;
using AbsenceNotifier.Core.Interfaces;
using Moq;

namespace AbsenceNotifier.Core.Tests.CoreServices.Helpers
{
    public class TimeOffReportMessageBuilderTests
    {
        private readonly TimeOffReportMessageBuilder _timeOffReportMessageBuilder;
        public TimeOffReportMessageBuilderTests()
        {
            var dataProvider = new Mock<IDataProtectionProviderHelper>();
            dataProvider.Setup(x => x.GetEncryptedValue(It.IsAny<string>())).Returns(new DTOs.Results.DataProtectionResult()
            {
                Success = true,
                Value = "test"
            });
            var appSettings = new ApplicationCommonConfiguration();
            _timeOffReportMessageBuilder = new TimeOffReportMessageBuilder(dataProvider.Object, appSettings);
        }

        [Fact]
        public void AllTimeOffTypesExistsInDictionary()
        {
            // arrange
            var typesNames = Enum.GetNames(typeof(TimeOffType));

            // act 
            foreach (var type in typesNames)
            {
                var enumType = (TimeOffType)Enum.Parse(typeof(TimeOffType), type);
                var ruNameExists = TimeOffConstants.TimeOffTypesNamesDict.TryGetValue(enumType, out var name);
                var ruDescriptionExists = TimeOffConstants.TimeOffTypesRuDescriptionDict.TryGetValue(enumType, out var description);


                // assert
                Assert.True(ruNameExists);
                Assert.True(ruDescriptionExists);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetRocketTimeOffTests(bool isForGeneral)
        {
            // arrange
            var timeOff = new TimeOff()
            {
                TimeOffType = TimeOffType.Vacation,
                Status = RequestStatus.Pending,
                Id = 123,
                UserId = 2,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                UserName = "TestUserName"
            };
            var timeOffDescription = TimeOffConstants.TimeOffTypesRuDescriptionDict[timeOff.TimeOffType];

            var expected = $":bell: {timeOff.UserName} {timeOffDescription} from {timeOff.StartDate:dd.MM.yyyy} to {timeOff.EndDate:dd.MM.yyyy}";


            // act
            var result = _timeOffReportMessageBuilder.GetRocketTimeOff(timeOff, isForGeneral);

            // assert 
            Assert.NotEmpty(result);
            Assert.Equal(expected, result);
        }
    }
}
