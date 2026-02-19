using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Domain.Interfaces
{
    public interface ILoanRepository
    {
        Task<IEnumerable<Loan>> GetAllAsync();
        Task<IEnumerable<Loan>> GetAllActiveAsync();
        Task<Loan?> GetByIdAsync(int id);
        Task<IEnumerable<Loan>> GetActiveLoansByUserIdAsync(int userId);
        Task<bool> HasActiveLoanForBookAsync(int bookId);
        Task CreateAsync(Loan loan);
        Task UpdateAsync(Loan loan);
    }
}