using System.Security.Cryptography;
using System.Text;
using CommonLibrary.Helpers.Interfaces;

namespace CommonLibrary.Helpers
{
    public class KeyGeneratorHelper : IKeyGeneratorHelper
    {
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NumberChars = "0123456789";
        private const string SpecialChars = "!@#$%^&*()_-+=<>?";
        private static Lazy<KeyGeneratorHelper> _instance = new Lazy<KeyGeneratorHelper>(() => new KeyGeneratorHelper());

        public static KeyGeneratorHelper Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public string GenerateRandString(int length, bool isPassword, bool isSpecialSymbolsInclude)
        {
            if (isPassword && (length < 8 || length > 12))
            {
                throw new ArgumentException("Password length must be at 8 to 12 characters.");
            }

            var allChars = LowercaseChars + UppercaseChars + NumberChars;
            var result = new StringBuilder(length);
            var random = new Random();

            // 確保至少包含一個小寫字母、大寫字母、數字和特殊字符
            result.Append(LowercaseChars[random.Next(LowercaseChars.Length)]);
            result.Append(UppercaseChars[random.Next(UppercaseChars.Length)]);
            result.Append(NumberChars[random.Next(NumberChars.Length)]);
            if (isSpecialSymbolsInclude)
            {
                allChars += SpecialChars;
                result.Append(SpecialChars[random.Next(SpecialChars.Length)]);
            }

            // 填充剩餘的字符
            for (int i = isSpecialSymbolsInclude ? 4 : 3; i <= length; i++)
            {
                result.Append(allChars[random.Next(allChars.Length)]);
            }

            // 打亂密碼順序
            return new string(result.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray());
        }

        // 更安全的版本，使用 RNGCryptoServiceProvider
        public string GenerateSecurePassword(int length)
        {
            if (length < 8)
            {
                throw new ArgumentException("Password length must be at least 8 characters.");
            }

            var allChars = LowercaseChars + UppercaseChars + NumberChars + SpecialChars;
            var result = new StringBuilder(length);

            using (var rng = new RNGCryptoServiceProvider())
            {
                var allCharArray = allChars.ToCharArray();
                var byteArray = new byte[4];

                // 確保至少包含一個小寫字母、大寫字母、數字和特殊字符
                result.Append(GetRandomChar(LowercaseChars, rng));
                result.Append(GetRandomChar(UppercaseChars, rng));
                result.Append(GetRandomChar(NumberChars, rng));
                result.Append(GetRandomChar(SpecialChars, rng));

                // 填充剩餘的字符
                for (int i = 4; i < length; i++)
                {
                    result.Append(GetRandomChar(allChars, rng));
                }

                // 打亂密碼順序
                return new string(result.ToString().ToCharArray().OrderBy(x => GetRandomInt(rng)).ToArray());
            }
        }

        public string GenerateRandomCapitalLetters()
        {
            Random random = new Random();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 4; i++)
            {
                char randomChar = (char)random.Next('A', 'Z' + 1);
                sb.Append(randomChar);
            }

            return sb.ToString();
        }

        private char GetRandomChar(string chars, RNGCryptoServiceProvider rng)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var index = BitConverter.ToUInt32(bytes, 0) % chars.Length;
            return chars[(int)index];
        }

        private int GetRandomInt(RNGCryptoServiceProvider rng)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}

