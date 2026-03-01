namespace Services.Shared.FeatureFlag.Models;

/// <summary>
/// Feature Flag è©•ä¼°ä¸Šä???
/// </summary>
public class FeatureFlagContext
{
    /// <summary>
    /// ?¨æˆ¶ ID
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// åº—å®¶ ID
    /// </summary>
    public string? StoreId { get; set; }
    
    /// <summary>
    /// ?°å??ç¨± (Development, Staging, Production)
    /// </summary>
    public string? Environment { get; set; }
    
    /// <summary>
    /// ?ªè?å±¬æ€§ï??ªä??´å??¨ï?
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
    
    /// <summary>
    /// å»ºç?ç©ºç?ä¸Šä???
    /// </summary>
    public static FeatureFlagContext Empty => new();
}

