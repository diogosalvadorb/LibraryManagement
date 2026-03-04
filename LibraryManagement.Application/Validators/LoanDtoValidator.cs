using FluentValidation;
using LibraryManagement.Application.DTOs;

namespace LibraryManagement.Application.Validators
{
    public class CreateLoanDtoValidator : AbstractValidator<CreateLoanDto>
    {
        public CreateLoanDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.BookId)
                .GreaterThan(0).WithMessage("BookId must be greater than 0.");
        }
    }

    public class ReturnLoanDtoValidator : AbstractValidator<ReturnLoanDto>
    {
        public ReturnLoanDtoValidator()
        {
            RuleFor(x => x.LoanId)
                .GreaterThan(0).WithMessage("LoanId must be greater than 0.");
        }
    }
}