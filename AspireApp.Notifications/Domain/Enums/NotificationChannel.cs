namespace AspireApp.Notifications.Domain.Enums;

/// <summary>
/// Notification delivery channels
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Send via all available channels
    /// </summary>
    All = 0,
    
    /// <summary>
    /// Send via Twilio SMS
    /// </summary>
    TwilioSms = 1,
    
    /// <summary>
    /// Send via Twilio WhatsApp
    /// </summary>
    TwilioWhatsApp = 2,
    
    /// <summary>
    /// Send via Email
    /// </summary>
    Email = 3,
    
    /// <summary>
    /// Send via Firebase Cloud Messaging
    /// </summary>
    Firebase = 4
}

