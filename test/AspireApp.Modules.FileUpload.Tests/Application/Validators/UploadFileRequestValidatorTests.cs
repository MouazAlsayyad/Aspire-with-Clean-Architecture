using AspireApp.Modules.FileUpload.Application.DTOs;
using AspireApp.Modules.FileUpload.Application.Validators;
using AspireApp.Modules.FileUpload.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace AspireApp.Modules.FileUpload.Tests.Application.Validators;

public class UploadFileRequestValidatorTests
{
    private readonly UploadFileRequestValidator _validator;

    public UploadFileRequestValidatorTests()
    {
        _validator = new UploadFileRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_StorageType_Is_Invalid()
    {
        var model = new UploadFileRequest { StorageType = (FileStorageType)999 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.StorageType);
    }

    [Fact]
    public void Should_Not_Have_Error_When_StorageType_Is_Valid()
    {
        var model = new UploadFileRequest { StorageType = FileStorageType.FileSystem };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.StorageType);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_MaxLength()
    {
        var model = new UploadFileRequest { Description = new string('a', 1001) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Description_Is_Valid()
    {
        var model = new UploadFileRequest { Description = "Valid description" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Tags_Exceeds_MaxLength()
    {
        var model = new UploadFileRequest { Tags = new string('a', 501) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Tags_Is_Valid()
    {
        var model = new UploadFileRequest { Tags = "tag1,tag2" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }
}
