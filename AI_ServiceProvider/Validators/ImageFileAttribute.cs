
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class ImageFileAttribute : ValidationAttribute
{
    private readonly int _maxFileSizeInMB;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };

    public ImageFileAttribute(int maxFileSizeInMB = 5)
    {
        _maxFileSizeInMB = maxFileSizeInMB;
        ErrorMessage = $"Only image files (.jpg, .jpeg, .png, .webp) up to {_maxFileSizeInMB}MB are allowed.";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not IFormFile file)
            return ValidationResult.Success; // [Required] handles null

        //  Size check
        if (file.Length == 0 || file.Length > _maxFileSizeInMB * 1024 * 1024)
            return new ValidationResult(ErrorMessage);

        //  Extension check
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return new ValidationResult("Invalid image file extension.");

        // MIME type check
        if (!_allowedMimeTypes.Contains(file.ContentType))
            return new ValidationResult("Invalid image MIME type.");

        return ValidationResult.Success;
    }
}
