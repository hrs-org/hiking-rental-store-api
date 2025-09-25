using FluentValidation;
using HRS.API.Contracts.DTOs.Item;

namespace HRS.API.Validators.Item;

public class AddItemRequestDtoValidator : AbstractValidator<AddItemRequestDto>
{
    public AddItemRequestDtoValidator()
    {
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
            .SetValidator(new ItemChildRequestDtoValidator());
    }

    private sealed class ItemChildRequestDtoValidator : AbstractValidator<AddItemRequestDto>
    {
        public ItemChildRequestDtoValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Child name is required");

            RuleFor(c => c.Description)
                .NotEmpty().WithMessage("Child description is required");

            RuleFor(c => c.Quantity)
                .NotNull().WithMessage("Child quantity is required")
                .GreaterThanOrEqualTo(0).WithMessage("Child quantity cannot be negative");

            RuleFor(c => c.Price)
                .NotNull().WithMessage("Child price is required")
                .GreaterThanOrEqualTo(0).WithMessage("Child price cannot be negative");
        }
    }
}
