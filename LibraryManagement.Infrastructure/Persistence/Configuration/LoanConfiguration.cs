using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Infrastructure.Persistence.Configuration
{
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.LoanDate)
                .IsRequired();

            builder.Property(l => l.ExpectedReturnDate)
                .IsRequired();

            builder.Property(l => l.ReturnDate)
                .IsRequired(false);

            builder.Property(l => l.LoanStatus)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(l => l.Active)
                .IsRequired();

            builder.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.Book)
                .WithMany()
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
