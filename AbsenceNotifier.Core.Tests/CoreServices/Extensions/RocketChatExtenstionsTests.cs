using AbsenceNotifier.Core.DTOs.Results.RocketChat;
using AbsenceNotifier.Core.DTOs.Rocket;
using AbsenceNotifier.Core.DTOs.Users;
using AbsenceNotifier.Core.Helpers;

namespace AbsenceNotifier.Core.Tests.CoreServices.Extensions
{
    public class RocketChatExtenstionsTests
    {

        [Theory]
        [InlineData(true, "error", "test", "test", true)]
        [InlineData(false, "success", "test", "test", false)]
        [InlineData(false, "error", "test", "test", true)]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
        [InlineData(false, "error", null, "test", true)]
        [InlineData(false, "error", "test", null, true)]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
        public void LoginToChatNotFailedTest(bool success, string statusMessage,
            string authToken, string userId, bool loginResult)
        {
            // arrange
            var login = new LoginToRocketResult()
            {
                Status = statusMessage,
                Success = !success,
                Data = new RocketChatLoginResult()
                {
                    AuthToken = authToken,
                    UserId = userId
                },
            };

            // act 
            var result = login.LoginToRocketChatFailed();

            // assert
            Assert.Equal(loginResult, result);
        }

        [Fact]
        public void CreateRoomIsErrorTest()
        {
            // arrange
            var login = new CreateRoomResult()
            {
                Success = false,
                Status = "error",
                Room = null
            };

            // act 
            var result = login.IsRocketChatCreateRoomWithError();

            // assert
            Assert.True(result);
        }

        [Fact]
        public void CreateRoomIdNullTest()
        {
            // arrange
            var login = new CreateRoomResult()
            {
                Success = true,
                Status = "success",
                Room = new Room()
                {
                    Id = null
                }
            };

            // act 
            var result = login.IsRocketChatCreateRoomWithError();

            // assert
            Assert.True(result);
        }

        [Fact]
        public void GetRocketChatUserByUserNameTimeTasticTest()
        {
            // arrange
            var expected = new RocketChatUser()
            {
                Name = "Test testov"
            };
            var users = new List<RocketChatUser>()
            {
                new RocketChatUser()
                {
                    UserName = "User Usernamov",
                    Emails = new[]
                    {
                        new RocketChatUserEmail()
                        {
                            Address = "Test@mail.com"
                        }
                    }
                },
                expected,
                new RocketChatUser()
                {
                    UserName = "User2 Usernamov2",
                    Emails = new[]
                    {
                        new RocketChatUserEmail()
                        {
                            Address = "Test@mail.com"
                        }
                    }
                },
            };
            var timeTasticUser = new TimetasticUser()
            {
                FirstName = "Test",
                Surname = "testov",
                Email = "Test222@mail.com"
            };

            // act
            var user = users.GetRocketChatUserByTimeTastic(timeTasticUser);

            // assert
            Assert.NotNull(user);
            Assert.Equal(expected.UserName, user.UserName);
        }

        [Fact]
        public void GetRocketChatUserByEmailTimetasticTest()
        {
            // arrange
            var expected = new RocketChatUser()
            {
                UserName = "Test testov",
                Emails = new[]
                {
                    new RocketChatUserEmail()
                    {
                        Address = "Test222@mail.com"
                    }
                }
            };
            var users = new List<RocketChatUser>()
            {
                new RocketChatUser()
                {
                    UserName = "User Usernamov",
                    Emails = new[]
                    {
                        new RocketChatUserEmail()
                        {
                            Address = "Test@mail.com"
                        }
                    }
                },
                expected,
                new RocketChatUser()
                {
                    UserName = "User2 Usernamov2",
                    Emails = new[]
                    {
                        new RocketChatUserEmail()
                        {
                            Address = "Test123@mail.com"
                        }
                    }
                },
            };
            var timeTasticUser = new TimetasticUser()
            {
                FirstName = "Test5123",
                Surname = "testov2323",
                Email = "Test222@mail.com"
            };

            // act
            var user = users.GetRocketChatUserByTimeTastic(timeTasticUser);

            // assert
            Assert.NotNull(user);
            Assert.NotNull(user.Emails);
            Assert.Contains(user.Emails, x => expected.Emails.Any(y => y.Address == x.Address));
        }

