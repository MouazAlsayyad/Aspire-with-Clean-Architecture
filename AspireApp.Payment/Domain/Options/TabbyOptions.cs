namespace AspireApp.Payment.Domain.Options;

/// <summary>
/// Configuration options for Tabby payment provider
/// </summary>
public class TabbyOptions
{
    public const string SectionName = "Tabby";
    
    /// <summary>
    /// Tabby public API token
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Tabby secret API token
    /// </summary>
    public string SecretToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Tabby API base address
    /// </summary>
    public string BaseAddress { get; set; } = "https://api.tabby.ai/api/v2/";
    
    /// <summary>
    /// Merchant code
    /// </summary>
    public string MerchantCode { get; set; } = string.Empty;
}

