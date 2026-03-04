namespace LibraryManagement.Application.DTOs
{
    public sealed record LoanDto(
        int Id,
        int UserId,
        string UserName,
        int BookId,
        string BookTitle,
        DateTime LoanDate,
        DateTime ExpectedReturnDate,
        DateTime? ReturnDate,
        string LoanStatus,
        bool Active);

    public sealed record CreateLoanDto(int UserId, int BookId);
    public sealed record ReturnLoanDto(int LoanId);
}