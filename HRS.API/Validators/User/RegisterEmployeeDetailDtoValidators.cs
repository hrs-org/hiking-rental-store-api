using FluentValidation;
using HRS.API.Contracts.DTOs.User;

namespace HRS.API.Validators.User;

public class RegisterEmployeeDetailDtoValidators : AbstractValidator<RegisterEmployeeDetailDto>
{
    public RegisterEmployeeDetailDtoValidators()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty();
    }
}
