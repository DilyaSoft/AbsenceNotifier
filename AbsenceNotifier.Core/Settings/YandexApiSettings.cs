using AbsenceNotifier.Core.Constants;

namespace AbsenceNotifier.Core.Settings
{
    public static class YandexApiSettings
    {
        public static string AuthHeader()
        {
            var prefix = "OAuth ";
            var envHeader = Environment.GetEnvironmentVariable(EnvironmentVariablesNames.YandexAuthHeader);
            if (string.IsNullOrWhiteSpace(envHeader))
            {
                throw new ArgumentException($"{EnvironmentVariablesNames.YandexAuthHeader} was not added to application");
            }
            return prefix + envHeader;
        }

        /// <summary>
        /// If you doesn't use web api to decline\approve timeoff this GateWayUrl should be requried
        /// </summary>
        public static string GateWayUrl
        {
            get => Environment.GetEnvironmentVariable(EnvironmentVariablesNames.YandexGatewayUrl)
                   ?? throw new ArgumentException($"{EnvironmentVariablesNames.YandexGatewayUrl} was not added to application");
        }
    }
}
