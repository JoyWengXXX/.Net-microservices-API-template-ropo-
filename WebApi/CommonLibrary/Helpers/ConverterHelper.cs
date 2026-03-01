using System.Security.Cryptography;
using System.Text;

namespace CommonLibrary.Helpers
{
    public class ConverterHelper
    {
        public static Guid StringToGuid(string input)
        {
            // 轉換輸入字串為位元組陣列
            var inputBytes = Encoding.UTF8.GetBytes(input);

            // 計算雜湊
            using (var algorithm = SHA256.Create())
            {
                var hashBytes = algorithm.ComputeHash(inputBytes);

                // 確保產生版本 3 UUID
                hashBytes[6] = (byte)((hashBytes[6] & 0x0F) | 0x30);
                hashBytes[8] = (byte)((hashBytes[8] & 0x3F) | 0x80);

                return new Guid(hashBytes.Take(16).ToArray());
            }
        }
    }
}

