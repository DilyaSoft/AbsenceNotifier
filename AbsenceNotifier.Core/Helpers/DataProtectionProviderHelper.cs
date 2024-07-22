using AbsenceNotifier.Core.Configurations;
using AbsenceNotifier.Core.DTOs.Results;
using AbsenceNotifier.Core.Interfaces;
using Microsoft.Extensions.Options;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace AbsenceNotifier.Core.Helpers
{
    public class DataProtectionProviderHelper : IDataProtectionProviderHelper
    {
        private readonly ApplicationCommonConfiguration _applicationCommon;
        public DataProtectionProviderHelper(
            IOptions<ApplicationCommonConfiguration> options)
        {
            _applicationCommon = options.Value;
        }

        public DataProtectionResult GetDecryptedValue(string value)
        {
            var result = new DataProtectionResult();
            var decryptedValue = "";
            try
            {
                value = value.Replace(" ", "+");
                var cipherBytes = Convert.FromBase64String(value);
                using (var encryption = Aes.Create())
                {
                    var pdb = new Rfc2898DeriveBytes(_applicationCommon.DataProtectorPurpose,
                        _applicationCommon.QueryParamsSalt,
                        _applicationCommon.AlgorithmIterations, 
                        Enum.Parse<HashAlgorithmName>(_applicationCommon.HashAlgorithmName));

                    encryption.Key = pdb.GetBytes(_applicationCommon.SymmetricKey);
                    encryption.IV = pdb.GetBytes(_applicationCommon.AlogirthmVector);
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryption.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        decryptedValue = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                result.Success = true;
                result.Value = decryptedValue;
            }
            catch (CryptographicException ex)
            {
                Log.Logger.Error(ex.Message);
                result.Success = false;
                result.Value = decryptedValue;
                result.Message = ex.Message;
                result.Message += " " + ex.InnerException?.Message;
                return result;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                result.Success = false;
                result.Value = decryptedValue;
                result.Message = ex.Message;
                result.Message += " " + ex.InnerException?.Message;
                return result;
            }
            return result;
        }

        public DataProtectionResult GetEncryptedValue(string value)
        {
            var result = new DataProtectionResult();
            var encryptedValue = "";
            try
            {
                var clearBytes = Encoding.Unicode.GetBytes(value);
                using (var encryption = Aes.Create())
                {
                    var pdb = new Rfc2898DeriveBytes(
                        _applicationCommon.DataProtectorPurpose,
                        _applicationCommon.QueryParamsSalt,
                        _applicationCommon.AlgorithmIterations,
                       Enum.Parse<HashAlgorithmName>(_applicationCommon.HashAlgorithmName));
                    encryption.Key = pdb.GetBytes(_applicationCommon.SymmetricKey);
                    encryption.IV = pdb.GetBytes(_applicationCommon.AlogirthmVector);
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryption.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        encryptedValue = Convert.ToBase64String(ms.ToArray());
                    }
                }
                result.Value = encryptedValue;
            }
            catch (CryptographicException ex)
            {
                Log.Logger.Error(ex.Message);
                result.Value = encryptedValue;
                result.Message = ex.Message;
                return result;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                result.Value = encryptedValue;
                result.Message = ex.Message;
                return result;
            }
            result.Success = true;
            return result;
        }
    }
}
