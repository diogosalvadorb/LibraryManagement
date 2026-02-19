using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using LibraryManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Persistence.Repository
{
    public class LoanRepository : ILoanRepository
    {
        private readonly DataBaseContext _context;

        public LoanRepository(DataBaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Loan>> GetAllAsync()
        {
            return await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Book)
                .ToListAsync();
        }

        public async Task<IEnumerable<Loan>> GetAllActiveAsync()
        {
            return await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Book)
                .Where(l => l.Active)
                .ToListAsync();
        }

        public async Task<Loan?> GetByIdAsync(int id)
        {
            return await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Book)
                .SingleOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Loan>> GetActiveLoansByUserIdAsync(int userId)
        {
            return await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Book)
                .Where(l => l.UserId == userId
                         && l.Active
                         && l.LoanStatus == LoanStatus.Borrowed)
                .ToListAsync();
        }

        public async Task<bool> HasActiveLoanForBookAsync(int bookId)
        {
            return await _context.Loans
                .AnyAsync(l => l.BookId == bookId
                            && l.Active
                            && l.LoanStatus == LoanStatus.Borrowed);
        }

        public async Task CreateAsync(Loan loan)
        {
            await _context.Loans.AddAsync(loan);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Loan loan)
        {
            _context.Loans.Update(loan);
            await _context.SaveChangesAsync();
        }
    }
}