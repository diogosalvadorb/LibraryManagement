using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Application.Mapping;
using LibraryManagement.Domain.Interfaces;

namespace LibraryManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        public UserService(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var hashedPassword = _authService.ComputeSha256Hash(createUserDto.Password);

            var createUserWithHashedPassword = createUserDto with { Password = hashedPassword };

            var useEntity = createUserWithHashedPassword.ToEntity();

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
            var hashedPassword = _authService.ComputeSha256Hash(loginDto.Password);

            var user = await _userRepository.GetUserByEmailAndPasswordAsync(loginDto.Email, hashedPassword);

            if (user is null)
                throw new Exception("Invalid email or password.");

            var token = _authService.GenerateTokenJwt(user.Email, user.UserRole.ToString());

            return new AuthResponseDto(token);
        }
    }
}
