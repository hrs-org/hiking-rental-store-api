using FluentValidation;
using HRS.API.Contracts.DTOs.Item;

namespace HRS.API.Validators.Item;

public class AddItemRequestDtoValidator : AbstractValidator<AddItemRequestDto>
{
    public AddItemRequestDtoValidator()
    {
        AddCommonRules(this);

        RuleForEach(x => x.Children)
            .SetValidator(new ItemChildRequestDtoValidator());
    }

    private sealed class ItemChildRequestDtoValidator : AbstractValidator<AddItemRequestDto>
    {
        public ItemChildRequestDtoValidator()
        {
            AddCommonRules(this);
        }
    }

    private static void AddCommonRules(AbstractValidator<AddItemRequestDto> validator)
    {
        validator.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required");

        validator.RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Item Description is required");

        validator.RuleFor(x => x.Quantity)
            .NotNull().WithMessage("Item Quantity is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Quantity cannot be negative");

        validator.RuleFor(x => x.Price)
            .NotNull().WithMessage("Item Price is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Price cannot be negative");
    }
}
