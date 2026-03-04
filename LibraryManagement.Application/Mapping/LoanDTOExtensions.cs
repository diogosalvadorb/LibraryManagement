using LibraryManagement.Application.DTOs;
using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Application.Mapping
{
    public static class LoanDTOExtensions
    {
        public static LoanDto ToDto(this Loan loan)
        {
            return new LoanDto(
                loan.Id,
                loan.UserId,
                loan.User?.Name ?? string.Empty,
                loan.BookId,
                loan.Book?.Title ?? string.Empty,
                loan.LoanDate,
                loan.ExpectedReturnDate,
                loan.ReturnDate,
                loan.LoanStatus.ToString(),
                loan.Active);
        }
    }
}