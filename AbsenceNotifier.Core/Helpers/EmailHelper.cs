using Serilog;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace AbsenceNotifier.Core.Helpers
{
    public static class EmailHelper
    {
        private static readonly Regex _emailRegex
          = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Log.Logger.Error("Email was null");
                return false;
            }

            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith(".", StringComparison.InvariantCulture))
            {
                Log.Logger.Error("Email should not contain . in the end");
                return false;
            }
            if (!_emailRegex.IsMatch(trimmedEmail))
            {
                Log.Logger.Error("Email was not valid");
                return false;
            }
            var splitBySeparatorEmail = trimmedEmail.Split('@');
            if (splitBySeparatorEmail.Length != 2)
            {
                Log.Logger.Error("Email should be separated by one @");
                return false;
            }
            if (splitBySeparatorEmail[0].Count(x => x == '.') > 1)
            {
                Log.Logger.Error("Email part before @, should contain only 1 dot character");
                return false;
            }
            // to do: static class to log error
            try
            {
                var address = new MailAddress(email);
                return address.Address == trimmedEmail;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return false;
            }
        }
    }
}
