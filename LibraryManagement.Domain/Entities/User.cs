using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Domain.Entities
{
    public class User
    {
        public User(string name, string email, string password, UserRole userRole)
        {
            Name = name;
            Email = email;
            Password = password;
            UserRole = userRole;
        }

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public UserRole UserRole { get; private set; }
    }
}
