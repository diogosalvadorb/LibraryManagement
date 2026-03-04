using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Application.Mapping;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using LibraryManagement.Domain.Interfaces;

namespace LibraryManagement.Application.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;

        public LoanService(
            ILoanRepository loanRepository,
            IUserRepository userRepository,
            IBookRepository bookRepository)
        {
            _loanRepository = loanRepository;
            _userRepository = userRepository;
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<LoanDto>> GetAllLoansAsync()
        {
            var loans = await _loanRepository.GetAllAsync();
            return loans.Select(l => l.ToDto());
        }

        public async Task<IEnumerable<LoanDto>> GetLoansByUserIdAsync(int userId)
        {
            var loans = await _loanRepository.GetActiveLoansByUserIdAsync(userId);
            return loans.Select(l => l.ToDto());
        }

        public async Task<LoanDto?> GetLoanByIdAsync(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            return loan?.ToDto();
        }

        public async Task<LoanDto> CreateLoanAsync(CreateLoanDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(dto.UserId)
                ?? throw new KeyNotFoundException($"User with id {dto.UserId} was not found.");

            var book = await _bookRepository.GetByIdAsync(dto.BookId)
                ?? throw new KeyNotFoundException($"Book with id {dto.BookId} was not found.");

            if (!book.Active)
                throw new InvalidOperationException("The requested book is inactive and cannot be borrowed.");

            if (!book.IsAvailable)
                throw new InvalidOperationException("The requested book is currently unavailable.");

            var userRole = Enum.Parse<UserRole>(user.UserRole.ToString());

            var activeLoans = await _loanRepository.GetActiveLoansByUserIdAsync(dto.UserId);
            var activeLoanCount = activeLoans.Count(l => l.LoanStatus == LoanStatus.Borrowed);

            int maxLoans = Loan.GetMaxLoans(userRole);
            if (activeLoanCount >= maxLoans)
                throw new InvalidOperationException(
                    $"User with role '{userRole}' can have at most {maxLoans} active loan(s). " +
                    $"Current active loans: {activeLoanCount}.");

            var loan = Loan.Create(dto.UserId, dto.BookId, userRole);
            book.MarkAsUnavailable();

            await _loanRepository.CreateAsync(loan);
            await _bookRepository.UpdateAsync(book);

            var created = await _loanRepository.GetByIdAsync(loan.Id)
                ?? throw new InvalidOperationException("Loan could not be retrieved after creation.");

            return created.ToDto();
        }   

        public async Task<LoanDto> ReturnBookAsync(int loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId)
                ?? throw new KeyNotFoundException($"Loan with id {loanId} was not found.");

            if (!loan.Active)
                throw new InvalidOperationException("This loan is already inactive.");

            if (loan.LoanStatus == LoanStatus.Returned)
                throw new InvalidOperationException("This book has already been returned.");

            var book = await _bookRepository.GetByIdAsync(loan.BookId)
                ?? throw new InvalidOperationException($"Book with id {loan.BookId} was not found.");

            loan.Return();
            book.MarkAsAvailable();

            await _loanRepository.UpdateAsync(loan);
            await _bookRepository.UpdateAsync(book);

            var updated = await _loanRepository.GetByIdAsync(loanId);
            return updated!.ToDto();
        }

        public async Task DeleteLoanAsync(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Loan with id {id} was not found.");

            if (!loan.Active)
                throw new InvalidOperationException("Loan is already inactive.");
 
            if (loan.LoanStatus == LoanStatus.Borrowed)
            {
                var book = await _bookRepository.GetByIdAsync(loan.BookId);
                if (book is not null)
                {
                    book.MarkAsAvailable();
                    await _bookRepository.UpdateAsync(book);
                }
            }

            loan.Deactivate();
            await _loanRepository.UpdateAsync(loan);
        }
    }
}