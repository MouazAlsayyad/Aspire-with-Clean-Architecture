namespace AspireApp.Modules.FileUpload.Domain.Enums;

/// <summary>
/// Represents the type/category of file
/// </summary>
public enum FileType
{
    /// <summary>
    /// Image files (jpg, png, gif, webp, etc.)
    /// </summary>
    Image = 1,

    /// <summary>
    /// Document files (pdf, doc, docx, txt, etc.)
    /// </summary>
    Document = 2,

    /// <summary>
    /// Video files (mp4, avi, mov, etc.)
    /// </summary>
    Video = 3,

    /// <summary>
    /// Audio files (mp3, wav, ogg, etc.)
    /// </summary>
    Audio = 4,

    /// <summary>
    /// Other/unknown file types
    /// </summary>
    Other = 99
}

