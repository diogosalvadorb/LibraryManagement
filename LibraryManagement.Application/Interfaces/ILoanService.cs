using LibraryManagement.Application.DTOs;

namespace LibraryManagement.Application.Interfaces
{
    public interface ILoanService
    {
        Task<IEnumerable<LoanDto>> GetAllLoansAsync();
        Task<IEnumerable<LoanDto>> GetLoansByUserIdAsync(int userId);
        Task<LoanDto?> GetLoanByIdAsync(int id);
        Task<LoanDto> CreateLoanAsync(CreateLoanDto createLoanDto);
        Task<LoanDto> ReturnBookAsync(int loanId);    
        Task DeleteLoanAsync(int id);
    }
}