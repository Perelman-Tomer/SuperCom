using FluentValidation.TestHelper;
using SuperCom.Core.DTOs;
using SuperCom.Core.Enums;
using SuperCom.Core.Validators;

namespace SuperCom.Tests.Validators;

public class CreateTaskItemValidatorTests
{
    private readonly CreateTaskItemValidator _validator = new();

    private CreateTaskItemDto CreateValidDto() => new()
    {
        Title = "Valid Task",
        Description = "A valid description",
        DueDate = DateTime.UtcNow.AddDays(7),
        Priority = Priority.Medium,
        UserFullName = "John Doe",
        UserTelephone = "050-1234567",
        UserEmail = "john@example.com",
        TagIds = new List<int> { 1 }
    };

    [Fact]
    public async Task ValidDto_ShouldPassValidation()
    {
        var result = await _validator.TestValidateAsync(CreateValidDto());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Title_WhenEmptyOrNull_ShouldHaveError(string? title)
    {
        var dto = CreateValidDto();
        dto.Title = title!;
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Title_WhenExceeds200Chars_ShouldHaveError()
    {
        var dto = CreateValidDto();
        dto.Title = new string('a', 201);
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Description_WhenExceeds2000Chars_ShouldHaveError()
    {
        var dto = CreateValidDto();
        dto.Description = new string('a', 2001);
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Description_WhenNull_ShouldPassValidation()
    {
        var dto = CreateValidDto();
        dto.Description = null;
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task DueDate_WhenInThePast_ShouldHaveError()
    {
        var dto = CreateValidDto();
        dto.DueDate = DateTime.UtcNow.AddDays(-1);
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public async Task DueDate_WhenInTheFuture_ShouldPass()
    {
        var dto = CreateValidDto();
        dto.DueDate = DateTime.UtcNow.AddDays(30);
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public async Task Priority_WhenInvalidEnum_ShouldHaveError()
    {
        var dto = CreateValidDto();
        dto.Priority = (Priority)99;
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task UserFullName_WhenEmptyOrNull_ShouldHaveError(string? name)
    {
        var dto = CreateValidDto();
        dto.UserFullName = name!;
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserFullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task UserTelephone_WhenEmptyOrNull_ShouldHaveError(string? phone)
    {
        var dto = CreateValidDto();
        dto.UserTelephone = phone!;
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserTelephone);
    }

    [Theory]
    [InlineData("050-1234567")]
    [InlineData("+972501234567")]
    [InlineData("(050) 123-4567")]
    [InlineData("0501234567")]
    public async Task UserTelephone_ValidFormats_ShouldPass(string phone)
    {
        var dto = CreateValidDto();
        dto.UserTelephone = phone;
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.UserTelephone);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12")]
    [InlineData("not-a-phone!!")]
    public async Task UserTelephone_InvalidFormats_ShouldHaveError(string phone)
    {
        var dto = CreateValidDto();
        dto.UserTelephone = phone;
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserTelephone);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task UserEmail_WhenEmptyOrNull_ShouldHaveError(string? email)
    {
        var dto = CreateValidDto();
        dto.UserEmail = email!;
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserEmail);
    }

    [Fact]
    public async Task UserEmail_WhenInvalidFormat_ShouldHaveError()
    {
        var dto = CreateValidDto();
        dto.UserEmail = "not-an-email";
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserEmail);
    }

    [Fact]
    public async Task UserEmail_WhenExceeds254Chars_ShouldHaveError()
    {
        var dto = CreateValidDto();
        dto.UserEmail = new string('a', 246) + "@test.com"; // 256 chars, exceeds 254
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserEmail);
    }

    [Fact]
    public async Task TagIds_WhenNull_ShouldHaveError()
    {
        var dto = CreateValidDto();
        dto.TagIds = null!;
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.TagIds);
    }

    [Fact]
    public async Task TagIds_WhenEmpty_ShouldPassValidation()
    {
        var dto = CreateValidDto();
        dto.TagIds = new List<int>();
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.TagIds);
    }

    [Fact]
    public async Task TagIds_WhenContainsZeroOrNegative_ShouldHaveError()
    {
        var dto = CreateValidDto();
        dto.TagIds = new List<int> { 1, 0, -5 };
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor("TagIds[1]");
    }

    [Fact]
    public async Task TagIds_WhenExceeds20_ShouldHaveError()
    {
        var dto = CreateValidDto();
        dto.TagIds = Enumerable.Range(1, 21).ToList();
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.TagIds);
    }
}

public class CreateTagValidatorTests
{
    private readonly CreateTagValidator _validator = new();

    [Fact]
    public async Task ValidName_ShouldPassValidation()
    {
        var dto = new CreateTagDto { Name = "ValidTag" };
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Name_WhenEmptyOrNull_ShouldHaveError(string? name)
    {
        var dto = new CreateTagDto { Name = name! };
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Name_WhenExceeds50Chars_ShouldHaveError()
    {
        var dto = new CreateTagDto { Name = new string('a', 51) };
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

public class UpdateTagValidatorTests
{
    private readonly UpdateTagValidator _validator = new();

    [Fact]
    public async Task ValidName_ShouldPassValidation()
    {
        var dto = new UpdateTagDto { Name = "UpdatedTag" };
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Name_WhenEmptyOrNull_ShouldHaveError(string? name)
    {
        var dto = new UpdateTagDto { Name = name! };
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Name_WhenExceeds50Chars_ShouldHaveError()
    {
        var dto = new UpdateTagDto { Name = new string('a', 51) };
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
