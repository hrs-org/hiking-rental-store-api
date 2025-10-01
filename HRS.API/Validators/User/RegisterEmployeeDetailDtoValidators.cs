using FluentValidation;
using HRS.API.Contracts.DTOs.User;
using HRS.Domain.Interfaces;
namespace HRS.API.Validators.User;


public class RegisterEmployeeDetailDtoValidators : AbstractValidator<RegisterEmployeeDetailDto>
{
    public RegisterEmployeeDetailDtoValidators(IUserRepository userRepository)
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, cancellation) =>
            {
                return await userRepository.IsEmailUniqueAsync(email);
            })
            .WithMessage("Email is already in use.");
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => r == "Employee" || r == "Admin" || r == "Manager")
            .WithMessage("Role must be Employee or Admin or Manager");



    }
}
