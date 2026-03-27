using FluentValidation;
using HRS.API.Contracts.DTOs.User;

namespace HRS.API.Validators.User;

public class AssignCustomerRoleDtoValidator : AbstractValidator<AssignCustomerRoleDto>
{
    public AssignCustomerRoleDtoValidator()
    {
        RuleFor(x => x.Auth0UserId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}
