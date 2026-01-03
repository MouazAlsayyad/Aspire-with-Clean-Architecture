namespace AspireApp.Email.Domain.Options;

/// <summary>
/// Email configuration options
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";
    
    public string Provider { get; set; } = "SMTP";
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public bool EnableBcc { get; set; }
    public string ApplicationTitle { get; set; } = "AspireApp";
}

