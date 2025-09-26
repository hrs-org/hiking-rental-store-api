using FluentValidation;
using HRS.API.Contracts.DTOs.Item;
using HRS.API.Validators.Item.Helpers;

namespace HRS.API.Validators.Item;

public class AddItemRequestDtoValidator : AbstractValidator<AddItemRequestDto>
{
    public AddItemRequestDtoValidator()
    {
        ItemRequestValidatorHelper.AddCommonRules(this);

        RuleForEach(x => x.Children)
            .SetValidator(new ItemChildRequestDtoValidator());
    }

    private sealed class ItemChildRequestDtoValidator : AbstractValidator<AddItemRequestDto>
    {
        public ItemChildRequestDtoValidator()
        {
            ItemRequestValidatorHelper.AddCommonRules(this);
        }
    }
}
