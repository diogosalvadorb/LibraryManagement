using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public UserRole UserRole { get; private set; }
    }
}
