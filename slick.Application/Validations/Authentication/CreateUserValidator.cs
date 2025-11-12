using slick.Application.DTOs.Identity;
using FluentValidation;

namespace slick.Application.Validations.Authentication
{
    public class CreateUserValidator : AbstractValidator<CreateUser>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First Name is Requried");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last Name is Requried");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is Requried")
                .EmailAddress().WithMessage("Invalid email format");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is Required")
                .MinimumLength(6).WithMessage("Password must be at list 6 characters long.")
                .Matches(@"[A-Z]").WithMessage("Password must contain  at list one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain  at list one lower letter")
                .Matches(@"\d").WithMessage("Password must contain  at list one number")
                .Matches(@"[^\W]").WithMessage("Password must contain  at list one special character.");
            RuleFor(x => x.ConfirmPassword)
                 .Equal(x => x.Password).WithMessage("Password do not match.");
        }
    }
}
