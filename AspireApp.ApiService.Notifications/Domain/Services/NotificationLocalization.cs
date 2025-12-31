using System.Text.Json;
using AspireApp.ApiService.Notifications.Domain.Interfaces;

namespace AspireApp.ApiService.Notifications.Domain.Services;

/// <summary>
/// Static helper class for notification localization.
/// Provides easy access to localized strings and predefined notification templates.
/// </summary>
public static class NotificationLocalization
{
    private static Dictionary<string, Dictionary<string, string>> _resources = new();
    private static readonly object _lockObject = new();
    private static bool _initialized = false;

    /// <summary>
    /// Initializes the localization system by loading resource files
    /// </summary>
    public static void Initialize()
    {
        if (_initialized)
            return;

        lock (_lockObject)
        {
            if (_initialized)
                return;

            LoadResources();
            _initialized = true;
        }
    }

    /// <summary>
    /// Gets localized notification content with parameters
    /// </summary>
    public static LocalizedNotificationContent GetContent(
        string language,
        string titleKey,
        string bodyKey,
        string? actionUrl = null,
        params object[] args)
    {
        if (!_initialized)
            Initialize();

        var title = GetString(titleKey, language, args);
        var body = GetString(bodyKey, language, args);

        return new LocalizedNotificationContent
        {
            Title = title,
            Body = body,
            Language = language,
            ActionUrl = actionUrl
        };
    }

    /// <summary>
    /// Gets a single localized string
    /// </summary>
    public static string GetString(string key, string language = "en", params object[] args)
    {
        if (!_initialized)
            Initialize();

        // Fallback to English if language not found
        if (!_resources.ContainsKey(language))
            language = "en";

        // Fallback to English if key not found in requested language
        if (!_resources.ContainsKey(language) || !_resources[language].ContainsKey(key))
        {
            if (_resources.ContainsKey("en") && _resources["en"].ContainsKey(key))
            {
                language = "en";
            }
            else
            {
                return key; // Return key if translation not found
            }
        }

        var value = _resources[language][key];

        // Apply string formatting if args provided
        if (args != null && args.Length > 0)
        {
            try
            {
                return string.Format(value, args);
            }
            catch
            {
                return value; // Return unformatted string if formatting fails
            }
        }

        return value;
    }

    /// <summary>
    /// Gets available languages
    /// </summary>
    public static List<string> GetAvailableLanguages()
    {
        if (!_initialized)
            Initialize();

        return _resources.Keys.ToList();
    }

    /// <summary>
    /// Reloads resources (useful for development)
    /// </summary>
    public static void ReloadResources()
    {
        lock (_lockObject)
        {
            _initialized = false;
            _resources.Clear();
            Initialize();
        }
    }

    private static void LoadResources()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var possiblePaths = new[]
        {
            Path.Combine(baseDirectory, "Resources"),
            Path.Combine(baseDirectory, "Domain", "Resources"),
            Path.Combine(Directory.GetCurrentDirectory(), "Resources"),
            Path.Combine(Directory.GetCurrentDirectory(), "Domain", "Resources"),
            Path.Combine(baseDirectory, "Notifications", "Domain", "Resources"),
            Path.Combine(Directory.GetCurrentDirectory(), "Notifications", "Domain", "Resources")
        };

