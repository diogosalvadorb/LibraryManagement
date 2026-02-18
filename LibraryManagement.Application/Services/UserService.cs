using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Application.Mapping;
using LibraryManagement.Domain.Interfaces;

namespace LibraryManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var useEntity = createUserDto.ToEntity();

            await _userRepository.CreateUserAsync(useEntity);

            return useEntity.ToDto();
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();

            return users.Select(x => x.ToDto());
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            return user?.ToDto();
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByEmailAndPasswordAsync(loginDto.Email, loginDto.Password);

            if (user is null)
            {
                throw new Exception("Invalid email or password.");
            }
            
            var token = Guid.NewGuid().ToString();

            return new AuthResponseDto(token);
        }
    }
}
