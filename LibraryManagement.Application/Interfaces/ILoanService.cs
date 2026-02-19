using LibraryManagement.Application.DTOs;

namespace LibraryManagement.Application.Interfaces
{
    public interface ILoanService
    {
        Task<IEnumerable<LoanDto>> GetAllLoansAsync();
        Task<IEnumerable<LoanDto>> GetAllActiveLoansAsync();
        Task<IEnumerable<LoanDto>> GetLoansByUserIdAsync(int userId);
        Task<LoanDto?> GetLoanByIdAsync(int id);

        /// <summary>Cria um empréstimo aplicando as regras de perfil do usuário.</summary>
        Task<LoanDto> CreateLoanAsync(CreateLoanDto createLoanDto);

        /// <summary>Registra a devolução do livro e o marca como disponível.</summary>
        Task<LoanDto> ReturnBookAsync(int loanId);

        /// <summary>Soft delete — desativa o empréstimo e marca o livro como disponível.</summary>
        Task DeleteLoanAsync(int id);
    }
}