using FluentValidation;
using HRS.API.Contracts.DTOs.Item;

namespace HRS.API.Validators.Item;

public class UpdateItemRequestDtoValidator : AbstractValidator<UpdateItemRequestDto>
{
    public UpdateItemRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Item Id is required");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Item Description is required");

        RuleFor(x => x.Quantity)
            .NotNull().WithMessage("Item Quantity is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Quantity cannot be negative");

        RuleFor(x => x.Price)
            .NotNull().WithMessage("Item Price is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Price cannot be negative");

        RuleForEach(x => x.Children)
            .ChildRules(child =>
            {
                child.RuleFor(c => c.Name)
                    .NotEmpty().WithMessage("Child name is required");

                child.RuleFor(c => c.Description)
                    .NotEmpty().WithMessage("Child description is required");

                child.RuleFor(c => c.Quantity)
                    .NotNull().WithMessage("Child quantity is required")
                    .GreaterThanOrEqualTo(0).WithMessage("Child quantity cannot be negative");

                child.RuleFor(c => c.Price)
                    .NotNull().WithMessage("Child price is required")
                    .GreaterThanOrEqualTo(0).WithMessage("Child price cannot be negative");
            });
    }
}
