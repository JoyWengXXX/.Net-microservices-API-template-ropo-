using System.ComponentModel;

namespace Service.Common.Models
{
    public class UserInfoEnums
    {
        /// <summary>
        /// ä½¿ç”¨?…ç???        /// </summary>
        public enum UserStatus
        {
            /// <summary>
            /// ?œç”¨
            /// </summary>
            Disabled = 0,

            /// <summary>
            /// ?Ÿç”¨
            /// </summary>
            Active = 1
        }

        /// <summary>
        /// ä½¿ç”¨?…ç™»?¥é???        /// </summary>
        public enum SSOType
        {
            /// <summary>
            /// ä¸€?¬ç™»??            /// </summary>
            RegularLogin = 0,

            /// <summary>
            /// Google?»å…¥
            /// </summary>
            Google = 1,

            /// <summary>
            /// iOS?»å…¥
            /// </summary>
            iOS = 2,

            /// <summary>
            /// ç®¡ç??…å??°ç™»??            /// </summary>
            Admin = 3,
        }
    }

    public class UserRoleEnums
    {
        /// <summary>
        /// ç³»çµ±?³è‰²
        /// </summary>
        public enum Role
        {
            /// <summary>
            /// ç³»çµ±ç®¡ç???            /// </summary>
            Admin = 0,

            /// <summary>
            /// ä¸€ç´šä½¿?¨è€?            /// </summary>
            FirstRankUser = 1,

            /// <summary>
            /// äºŒç?ä½¿ç”¨??            /// </summary>
            SecondRankUser = 2,

            /// <summary>
            /// ä¸‰ç?ä½¿ç”¨??            /// </summary>
            ThirdRankUser = 3,
        }
    }

    public class ReturnResultCodeEnums
    {
        public enum SystemResultCode
        {
            #region ?šç”¨?¯èª¤ä»?¢¼
            /// <summary>
            /// JWT TOKENå¤±æ?
            /// </summary>
            TokenFail = 1,
            /// <summary>
            /// è«‹é??°ç™»??            /// </summary>
            PleaseLoginAgain = 2,
            /// <summary>
            /// ?¯èª¤?„è¼¸?¥æ ¼å¼?            /// </summary>
            InvalidInputForm = 3,
            /// <summary>
            /// ?æ??ä?
            /// </summary>
            InvalidOperation = 4,
            /// <summary>
            /// ?ä?æµç??¼ç??¯èª¤
            /// </summary>
            OperationError = 5,
            /// <summary>
            /// ä¼ºæ??¨éŒ¯èª?            /// </summary>
            ServerInternalError = 10,
            #endregion

            #region è¨»å?/?»å…¥?¸é??¯èª¤ä»?¢¼
            /// <summary>
            /// ?¥ç„¡æ­¤å¸³??            /// </summary>
            A01 = 101,
            /// <summary>
            /// å¸³è?å¯†ç¢¼?¯èª¤
            /// </summary>
            A02 = 102,
            /// <summary>
            /// ä¿¡ç®±?è?è¨»å?
            /// </summary>
            A03 = 103,
            /// <summary>
            /// è¨»å?é©—è?ç¢¼éŒ¯èª?            /// </summary>
            A04 = 104,
            /// <summary>
            /// å¯†ç¢¼ä¸å?è¦?            /// </summary>
            A05 = 105,
            /// <summary>
            /// ä½¿ç”¨?…å??ªé€šé?ä¿¡ç®±é©—è?
            /// </summary>
            A06 = 106,
            /// <summary>
            /// ä½¿ç”¨?…å·²è¢«å???            /// </summary>
            A07 = 107,
            /// <summary>
            /// SSO?»å…¥?¡æ?è®Šæ›´å¯†ç¢¼
            /// </summary>
            A08 = 108,
            #endregion
        }
    }
}

