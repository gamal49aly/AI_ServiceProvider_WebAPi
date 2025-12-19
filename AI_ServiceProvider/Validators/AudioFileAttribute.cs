namespace AI_ServiceProvider.Validators
{
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;

    public class AudioFileAttribute : ValidationAttribute
    {
        private readonly int _maxFileSizeInMB;

        private readonly string[] _allowedExtensions =
        {
        ".wav", ".mp3", ".m4a", ".ogg", ".webm"
    };

        private readonly string[] _allowedMimeTypes =
        {
        "audio/wav",
        "audio/mpeg",
        "audio/mp3",
        "audio/mp4",
        "audio/ogg",
        "audio/webm"
    };

        public AudioFileAttribute(int maxFileSizeInMB = 10)
        {
            _maxFileSizeInMB = maxFileSizeInMB;
            ErrorMessage = $"Only audio files (.wav, .mp3, .m4a, .ogg, .webm) up to {_maxFileSizeInMB}MB are allowed.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not IFormFile file)
                return ValidationResult.Success; // [Required] handles null

            // 1️⃣ File size check
            if (file.Length == 0 || file.Length > _maxFileSizeInMB * 1024 * 1024)
                return new ValidationResult(ErrorMessage);

            // 2️⃣ Extension check
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return new ValidationResult("Invalid audio file extension.");

            // 3️⃣ MIME type check
            if (!_allowedMimeTypes.Contains(file.ContentType))
                return new ValidationResult("Invalid audio MIME type.");

            return ValidationResult.Success;
        }
    }

}
