using CommonLibrary.Helpers.Interfaces;

namespace CommonLibrary.Helpers
{
    public class ValidateHelper : IValidateHelper
    {
        private static Lazy<ValidateHelper> _instance = new Lazy<ValidateHelper>(() => new ValidateHelper());

        public static ValidateHelper Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        /// <summary>
        /// 驗證密碼是否符合以下規則：
        /// 1. 長度介於8-12個字元
        /// 2. 必須包含英文大寫字母
        /// 3. 必須包含英文小寫字母
        /// 4. 必須包含數字
        /// </summary>
        /// <param name="password">要驗證的密碼</param>
        /// <returns>包含驗證結果與錯誤訊息的元組</returns>
        public bool ValidatePassword(string password)
        {
            // 檢查密碼是否為空
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            // 檢查長度是否在8-12之間
            if (password.Length < 8 || password.Length > 12)
            {
                return false;
            }

            // 檢查是否包含英文大寫字母
            if (!password.Any(char.IsUpper))
            {
                return false;
            }

            // 檢查是否包含英文小寫字母
            if (!password.Any(char.IsLower))
            {
                return false;
            }

            // 檢查是否包含數字
            if (!password.Any(char.IsDigit))
            {
                return false;
            }

            // 檢查是否只包含允許的字元（英文字母和數字）
            if (password.Any(c => !char.IsLetterOrDigit(c)))
            {
                return false;
            }

            return true;
        }
    }
}

