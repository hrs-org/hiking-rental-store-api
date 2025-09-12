using FluentValidation;
using HRS.API.Contracts.DTOs.Auth;

namespace HRS.API.Validators.Auth;

public class RefreshTokenRequestDtoValidators : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidators()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
