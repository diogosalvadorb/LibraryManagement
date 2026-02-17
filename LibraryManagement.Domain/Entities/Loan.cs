using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Domain.Entities
{
    public class Loan
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public User? User { get; private set; }
        public int BookId { get; private set; }
        public Book? Book { get; private set; }
        public DateTime? LoanDate { get; private set; }
        public DateTime? ExpectedReturnDate { get; private set; }
        public DateTime? ReturnDate { get; private set; }
        public LoanStatus LoanStatus { get; private set; }
        public bool Active { get; private set; }
    }
}
