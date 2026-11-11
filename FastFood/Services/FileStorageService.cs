namespace FastFood.Services;

public sealed class FileStorageService
{
    private const long MaxFileSize = 5 * 1024 * 1024;
    private readonly string _webRoot;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
    }

    public async Task<string> SaveFoodImageAsync(IFormFile file)
        => await SaveImageAsync(file, "fooditems");

    public async Task<string> SaveProfileImageAsync(IFormFile file)
        => await SaveImageAsync(file, "profiles");

    private async Task<string> SaveImageAsync(IFormFile file, string category)
    {
        if (file.Length == 0 || file.Length > MaxFileSize)
            throw new InvalidDataException("Файлът трябва да е до 5 MB и да не е празен.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
            throw new InvalidDataException("Позволени са само JPG, JPEG, PNG и WEBP файлове.");

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var folder = Path.Combine(_webRoot, "uploads", category);
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, fileName);

        await using (var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
        {
            await file.CopyToAsync(output);
        }

        return $"/uploads/{category}/{fileName}";
    }

    public Task DeleteFoodImageAsync(string? relativePath) => DeleteImageAsync(relativePath);

    public Task DeleteProfileImageAsync(string? relativePath) => DeleteImageAsync(relativePath);

    private Task DeleteImageAsync(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.CompletedTask;

        var fileName = Path.GetFileName(relativePath);
        var category = relativePath.StartsWith("/uploads/profiles/", StringComparison.Ordinal)
            ? "profiles"
            : "fooditems";
        var fullPath = Path.Combine(_webRoot, "uploads", category, fileName);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
