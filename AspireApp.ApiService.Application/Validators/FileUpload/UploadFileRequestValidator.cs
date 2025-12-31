using AspireApp.ApiService.Application.DTOs.FileUpload;
using AspireApp.ApiService.Domain.Enums;
using FluentValidation;

namespace AspireApp.ApiService.Application.Validators.FileUpload;

public class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
{
    public UploadFileRequestValidator()
    {
        RuleFor(x => x.StorageType)
            .IsInEnum()
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

