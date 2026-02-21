using FluentValidation;
using SuperCom.Core.DTOs;

namespace SuperCom.Core.Validators;

public class UpdateTagValidator : AbstractValidator<UpdateTagDto>
{
    public UpdateTagValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required.")
            .MaximumLength(50).WithMessage("Tag name must not exceed 50 characters.");
    }
}
