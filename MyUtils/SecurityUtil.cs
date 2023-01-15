using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MyUtils
{
    public static class SecurityUtil
    {
        /// <summary>
        /// Encrypt <paramref name="plaintext"/> to Base64 string
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="cryptoKey"></param>
        /// <returns></returns>
        public static string AesEncryptBase64(this string plaintext, string cryptoKey)
        {
            using var aes = Aes.Create();
            using var md5 = MD5.Create();
            using var sha256 = SHA256.Create();

            byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(cryptoKey));
            byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(cryptoKey));
            aes.Key = key;
            aes.IV = iv;

            byte[] dataByteArray = Encoding.UTF8.GetBytes(plaintext);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);

            cs.Write(dataByteArray, 0, dataByteArray.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Decrypt <paramref name="ciphertext"/> to plaintext
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="cryptoKey"></param>
        /// <returns></returns>
        public static string AesDecryptBase64(string ciphertext, string cryptoKey)
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(cryptoKey));
            byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(cryptoKey));
            aes.Key = key;
            aes.IV = iv;

            byte[] dataByteArray = Convert.FromBase64String(ciphertext);
            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(dataByteArray, 0, dataByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static string HashSHA1(this string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public static string MD5Hash(this string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }

        /// <summary>
        /// Designed to solve <see cref="Object.GetHashCode"/> issue that .net core version may return different values. 
        /// See https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetDeterministicHashCode(this string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

    }
}
