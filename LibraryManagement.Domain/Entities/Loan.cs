using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Domain.Entities
{
    public class Loan
    {
        public Loan(int userId, int bookId, DateTime expectedReturnDate)
        {
            UserId = userId;
            BookId = bookId;
            LoanDate = DateTime.UtcNow;
            ExpectedReturnDate = expectedReturnDate;
            LoanStatus = LoanStatus.Borrowed;
            Active = true;
        }

        public int Id { get; private set; }
        public int UserId { get; private set; }
        public User? User { get; private set; }
        public int BookId { get; private set; }
        public Book? Book { get; private set; }
        public DateTime LoanDate { get; private set; }
        public DateTime ExpectedReturnDate { get; private set; }
        public DateTime? ReturnDate { get; private set; }
        public LoanStatus LoanStatus { get; private set; }
        public bool Active { get; private set; }

        public void Return()
        {
            ReturnDate = DateTime.UtcNow;
            LoanStatus = LoanStatus.Returned;
        }

        public void MarkAsOverdue()
        {
            LoanStatus = LoanStatus.Overdue;
        }

        public void Deactivate()
        {
            Active = false;
        }

        private record LoanRule(int MaxLoans, int ReturnDays);

        private static readonly Dictionary<UserRole, LoanRule> _rules = new()
        {
            { UserRole.Common,  new LoanRule(MaxLoans: 1, ReturnDays: 4) },
            { UserRole.Premium, new LoanRule(MaxLoans: 1, ReturnDays: 8) },
            { UserRole.Vip,     new LoanRule(MaxLoans: 3, ReturnDays: 8) },
            { UserRole.Admin,   new LoanRule(MaxLoans: 3, ReturnDays: 8) },
        };

        public static int GetMaxLoans(UserRole role) => _rules[role].MaxLoans;
        public static int GetReturnDays(UserRole role) => _rules[role].ReturnDays;
        public static DateTime CalculateExpectedReturnDate(UserRole role) =>
            DateTime.UtcNow.AddDays(GetReturnDays(role));

        public static Loan Create(int userId, int bookId, UserRole userRole)
        {
            var expectedReturnDate = CalculateExpectedReturnDate(userRole);
            return new Loan(userId, bookId, expectedReturnDate);
        }
    }
}
