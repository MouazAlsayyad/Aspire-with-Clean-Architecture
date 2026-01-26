using AspireApp.Modules.FileUpload.Application.DTOs;
using AspireApp.Modules.FileUpload.Application.Validators;
using AspireApp.Modules.FileUpload.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace AspireApp.Modules.FileUpload.Tests.Application.Validators;

public class UploadFileFormDtoValidatorTests
{
    private readonly UploadFileFormDtoValidator _validator;

    public UploadFileFormDtoValidatorTests()
    {
        _validator = new UploadFileFormDtoValidator();
    }

    [Fact]
    public void Should_Have_Error_When_File_Is_Null()
    {
        var model = new UploadFileFormDto { File = null! };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void Should_Have_Error_When_File_Is_Empty()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);

        var model = new UploadFileFormDto { File = mockFile.Object };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void Should_Not_Have_Error_When_File_Is_Valid()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(100);

        var model = new UploadFileFormDto { File = mockFile.Object };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void Should_Have_Error_When_StorageType_Is_Invalid()
    {
        var model = new UploadFileFormDto { StorageType = (FileStorageType)999 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.StorageType);
    }

    [Fact]
    public void Should_Not_Have_Error_When_StorageType_Is_Valid()
    {
        var model = new UploadFileFormDto { StorageType = FileStorageType.Database };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.StorageType);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Too_Long()
    {
        var model = new UploadFileFormDto { Description = new string('a', 1001) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Tags_Is_Too_Long()
    {
        var model = new UploadFileFormDto { Tags = new string('a', 501) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Tags);
    }
}
