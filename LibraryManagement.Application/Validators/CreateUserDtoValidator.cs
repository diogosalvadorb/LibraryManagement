using FluentValidation;
using LibraryManagement.Application.DTOs;
using LibraryManagement.Domain.Enums;
using System.Text.RegularExpressions;

namespace LibraryManagement.Application.Validators
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(320).WithMessage("Email must not exceed 100 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MaximumLength(255).WithMessage("Password must not exceed 255 characters.")
                .Must(ValidPassword).WithMessage("The password must contain at least 8 characters: one number, one hidden letter, one lowercase letter, and one special character.");

            RuleFor(x => x.Role)
                .Must(role => Enum.IsDefined(typeof(UserRole), role))
                .WithMessage("The provided User Role is invalid.");
        }

        public bool ValidPassword(string password)
        {
            var regex = new Regex(@"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$");

            return regex.IsMatch(password);
        }
    }
}
