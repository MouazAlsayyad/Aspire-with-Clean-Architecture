namespace AspireApp.FirebaseNotifications.Domain.Enums;

/// <summary>
/// Time filter options for retrieving notifications
/// </summary>
public enum NotificationTimeFilter
{
    All = 0,         // All notifications
    Today = 1,       // Today's notifications
    Yesterday = 2,   // Yesterday's notifications
    Earlier = 3      // Earlier notifications
}

