using FluentValidation;
using SuperCom.Core.DTOs;

namespace SuperCom.Core.Validators;

public class UpdateTaskItemValidator : AbstractValidator<UpdateTaskItemDto>
{
    public UpdateTaskItemValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority must be a valid value (Low, Medium, High, Critical).");

        RuleFor(x => x.UserFullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name must not exceed 150 characters.");

        RuleFor(x => x.UserTelephone)
            .NotEmpty().WithMessage("Telephone is required.")
            .Matches(@"^[\+]?[0-9\-\s\(\)]{7,20}$").WithMessage("Telephone must be a valid phone number.");

        RuleFor(x => x.UserEmail)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters.");

        RuleFor(x => x.TagIds)
            .NotNull().WithMessage("Tags list must not be null.")
            .Must(tags => tags != null && tags.Count <= 20).WithMessage("A task can have at most 20 tags.")
            .ForEach(id => id.GreaterThan(0).WithMessage("Each tag ID must be a positive number."));
    }
}
