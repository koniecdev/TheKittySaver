using System.Text.RegularExpressions;

namespace TheKittySaver.AdoptionSystem.Infrastructure.FileUpload;

internal static partial class FileNameValidator
{
    private static readonly HashSet<string> DangerousExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".cmd", ".com", ".scr", ".pif", ".msi", ".msp",
        ".ps1", ".psm1", ".psd1", ".vbs", ".vbe", ".js", ".jse", ".ws", ".wsf",
        ".wsh", ".wsc", ".hta", ".cpl", ".msc", ".inf", ".reg", ".scf", ".lnk",
        ".php", ".php3", ".php4", ".php5", ".phtml", ".asp", ".aspx", ".ashx",
        ".asmx", ".cer", ".csr", ".jsp", ".jspx", ".cgi", ".pl", ".py", ".pyc",
        ".pyo", ".rb", ".sh", ".bash", ".zsh", ".ksh", ".csh", ".fish",
        ".jar", ".war", ".ear", ".class", ".swf", ".svg", ".html", ".htm",
        ".xhtml", ".xml", ".xsl", ".xslt", ".css", ".elf", ".bin", ".run",
        ".app", ".deb", ".rpm", ".dmg", ".pkg", ".apk", ".ipa"
    };

    public static FileNameValidationResult Validate(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return FileNameValidationResult.Success;
        }

        if (ContainsNullBytes(fileName))
        {
            return FileNameValidationResult.NullByteDetected;
        }

        if (ContainsPathTraversal(fileName))
        {
            return FileNameValidationResult.PathTraversalDetected;
        }

        if (ContainsDangerousCharacters(fileName))
        {
            return FileNameValidationResult.DangerousCharactersDetected;
        }

        if (HasDoubleExtension(fileName, out string? hiddenExtension))
        {
            return FileNameValidationResult.DoubleExtensionDetected(hiddenExtension!);
        }

        if (IsTooLong(fileName))
        {
            return FileNameValidationResult.FileNameTooLong;
        }

        return FileNameValidationResult.Success;
    }

    private static bool ContainsNullBytes(string fileName)
    {
        return fileName.Contains('\0', StringComparison.Ordinal);
    }

    private static bool ContainsPathTraversal(string fileName)
    {
        return fileName.Contains("..", StringComparison.Ordinal)
               || fileName.Contains('/', StringComparison.Ordinal)
               || fileName.Contains('\\', StringComparison.Ordinal)
               || fileName.StartsWith('~');
    }

    private static bool ContainsDangerousCharacters(string fileName)
    {
        return fileName.Any(c => c < 32)
               || fileName.Contains('<', StringComparison.Ordinal)
               || fileName.Contains('>', StringComparison.Ordinal)
               || fileName.Contains(':', StringComparison.Ordinal)
               || fileName.Contains('"', StringComparison.Ordinal)
               || fileName.Contains('|', StringComparison.Ordinal)
               || fileName.Contains('?', StringComparison.Ordinal)
               || fileName.Contains('*', StringComparison.Ordinal);
    }

    private static bool HasDoubleExtension(string fileName, out string? hiddenExtension)
    {
        hiddenExtension = null;

        string[] parts = fileName.Split('.');
        if (parts.Length < 3)
        {
            return false;
        }

        for (int i = 1; i < parts.Length - 1; i++)
        {
            string potentialExtension = "." + parts[i];
            if (DangerousExtensions.Contains(potentialExtension))
            {
                hiddenExtension = potentialExtension;
                return true;
            }
        }

        return false;
    }

    private static bool IsTooLong(string fileName)
    {
        return fileName.Length > 255;
    }

    [GeneratedRegex(@"^[\w\-. ]+$", RegexOptions.Compiled)]
    private static partial Regex SafeFileNamePattern();
}

internal sealed class FileNameValidationResult
{
    public bool IsValid { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public string? AdditionalInfo { get; }

    private FileNameValidationResult(bool isValid, string? errorCode = null, string? errorMessage = null, string? additionalInfo = null)
    {
        IsValid = isValid;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        AdditionalInfo = additionalInfo;
    }

    public static FileNameValidationResult Success => new(true);

    public static FileNameValidationResult NullByteDetected => new(
        false,
        "FileUpload.NullByteInFileName",
        "File name contains invalid null byte character.");

    public static FileNameValidationResult PathTraversalDetected => new(
        false,
        "FileUpload.PathTraversalDetected",
        "File name contains path traversal characters.");

    public static FileNameValidationResult DangerousCharactersDetected => new(
        false,
        "FileUpload.DangerousCharacters",
        "File name contains potentially dangerous characters.");

    public static FileNameValidationResult DoubleExtensionDetected(string hiddenExtension) => new(
        false,
        "FileUpload.DoubleExtension",
        $"File appears to have a hidden executable extension '{hiddenExtension}'.",
        hiddenExtension);

    public static FileNameValidationResult FileNameTooLong => new(
        false,
        "FileUpload.FileNameTooLong",
        "File name exceeds maximum allowed length of 255 characters.");
}
