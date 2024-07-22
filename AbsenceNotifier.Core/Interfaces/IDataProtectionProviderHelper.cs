using AbsenceNotifier.Core.DTOs.Results;

namespace AbsenceNotifier.Core.Interfaces
{
    public interface IDataProtectionProviderHelper
    {
        public DataProtectionResult GetEncryptedValue(string value);
        public DataProtectionResult GetDecryptedValue(string value);
    }
}
