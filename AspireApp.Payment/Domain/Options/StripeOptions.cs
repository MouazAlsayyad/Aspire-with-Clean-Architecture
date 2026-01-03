namespace AspireApp.Payment.Domain.Options;

/// <summary>
/// Configuration options for Stripe payment provider
/// </summary>
public class StripeOptions
{
    public const string SectionName = "Stripe";
    
    /// <summary>
    /// Stripe secret API key
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Stripe publishable API key
    /// </summary>
    public string PublishableKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Webhook signing secret for validating Stripe webhooks
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;
}

