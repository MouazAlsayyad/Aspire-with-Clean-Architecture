using AspireApp.ApiService.Application.FileUpload.DTOs;
using FluentValidation;

namespace AspireApp.ApiService.Application.FileUpload.Validators;

/// <summary>
/// Validator for UploadFileFormDto
/// </summary>
public class UploadFileFormDtoValidator : AbstractValidator<UploadFileFormDto>
{
    public UploadFileFormDtoValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required")
            .Must(file => file != null && file.Length > 0)
            .WithMessage("File cannot be empty");

        RuleFor(x => x.StorageType)
            .IsInEnum()
            .When(x => x.StorageType.HasValue)
            .WithMessage("Storage type must be a valid value (FileSystem, Database, or R2)");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Tags)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Tags))
            .WithMessage("Tags must not exceed 500 characters");
    }
}

