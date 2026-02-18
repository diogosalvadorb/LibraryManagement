using LibraryManagement.Application.DTOs;

namespace LibraryManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }
}
