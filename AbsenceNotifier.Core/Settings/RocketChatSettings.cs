using AbsenceNotifier.Core.Constants;

namespace AbsenceNotifier.Core.Settings
{
    public static class RocketChatSettings
    {
        public static string RocketChatSenderReportEmail
        {
            get => Environment.GetEnvironmentVariable(EnvironmentVariablesNames.RocketChatSenderReportEmail) 
                ?? throw new ArgumentException($"{EnvironmentVariablesNames.RocketChatSenderReportEmail} was not added to application");
        }

        public static string RocketChatSenderReportPassword
        {
            get => Environment.GetEnvironmentVariable(EnvironmentVariablesNames.RocketChatSenderReportPassword) 
                ?? throw new ArgumentException($"{EnvironmentVariablesNames.RocketChatSenderReportPassword} was not added to application");
        }

    }
}
