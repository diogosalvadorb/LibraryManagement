using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Infrastructure.Persistence.Configuration
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title)
               .IsRequired()
               .HasMaxLength(200);

            builder.Property(b => b.Author)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.ISBN)
                .IsRequired()
                .HasMaxLength(13);

            builder.Property(b => b.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(b => b.PublicationYear)
                .IsRequired();
        }
    }
}