        string? resourcesPath = null;
        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path))
            {
                resourcesPath = path;
                break;
            }
        }

        if (resourcesPath == null)
        {
            // Create Resources directory if it doesn't exist
            resourcesPath = Path.Combine(baseDirectory, "Resources");
            Directory.CreateDirectory(resourcesPath);
        }

        // Load English resources
        var englishPath = Path.Combine(resourcesPath, "NotificationResources.json");
        if (File.Exists(englishPath))
        {
            var englishJson = File.ReadAllText(englishPath);
            var englishDict = JsonSerializer.Deserialize<Dictionary<string, string>>(englishJson);
            if (englishDict != null)
            {
                _resources["en"] = englishDict;
            }
        }

        // Load Arabic resources
        var arabicPath = Path.Combine(resourcesPath, "NotificationResources.ar.json");
        if (File.Exists(arabicPath))
        {
            var arabicJson = File.ReadAllText(arabicPath);
            var arabicDict = JsonSerializer.Deserialize<Dictionary<string, string>>(arabicJson);
            if (arabicDict != null)
            {
                _resources["ar"] = arabicDict;
            }
        }
    }

    /// <summary>
    /// Predefined templates for common notification scenarios
    /// </summary>
    public static class FacilityRequest
    {
        public static LocalizedNotificationContent NewRequest(string language, string userName, string facilityCode, string? actionUrl = null)
        {
            return GetContent(language, "FacilityRequest_New_Title", "FacilityRequest_New_Body", actionUrl, userName, facilityCode);
        }

        public static LocalizedNotificationContent Approved(string language, string facilityName, string facilityCode, string? actionUrl = null)
        {
            return GetContent(language, "FacilityRequest_Approved_Title", "FacilityRequest_Approved_Body", actionUrl, facilityName, facilityCode);
        }

        public static LocalizedNotificationContent Rejected(string language, string facilityName, string facilityCode, string? actionUrl = null)
        {
            return GetContent(language, "FacilityRequest_Rejected_Title", "FacilityRequest_Rejected_Body", actionUrl, facilityName, facilityCode);
        }
    }

    public static class TestReport
    {
        public static LocalizedNotificationContent NewReport(string language, string patientName, string? actionUrl = null)
        {
            return GetContent(language, "TestReport_New_Title", "TestReport_New_Body", actionUrl, patientName);
        }

        public static LocalizedNotificationContent Completed(string language, string patientName, string? actionUrl = null)
        {
            return GetContent(language, "TestReport_Completed_Title", "TestReport_Completed_Body", actionUrl, patientName);
        }
    }

    public static class User
    {
        public static LocalizedNotificationContent Registration(string language, string userName, string? actionUrl = null)
        {
            return GetContent(language, "User_Registration_Title", "User_Registration_Body", actionUrl, userName);
        }

        public static LocalizedNotificationContent PasswordReset(string language, string? actionUrl = null)
        {
            return GetContent(language, "User_PasswordReset_Title", "User_PasswordReset_Body", actionUrl);
        }
    }

    public static class System
    {
        public static LocalizedNotificationContent Maintenance(string language, string startTime, string? actionUrl = null)
        {
            return GetContent(language, "System_Maintenance_Title", "System_Maintenance_Body", actionUrl, startTime);
        }

        public static LocalizedNotificationContent EmergencyAlert(string language, string location, string? actionUrl = null)
        {
            return GetContent(language, "System_EmergencyAlert_Title", "System_EmergencyAlert_Body", actionUrl, location);
        }
    }

    public static class Appointment
    {
        public static LocalizedNotificationContent Reminder(string language, string appointmentTime, string? actionUrl = null)
        {
            return GetContent(language, "Appointment_Reminder_Title", "Appointment_Reminder_Body", actionUrl, appointmentTime);
        }
    }

    public static class Payment
    {
        public static LocalizedNotificationContent Received(string language, string amount, string? actionUrl = null)
        {
            return GetContent(language, "Payment_Received_Title", "Payment_Received_Body", actionUrl, amount);
        }
    }

    public static class Document
    {
        public static LocalizedNotificationContent Expired(string language, string documentName, string? actionUrl = null)
        {
            return GetContent(language, "Document_Expired_Title", "Document_Expired_Body", actionUrl, documentName);
        }
    }

    public static class ReportStack
    {
        public static LocalizedNotificationContent LowWarning(string language, int available, int total, string? actionUrl = null)
        {
            return GetContent(language, "ReportStack_LowWarning_Title", "ReportStack_LowWarning_Body", actionUrl, available, total);
        }
    }

    public static class ReportSharing
    {
        public static LocalizedNotificationContent Redeemed(string language, string doctorName, string? actionUrl = null)
        {
            return GetContent(language, "ReportSharing_Redeemed_Title", "ReportSharing_Redeemed_Body", actionUrl, doctorName);
        }
    }
}

