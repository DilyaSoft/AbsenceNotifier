using AbsenceNotifier.Core.Constants;

namespace AbsenceNotifier.Core.Settings
{
    public static class ApplicationSettings
    {
        public static string CurrentMessengerName
        {
            get => Environment.GetEnvironmentVariable(EnvironmentVariablesNames.CurrentMessengerName)
                ?? throw new ArgumentException($"{EnvironmentVariablesNames.CurrentMessengerName} was not added to application");
        }

    }
}
