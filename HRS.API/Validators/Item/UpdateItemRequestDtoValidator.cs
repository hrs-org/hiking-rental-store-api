using FluentValidation;
using HRS.API.Contracts.DTOs.Item;

namespace HRS.API.Validators.Item;

public class UpdateItemRequestDtoValidator : AbstractValidator<UpdateItemRequestDto>
{
    public UpdateItemRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Item Id is required");

        AddCommonRules(this);

        RuleForEach(x => x.Children)
            .SetValidator(new ItemChildRequestDtoValidator());
    }


    private sealed class ItemChildRequestDtoValidator : AbstractValidator<UpdateItemChildDto>
    {
        public ItemChildRequestDtoValidator()
        {
            AddCommonRules(this);
        }
    }

    private static void AddCommonRules<T>(AbstractValidator<T> validator) where T : class
    {
        validator.RuleFor(x => (string)x.GetType().GetProperty("Name")!.GetValue(x)!)
            .NotEmpty().WithMessage("Item name is required");

        validator.RuleFor(x => (string)x.GetType().GetProperty("Description")!.GetValue(x)!)
            .NotEmpty().WithMessage("Item Description is required");

        validator.RuleFor(x => (int?)x.GetType().GetProperty("Quantity")!.GetValue(x) ?? 0)
            .NotNull().WithMessage("Item Quantity is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Quantity cannot be negative");

        validator.RuleFor(x => (int?)x.GetType().GetProperty("Price")!.GetValue(x) ?? 0)
            .NotNull().WithMessage("Item Price is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Price cannot be negative");
    }
}
