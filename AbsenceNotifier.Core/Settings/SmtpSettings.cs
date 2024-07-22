using AbsenceNotifier.Core.Constants;

namespace AbsenceNotifier.Core.Settings
{
    public static class SmtpSettings
    {
        public static string? Server => Environment.GetEnvironmentVariable(EnvironmentVariablesNames.SmtpServer) ?? string.Empty;
        public static int Port => int.Parse(Environment.GetEnvironmentVariable(EnvironmentVariablesNames.SmtpPort) ?? "0");
        public static string? Username => Environment.GetEnvironmentVariable(EnvironmentVariablesNames.SmtpUserName) ?? string.Empty;
        public static string? Password => Environment.GetEnvironmentVariable(EnvironmentVariablesNames.SmtpPassword) ?? string.Empty;
        public static string? From => Environment.GetEnvironmentVariable(EnvironmentVariablesNames.SmtpFrom) ?? string.Empty;

    }
}
