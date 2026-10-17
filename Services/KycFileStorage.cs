using Microsoft.AspNetCore.Http;

namespace EverestRemit.Api.Services;

public sealed class KycFileStorage
{
    private readonly string _root;

    public KycFileStorage(IWebHostEnvironment environment)
    {
        _root = Path.Combine(
            environment.ContentRootPath,
            "PrivateKycFiles");

        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(
        IFormFile file,
        bool allowPdf,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Please select a file.");

        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("Maximum file size is 5 MB.");

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        var allowedExtensions = allowPdf
            ? new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" }
            : new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Unsupported file type.");

        // Do not trust ContentType or the extension alone.
        // Production code must also inspect file signatures and
        // validate/decode images or parse PDFs before storing them.
        var key = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_root, key);

        await using var stream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);

        await file.CopyToAsync(stream, ct);

        // Store only the generated key in the database.
        return key;
    }

    public string GetPath(string key)
    {
        // Never accept a client-supplied path.
        if (string.IsNullOrWhiteSpace(key) ||
            key != Path.GetFileName(key))
        {
            throw new ArgumentException("Invalid file reference.");
        }

        return Path.Combine(_root, key);
    }
}