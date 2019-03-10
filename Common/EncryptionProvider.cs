using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Common
{
    public static class EncryptionProvider
    {
        public static byte[] Encrypt(string plainBytes, string key)
        {
            using (var aesAlg = new AesManaged())
            {
                using (var hashProvider = SHA256.Create())
                {
                    var aesKey = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(key));
                    aesAlg.Key = aesKey;
                }
                using (var hashProvider = MD5.Create())
                {
                    var aesIV = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(key));
                    aesAlg.IV = aesIV;
                }
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainBytes);
                        }
                        return msEncrypt.ToArray();
                        
                    }
                }
            }
        }

        public static string Decrypt(byte[] cipherText, string key)
        {
            using (var aesAlg = new AesManaged())
            {
                using (var hashProvider = SHA256.Create())
                {
                    var aesKey = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(key));
                    aesAlg.Key = aesKey;
                }
                using (var hashProvider = MD5.Create())
                {
                    var aesIV = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(key));
                    aesAlg.IV = aesIV;
                }
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }

            }
        }
    }
}
