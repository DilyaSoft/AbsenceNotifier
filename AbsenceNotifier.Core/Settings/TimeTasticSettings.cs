using AbsenceNotifier.Core.Constants;

namespace AbsenceNotifier.Core.Settings
{
    public static class TimeTasticSettings
    {
        public static string AuthHeader
        {
            get => Environment.GetEnvironmentVariable(EnvironmentVariablesNames.TimeTasticAuthHeader) 
                ?? throw new ArgumentException($"{EnvironmentVariablesNames.TimeTasticAuthHeader} was not added to application");
        }
    }
}
