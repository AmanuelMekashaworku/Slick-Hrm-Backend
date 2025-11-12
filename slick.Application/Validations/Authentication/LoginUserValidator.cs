using slick.Application.DTOs.Identity;
using FluentValidation;

namespace slick.Application.Validations.Authentication
{
    public class LoginUserValidator : AbstractValidator<LoginUser>
    {
        public LoginUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is Required")
                .EmailAddress().WithMessage("Invalid email Format");
            RuleFor(x => x.Password)
               .NotEmpty().WithMessage("Password is Required");

        }

    }
}
