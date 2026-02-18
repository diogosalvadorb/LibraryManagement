using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Persistence.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataBaseContext _context;
        public UserRepository(DataBaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();

            return users;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User?> GetUserByEmailAndPasswordAsync(string email, string password)
        {
            return await _context.Users.SingleOrDefaultAsync(x => x.Email == email && x.Password == password);
        }

        public async Task CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