        [Fact]
        public void LoginToChatFailedTest()
        {
            // arrange
            LoginToRocketResult? loginNull = null;
            var statusNull = new LoginToRocketResult()
            {
                Status = null
            };
            var statusError = new LoginToRocketResult()
            {
                Status = "error"
            };
            var dataNull = new LoginToRocketResult()
            {
                Data = null
            };
            var notSuccess = new LoginToRocketResult()
            {
                Success = false
            };
            var emptyToken = new LoginToRocketResult()
            {
                Data = new RocketChatLoginResult()
                {
                    AuthToken = null,
                    UserId = "123"
                },
                Success = true,
                Status = "success"
            };
            var emptyUserId = new LoginToRocketResult()
            {
                Data = new RocketChatLoginResult()
                {
                    AuthToken = "qwerty",
                    UserId = null
                },
                Success = true,
                Status = "success"
            };

            // act
            var loginNullResult = loginNull.LoginToRocketChatFailed();
            var statusNullResult = statusNull.LoginToRocketChatFailed();
            var errorStatusResult = statusError.LoginToRocketChatFailed();
            var errorDataNullResult = dataNull.LoginToRocketChatFailed();
            var statusNotSuccessResult = notSuccess.LoginToRocketChatFailed();
            var emptyTokenResult = emptyToken.LoginToRocketChatFailed();
            var emptyUserIdResult = emptyUserId.LoginToRocketChatFailed();

            // assert
            Assert.True(loginNullResult);
            Assert.True(statusNullResult);
            Assert.True(errorStatusResult);
            Assert.True(errorDataNullResult);
            Assert.True(statusNotSuccessResult);
            Assert.True(emptyTokenResult);
            Assert.True(emptyUserIdResult);
        }

        [Fact]
        public void CreateRoomErrorTest()
        {
            // arrange
            CreateRoomResult? resultNull = null;
            var notSuccess = new CreateRoomResult()
            {
                Success = false,
            };
            var roomNull = new CreateRoomResult()
            {
                Room = null,
                Success = true,
            };
            var roomIdNull = new CreateRoomResult()
            {
                Room = new Room()
                {
                    Id = null
                },
                Success = true,
            };

            // act
            var actualResultNull = resultNull.IsRocketChatCreateRoomWithError();
            var actualNotSuccess = notSuccess.IsRocketChatCreateRoomWithError();
            var actualRoomNull = roomNull.IsRocketChatCreateRoomWithError();
            var actualRoomIdNull = roomIdNull.IsRocketChatCreateRoomWithError();

            // assert
            Assert.True(actualResultNull);
            Assert.True(actualNotSuccess);
            Assert.True(actualRoomNull);
            Assert.True(actualRoomIdNull);
        }

        [Fact]
        public void GetRocketChatUserByTimeTasticTest()
        {
            // arrange
            var manager = new RocketChatUser()
            {
                Name = "Manager1 Managerov",
            };
            var emailManager = new RocketChatUser()
            {
                Name = "Manager2",
                Emails = new[]
                {
                    new RocketChatUserEmail()
                    {
                        Address = "manager2@gmail.com"
                    }
                }
            };

            var timetasticManager = new TimetasticUser()
            {
                FirstName = "Manager1",
                Surname = "Managerov"
            };

            var timetasticManagerEmail = new TimetasticUser()
            {
                FirstName = "Manager2",
                Surname = "Managerov",
                Email = "manager2@gmail.com"
            };

            var users = new List<RocketChatUser>()
            {
                manager,
                emailManager,
                new RocketChatUser()
                {
                    Name = "User1",
                },
                new RocketChatUser()
                {
                    Name = "User2"
                }
            };

            // act
            var managerResult = users.GetRocketChatUserByTimeTastic(timetasticManager);
            var managerEmailResult = users.GetRocketChatUserByTimeTastic(timetasticManagerEmail);

            // assert
            Assert.NotNull(managerResult);
            Assert.Equal(manager, managerResult);
            Assert.NotNull(managerEmailResult);
            Assert.Equal(emailManager, managerEmailResult);

        }
    }
}
