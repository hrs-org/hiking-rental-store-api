using FluentValidation;
using HRS.API.Contracts.DTOs.Item;

namespace HRS.API.Validators.Item.Helpers;

public static class ItemRequestValidatorHelper
{
    public static void AddCommonRules<T>(AbstractValidator<T> validator) where T : ItemRequestDto
    {
        validator.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required");

        validator.RuleFor(x => x.Quantity)
            .NotNull().WithMessage("Item Quantity is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Quantity cannot be negative");

        validator.RuleFor(x => x.Price)
            .NotNull().WithMessage("Item Price is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Price cannot be negative");
    }
}
