namespace Services.Shared.FeatureFlag.Models;

/// <summary>
/// Feature Flag ?ç½®æ¨¡å?
/// </summary>
public class FeatureFlagConfiguration
{
    /// <summary>
    /// Provider é¡å?
    /// </summary>
    public string Provider { get; set; } = "Configuration";
    
    /// <summary>
    /// ?Ÿèƒ½å®šç¾©å­—å…¸
    /// </summary>
    public Dictionary<string, FeatureFlagDefinition> Features { get; set; } = new();
}

/// <summary>
/// ?®ä??Ÿèƒ½?„å?ç¾?
/// </summary>
public class FeatureFlagDefinition
{
    /// <summary>
    /// ?¯å¦?Ÿç”¨
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// ?Ÿèƒ½?è¿°
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// ?Ÿç”¨?„ç”¨??ID ?—è¡¨
    /// </summary>
    public List<string>? EnabledForUsers { get; set; }
    
    /// <summary>
    /// ?Ÿç”¨?„å?å®?ID ?—è¡¨
    /// </summary>
    public List<string>? EnabledForStores { get; set; }
    
    /// <summary>
    /// ?Ÿç”¨?„ç’°å¢ƒå?è¡?
    /// </summary>
    public List<string>? EnabledEnvironments { get; set; }
    
    /// <summary>
    /// è®Šé??¼å??¸ï??¨æ–¼ A/B testingï¼?
    /// </summary>
    public Dictionary<string, object>? Variants { get; set; }
}

