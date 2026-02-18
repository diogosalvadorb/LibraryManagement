using LibraryManagement.Application.DTOs;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Application.Mapping
{
    public static class UserDTOExtensions
    {
        public static UserDto ToDto(this User user)
        {
            return new UserDto(user.Id, user.Name, user.Email, user.UserRole.ToString());
        }

        public static User ToEntity(this CreateUserDto request)
        {
            var roleEnum = (UserRole)request.Role;

            return new User(
                request.Name,
                request.Email,
                request.Password,
                roleEnum
            );
        }
    }
}
