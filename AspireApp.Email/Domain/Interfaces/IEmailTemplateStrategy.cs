namespace AspireApp.Email.Domain.Interfaces;

/// <summary>
/// Base interface for email template strategies
/// </summary>
public interface IEmailTemplateStrategy
{
    /// <summary>
    /// Gets the HTML template content
    /// </summary>
    string GetTemplate();
}

