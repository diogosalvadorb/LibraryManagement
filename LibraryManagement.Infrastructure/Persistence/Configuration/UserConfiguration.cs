using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Infrastructure.Persistence.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
               .IsRequired()
               .HasMaxLength(100);

            builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(320);

            builder.Property(u => u.Password)
               .IsRequired()
               .HasMaxLength(255);

            builder.Property(u => u.UserRole)
               .IsRequired()
               .HasMaxLength(50);
        }
    }
}
