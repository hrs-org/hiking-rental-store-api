using FluentValidation;
using HRS.API.Contracts.DTOs.Store;

namespace HRS.API.Validators.Store;

public class StoreOnboardingRequestDtoValidator : AbstractValidator<StoreOnboardingRequestDto>
{
    public StoreOnboardingRequestDtoValidator()
    {
        RuleFor(x => x.Auth0UserId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches("^\\+?[0-9\\- ]{7,15}$")
            .WithMessage("Please enter a valid phone number");
    }
}
