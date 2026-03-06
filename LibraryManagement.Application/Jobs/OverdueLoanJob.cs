using LibraryManagement.Domain.Enums;
using LibraryManagement.Domain.Interfaces;

namespace LibraryManagement.Application.Jobs
{
    public class OverdueLoanJob
    {
        private readonly ILoanRepository _loanRepository;

        public OverdueLoanJob(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task CheckAndMarkOverdueLoansAsync()
        {
            var loans = await _loanRepository.GetAllAsync();

            Console.WriteLine($"Checking for overdue loans at {DateTime.UtcNow}...");

            var overdueLoans = loans.Where(l =>
                l.Active &&
                l.LoanStatus == LoanStatus.Borrowed &&
                l.ExpectedReturnDate < DateTime.UtcNow)
                .ToList();

            foreach (var loan in overdueLoans)
            {
                loan.MarkAsOverdue();
                await _loanRepository.UpdateAsync(loan);
            }
        }
    }
}