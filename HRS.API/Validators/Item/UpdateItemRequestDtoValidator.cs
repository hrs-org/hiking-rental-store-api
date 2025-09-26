using FluentValidation;
using HRS.API.Contracts.DTOs.Item;
using HRS.API.Validators.Item.Helpers;

namespace HRS.API.Validators.Item;

public class UpdateItemRequestDtoValidator : AbstractValidator<UpdateItemRequestDto>
{
    public UpdateItemRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotNull().WithMessage("Item Id is required")
            .GreaterThan(0).WithMessage("Item Id must be greater than 0");

        ItemRequestValidatorHelper.AddCommonRules(this);

        RuleForEach(x => x.Children)
            .SetValidator(new UpdateItemChildRequestDtoValidator());
    }

    private sealed class UpdateItemChildRequestDtoValidator : AbstractValidator<UpdateItemRequestDto>
    {
        public UpdateItemChildRequestDtoValidator()
        {
            ItemRequestValidatorHelper.AddCommonRules(this);
        }
    }
}
