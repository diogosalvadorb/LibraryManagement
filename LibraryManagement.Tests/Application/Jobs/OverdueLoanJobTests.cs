using LibraryManagement.Application.Jobs;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using LibraryManagement.Domain.Interfaces;
using NSubstitute;

namespace LibraryManagement.Tests.Application.Jobs
{
    public class OverdueLoanJobTests
    {
        // ──────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────

        private static Loan MakeActiveBorrowedLoan(DateTime expectedReturnDate)
        {
            var loan = Loan.Create(1, 1, UserRole.Common);

            typeof(Loan)
                .GetProperty(nameof(Loan.ExpectedReturnDate))!
                .SetValue(loan, expectedReturnDate);
            return loan;
        }


        /// <summary>
        /// Verifica se o job não chama UpdateAsync quando não há empréstimos cadastrados.
        /// </summary>
        [Fact]
        public async Task NoLoans_DoesNotCallUpdate()
        {
            var loanRepo = Substitute.For<ILoanRepository>();
            loanRepo.GetAllAsync().Returns(new List<Loan>());

            var job = new OverdueLoanJob(loanRepo);

            await job.CheckAndMarkOverdueLoansAsync();

            await loanRepo.DidNotReceive().UpdateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o job não chama UpdateAsync quando todos os empréstimos
        /// ainda estão dentro do prazo de devolução.
        /// </summary>
        [Fact]
        public async Task AllLoansOnTime_DoesNotCallUpdate()
        {
            var loan = MakeActiveBorrowedLoan(DateTime.UtcNow.AddDays(3));

            var loanRepo = Substitute.For<ILoanRepository>();
            loanRepo.GetAllAsync().Returns(new List<Loan> { loan });

            var job = new OverdueLoanJob(loanRepo);

            await job.CheckAndMarkOverdueLoansAsync();

            await loanRepo.DidNotReceive().UpdateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o job marca como Overdue e chama UpdateAsync
        /// exatamente uma vez para um empréstimo com prazo vencido.
        /// </summary>
        [Fact]
        public async Task OneLoanOverdue_MarksAsOverdueAndCallsUpdate()
        {
            var loan = MakeActiveBorrowedLoan(DateTime.UtcNow.AddDays(-1));

            var loanRepo = Substitute.For<ILoanRepository>();
            loanRepo.GetAllAsync().Returns(new List<Loan> { loan });

            var job = new OverdueLoanJob(loanRepo);

            await job.CheckAndMarkOverdueLoansAsync();

            Assert.Equal(LoanStatus.Overdue, loan.LoanStatus);
            await loanRepo.Received(1).UpdateAsync(loan);
        }

        /// <summary>
        /// Verifica se o job processa corretamente múltiplos empréstimos atrasados,
        /// marcando todos como Overdue e chamando UpdateAsync para cada um.
        /// </summary>
        [Fact]
        public async Task MultipleLoansOverdue_MarksAllAsOverdueAndCallsUpdateForEach()
        {
            var loan1 = MakeActiveBorrowedLoan(DateTime.UtcNow.AddDays(-1));
            var loan2 = MakeActiveBorrowedLoan(DateTime.UtcNow.AddDays(-5));

            var loanRepo = Substitute.For<ILoanRepository>();
            loanRepo.GetAllAsync().Returns(new List<Loan> { loan1, loan2 });

            var job = new OverdueLoanJob(loanRepo);

            await job.CheckAndMarkOverdueLoansAsync();

            Assert.Equal(LoanStatus.Overdue, loan1.LoanStatus);
            Assert.Equal(LoanStatus.Overdue, loan2.LoanStatus);
            await loanRepo.Received(1).UpdateAsync(loan1);
            await loanRepo.Received(1).UpdateAsync(loan2);
        }

        /// <summary>
        /// Verifica se o job processa apenas os empréstimos atrasados quando há
        /// uma mistura de empréstimos dentro e fora do prazo na mesma lista.
        /// </summary>
        [Fact]
        public async Task MixedLoans_MarksOnlyOverdueOnes()
        {
            var overdуeLoan = MakeActiveBorrowedLoan(DateTime.UtcNow.AddDays(-2));
            var onTimeLoan = MakeActiveBorrowedLoan(DateTime.UtcNow.AddDays(2));

            var loanRepo = Substitute.For<ILoanRepository>();
            loanRepo.GetAllAsync().Returns(new List<Loan> { overdуeLoan, onTimeLoan });

            var job = new OverdueLoanJob(loanRepo);

            await job.CheckAndMarkOverdueLoansAsync();

            Assert.Equal(LoanStatus.Overdue, overdуeLoan.LoanStatus);
            Assert.Equal(LoanStatus.Borrowed, onTimeLoan.LoanStatus);
            await loanRepo.Received(1).UpdateAsync(overdуeLoan);
            await loanRepo.DidNotReceive().UpdateAsync(onTimeLoan);
        }

        /// <summary>
        /// Verifica se o job ignora empréstimos inativos, mesmo que estejam com prazo vencido.
        /// </summary>
        [Fact]
        public async Task InactiveLoanOverdue_IsIgnored()
        {
            var loan = MakeActiveBorrowedLoan(DateTime.UtcNow.AddDays(-1));
            loan.Deactivate();

            var loanRepo = Substitute.For<ILoanRepository>();
            loanRepo.GetAllAsync().Returns(new List<Loan> { loan });

            var job = new OverdueLoanJob(loanRepo);

            await job.CheckAndMarkOverdueLoansAsync();

            Assert.NotEqual(LoanStatus.Overdue, loan.LoanStatus);
            await loanRepo.DidNotReceive().UpdateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o job ignora empréstimos já devolvidos, mesmo que estejam com prazo vencido.
        /// </summary>
        [Fact]
        public async Task ReturnedLoanOverdue_IsIgnored()
        {
            var loan = MakeActiveBorrowedLoan(DateTime.UtcNow.AddDays(-1));
            loan.Return();

            var loanRepo = Substitute.For<ILoanRepository>();
            loanRepo.GetAllAsync().Returns(new List<Loan> { loan });

            var job = new OverdueLoanJob(loanRepo);

            await job.CheckAndMarkOverdueLoansAsync();

            Assert.Equal(LoanStatus.Returned, loan.LoanStatus);
            await loanRepo.DidNotReceive().UpdateAsync(Arg.Any<Loan>());
        }
    }
}