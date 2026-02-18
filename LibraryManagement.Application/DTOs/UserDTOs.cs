namespace LibraryManagement.Application.DTOs
{
    public sealed record UserDto(int Id, string Name, string Email, string Role);
    public sealed record CreateUserDto(string Name, string Email, string Password, int Role);
    public sealed record LoginDto(string Email, string Password);
    public sealed record AuthResponseDto(string Token); 
}
