using System.Security.Cryptography;

namespace AbsenceNotifier.Core.Configurations
{
    public class ApplicationCommonConfiguration
    {
        public const string EmptyGeneralManagerMessege = "General manager email was not provided";
        public string? DataProtectorPurpose 
        { 
            get
            {
                return Environment.GetEnvironmentVariable("Absence-data-protection-key") ?? 
                    throw new ArgumentException("data protection query params was not added");
            }
        }

        public byte[] QueryParamsSalt
        {
            get
            {
                var saltStr = Environment.GetEnvironmentVariable("absence-notifier-salt-query-params") 
                    ?? throw new ArgumentException("notifier query params salt was not added");

                var saltArray = saltStr.Split(",").Select(x => byte.Parse(x)).ToArray();
                return saltArray;
            }
        }

        public string? RequestActionsApiUrl { get; set; }
        public string? GeneralManagerEmail { get; set; }
        public int AlgorithmIterations { get; set; }

        private string _hashName;
        public string HashAlgorithmName { 
            get
            {
                if (Enum.TryParse<HashAlgorithmName>(_hashName, out var hash) )
                {
                    return _hashName;
                }
                throw new ArgumentException($"Hash algorithm with name cannot be parsed - '{_hashName}'");
            }
            set 
            {
                _hashName = value;
            } 
        }

        public int SymmetricKey
        {
            get
            {
                var envKey = Environment.GetEnvironmentVariable("symmetric-query-params-key");

                if (envKey == null)
                {
                    throw new ArgumentException("Symmetric key for hash algorithm was not added");
                }

                return int.Parse(envKey);
            }
        }

        public int AlogirthmVector
        {
            get
            {
                var envKey = Environment.GetEnvironmentVariable("algoithm-vector");
                if (envKey == null)
                {
                    throw new ArgumentException("vector was not added");
                }
                return int.Parse(envKey);
            }
        }
    }
}
