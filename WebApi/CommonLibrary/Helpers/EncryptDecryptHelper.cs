using System.Security.Cryptography;
using System.Text;
using CommonLibrary.Helpers.Interfaces;

namespace CommonLibrary.Helpers
{
    public class EncryptDecryptHelper : IEncryptDecryptHelper
    {
        private static readonly Lazy<EncryptDecryptHelper> lazy = new Lazy<EncryptDecryptHelper>(() => new EncryptDecryptHelper());

        public static EncryptDecryptHelper Instance { get { return lazy.Value; } }

        private EncryptDecryptHelper() { }

        private byte[] DeriveKey(string password, int keySizeInBits)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                Array.Resize(ref keyBytes, keySizeInBits / 8);
                return keyBytes;
            }
        }

        public string AESEncrypt(string input, string keyStr, int keySizeInBits = 256)
        {
            byte[] key = DeriveKey(keyStr, keySizeInBits);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    // 首先寫入 IV
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(inputBytes, 0, inputBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    // 返回 Base64 編碼的字串（包含 IV 和加密數據）
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string AESDecrypt(string encryptedData, string keyStr, int keySizeInBits = 256)
        {
            byte[] key = DeriveKey(keyStr, keySizeInBits);
            byte[] fullCipherText = Convert.FromBase64String(encryptedData);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // 讀取 IV（前 16 字節）
                byte[] iv = new byte[16];
                Array.Copy(fullCipherText, 0, iv, 0, iv.Length);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        // 解密剩餘的數據
                        cs.Write(fullCipherText, iv.Length, fullCipherText.Length - iv.Length);
                        cs.FlushFinalBlock();
                    }

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        public enum HashType
        {
            MD5,
            SHA1,
            SHA256
        }

        public byte[] CreateHash(string input, HashType hashType)
        {
            byte[] result = [];
            switch (hashType)
            {
                case HashType.MD5:
                    result = CreateMD5Hash(input);
                    break;
                case HashType.SHA1:
                    result = CreateSHA1Hash(input);
                    break;
                case HashType.SHA256:
                    result = CreateSHA256Hash(input);
                    break;
            }
            return result;
        }

        private byte[] CreateSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.Default.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return hashBytes;
            }
        }

        private byte[] CreateSHA1Hash(string input)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] inputBytes = Encoding.Default.GetBytes(input);
                byte[] hashBytes = sha1.ComputeHash(inputBytes);
                return hashBytes;
            }
        }

        private byte[] CreateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.Default.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return hashBytes;
            }
        }
    }
}

