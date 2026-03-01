namespace Services.Shared.FeatureFlag.Attributes
{
    /// <summary>
    /// æ¨™è??¹æ?ä½¿ç”¨ Feature Flag ?²è?è·¯ç”±
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FeatureFlagAttribute : Attribute
    {
        /// <summary>
        /// Feature Flag ?ç¨±
        /// </summary>
        public string FlagName { get; }

        /// <summary>
        /// ??Flag ?Ÿç”¨?‚èª¿?¨ç??¹æ??ç¨±ï¼ˆunified å¯¦ä?ï¼?
        /// </summary>
        public string UnifiedMethodName { get; }

        /// <summary>
        /// ??Flag ?œç”¨?‚èª¿?¨ç??¹æ??ç¨±ï¼ˆlegacy å¯¦ä?ï¼?
        /// </summary>
        public string LegacyMethodName { get; }

        public FeatureFlagAttribute(string flagName, string unifiedMethodName, string legacyMethodName)
        {
            FlagName = flagName;
            UnifiedMethodName = unifiedMethodName;
            LegacyMethodName = legacyMethodName;
        }
    }
}

