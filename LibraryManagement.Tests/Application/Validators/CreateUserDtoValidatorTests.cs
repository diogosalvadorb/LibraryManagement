using FluentValidation.TestHelper;
using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Validators;

namespace LibraryManagement.Tests.Application.Validators
{
    public class CreateUserDtoValidatorTests
    {
        private readonly CreateUserDtoValidator _validator;

        public CreateUserDtoValidatorTests()
        {
            _validator = new CreateUserDtoValidator();
        }

        /// <summary>
        /// Testa se ocorre erro quando o Nome é nulo ou vazio.
        /// </summary>
        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            // Arrange
            var model = new CreateUserDto("", "email@test.com", "StrongPass1!", 1);

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Name is required.");
        }

        /// <summary>
        /// Testa se ocorre erro quando o Nome tem menos caracteres do que o permitido (min 3).
        /// </summary>
        [Fact]
        public void Should_Have_Error_When_Name_Is_Too_Short()
        {
            var model = new CreateUserDto("Ab", "email@test.com", "StrongPass1!", 1);

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Name must be at least 3 characters long.");
        }

        /// <summary>
        /// Testa se ocorre erro quando o formato do Email é inválido.
        /// </summary>
        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var model = new CreateUserDto("John Doe", "invalid-email", "StrongPass1!", 1);

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("A valid email address is required.");
        }

        /// <summary>
        /// Testa vários cenários de senha fraca (curta, sem número, sem maiúscula, etc).
        /// </summary>
        [Theory]
        [InlineData("short1!", "Password must not exceed 255 characters.")]
        [InlineData("nonumber!", "The password must contain at least 8 characters: one number, one hidden letter, one lowercase letter, and one special character.")]
        [InlineData("NOLOWERCASE1!", "The password must contain at least 8 characters: one number, one hidden letter, one lowercase letter, and one special character.")]
        [InlineData("nouppercase1!", "The password must contain at least 8 characters: one number, one hidden letter, one lowercase letter, and one special character.")]
        [InlineData("NoSpecialChar1", "The password must contain at least 8 characters: one number, one hidden letter, one lowercase letter, and one special character.")]
        public void Should_Have_Error_When_Password_Does_Not_Meet_Complexity_Requirements(string weakPassword, string expectedMessage)
        {
            var model = new CreateUserDto("John Doe", "john@test.com", weakPassword, 1);

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        /// <summary>
        /// Testa se ocorre erro quando o ID da Role (Cargo) não existe no Enum.
        /// </summary>
        [Fact]
        public void Should_Have_Error_When_Role_Is_Invalid()
        {
            // Arrange (5 is not a valid UserRole)
            var model = new CreateUserDto("John Doe", "john@test.com", "StrongPass1!", 5);

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Role)
                  .WithErrorMessage("The provided User Role is invalid.");
        }

        /// <summary>
        /// Testa o "Caminho Feliz": DTO válido não deve gerar erros.
        /// </summary>
        [Fact]
        public void Should_Not_Have_Error_When_Dto_Is_Valid()
        {
            var model = new CreateUserDto("John Doe", "john@test.com", "StrongPass1!", 1);

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
