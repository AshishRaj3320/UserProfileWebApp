using System.ComponentModel.DataAnnotations;

namespace UserProfileWebApp.Validators
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public FileExtensionAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Check if the value is a file
            if (value is IFormFile file)
            {
                // Get the file extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // Check if the extension is in the allowed list
                if (Array.Exists(_extensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase)))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(ErrorMessage ?? $"The file must have one of the following extensions: {string.Join(", ", _extensions)}");
                }
            }

            // If there's no file, return success (or handle it according to your requirements)
            return ValidationResult.Success;
        }
    }
}
