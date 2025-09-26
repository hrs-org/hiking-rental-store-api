using FluentValidation;
using HRS.API.Contracts.DTOs.Item;

namespace HRS.API.Validators.Item;

public class UpdateItemRequestDtoValidator : AbstractValidator<UpdateItemRequestDto>
{
    public UpdateItemRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotNull().WithMessage("Item Id is required")
            .GreaterThan(0).WithMessage("Item Id must be greater than 0");

        AddCommonRules(this);

        RuleForEach(x => x.Children)
            .SetValidator(new UpdateItemChildRequestDtoValidator());
    }

    private static void AddCommonRules(AbstractValidator<UpdateItemRequestDto> validator)
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

    private sealed class UpdateItemChildRequestDtoValidator : AbstractValidator<UpdateItemRequestDto>
    {
        public UpdateItemChildRequestDtoValidator()
        {
            AddCommonRules(this);
        }
    }
}
