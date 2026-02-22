using FluentValidation.TestHelper;
using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Validators;

namespace LibraryManagement.Tests.Application.Validators
{
    public class BookDtoValidatorTests
    {
        private readonly CreateBookDtoValidator _createValidator = new();
        private readonly UpdateBookDtoValidator _updateValidator = new();

        private static CreateBookDto ValidCreate() =>
            new("Clean Code", "Robert C. Martin", "9780131103627", "A great book about clean code.", 2008);

        private static UpdateBookDto ValidUpdate() =>
            new("Clean Code", "Robert C. Martin", "9780131103627", "A great book about clean code.", 2008);

        // ── Title ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Testa se ocorre erro de validação quando o Título está vazio ou nulo.
        /// </summary>
        [Fact]
        public void Create_Should_Have_Error_When_Title_Is_Empty()
        {
            var result = _createValidator.TestValidate(ValidCreate() with { Title = "" });
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Title is required.");
        }

        /// <summary>
        /// Testa se ocorre erro de validação quando o Título excede o limite máximo de caracteres (200).
        /// </summary>
        [Fact]
        public void Create_Should_Have_Error_When_Title_Exceeds_MaxLength()
        {
            var result = _createValidator.TestValidate(ValidCreate() with { Title = new string('A', 201) });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        // ── Author ────────────────────────────────────────────────────────────

        /// <summary>
        /// Testa se ocorre erro de validação quando o campo Autor está vazio ou nulo.
        /// </summary>
        [Fact]
        public void Create_Should_Have_Error_When_Author_Is_Empty()
        {
            var result = _createValidator.TestValidate(ValidCreate() with { Author = "" });
            result.ShouldHaveValidationErrorFor(x => x.Author)
                  .WithErrorMessage("Author is required.");
        }

        // ── ISBN ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Testa se ocorre erro de validação quando o ISBN contém caracteres não numéricos (ex: hífens).
        /// </summary>
        [Fact]
        public void Create_Should_Have_Error_When_ISBN_Has_Non_Digits()
        {
            var result = _createValidator.TestValidate(ValidCreate() with { ISBN = "978-013-1103" });
            result.ShouldHaveValidationErrorFor(x => x.ISBN);
        }

        /// <summary>
        /// Testa se ocorre erro de validação quando o ISBN tem menos caracteres do que o permitido.
        /// </summary>
        [Fact]
        public void Create_Should_Have_Error_When_ISBN_Is_Too_Short()
        {
            var result = _createValidator.TestValidate(ValidCreate() with { ISBN = "123456789" });
            result.ShouldHaveValidationErrorFor(x => x.ISBN);
        }

        /// <summary>
        /// Testa se ocorre erro de validação quando o ISBN ultrapassa o limite máximo de caracteres.
        /// </summary>
        [Fact]
        public void Create_Should_Have_Error_When_ISBN_Is_Too_Long()
        {
            var result = _createValidator.TestValidate(ValidCreate() with { ISBN = "12345678901234" });
            result.ShouldHaveValidationErrorFor(x => x.ISBN);
        }

        // ── PublicationYear ───────────────────────────────────────────────────

        /// <summary>
        /// Testa se ocorre erro de validação quando o ano de publicação é superior ao ano atual.
        /// </summary>
        [Fact]
        public void Create_Should_Have_Error_When_PublicationYear_Is_In_Future()
        {
            var futureYear = DateTime.UtcNow.Year + 1;
            var result = _createValidator.TestValidate(ValidCreate() with { PublicationYear = futureYear });
            result.ShouldHaveValidationErrorFor(x => x.PublicationYear);
        }

        /// <summary>
        /// Testa se ocorre erro de validação quando o ano de publicação é inferior ao limite histórico (1000).
        /// </summary>
        [Fact]
        public void Create_Should_Have_Error_When_PublicationYear_Is_Below_1000()
        {
            var result = _createValidator.TestValidate(ValidCreate() with { PublicationYear = 999 });
            result.ShouldHaveValidationErrorFor(x => x.PublicationYear);
        }

        // ── Happy path ────────────────────────────────────────────────────────

        /// <summary>
        /// Verifica se o DTO de criação (CreateBookDto) passa na validação quando todos os dados são válidos.
        /// </summary>
        [Fact]
        public void Create_Should_Not_Have_Errors_When_Dto_Is_Valid()
        {
            var result = _createValidator.TestValidate(ValidCreate());
            result.ShouldNotHaveAnyValidationErrors();
        }

        /// <summary>
        /// Verifica se o DTO de atualização (UpdateBookDto) passa na validação quando todos os dados são válidos.
        /// </summary>
        [Fact]
        public void Update_Should_Not_Have_Errors_When_Dto_Is_Valid()
        {
            var result = _updateValidator.TestValidate(ValidUpdate());
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}