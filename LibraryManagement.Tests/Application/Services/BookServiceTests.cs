using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Services;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using Moq;
using NSubstitute;

namespace LibraryManagement.Tests.Application.Services
{
    public class BookServiceTests
    {
        // ──────────────────────────────────────────
        // GetAllBooksAsync
        // ──────────────────────────────────────────

        /// <summary>
        /// Verifica se todos os livros retornados pelo repositório são corretamente
        /// convertidos para DTOs e se a quantidade retornada corresponde ao total cadastrado.
        /// </summary>
        [Fact]
        public async Task BooksExist_GetAll_ReturnsAllBooksAsDtos()
        {
            var books = new List<Book>
            {
                new("Clean Code", "Robert C. Martin", "9780132350884", "A handbook.", 2008),
                new("The Pragmatic Programmer", "Andrew Hunt", "9780135957059", "Your journey to mastery.", 1999)
            };

            var repository = Substitute.For<IBookRepository>();
            repository.GetAllAsync().Returns(books);

            var service = new BookService(repository);

            var result = await service.GetAllBooksAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            await repository.Received(1).GetAllAsync();
        }

        /// <summary>
        /// Verifica se o serviço retorna uma coleção vazia quando não há
        /// nenhum livro cadastrado no repositório.
        /// </summary>
        [Fact]
        public async Task NoBooksExist_GetAll_ReturnsEmpty()
        {
            var repository = Substitute.For<IBookRepository>();
            repository.GetAllAsync().Returns(new List<Book>());

            var service = new BookService(repository);

            var result = await service.GetAllBooksAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
            await repository.Received(1).GetAllAsync();
        }

        // ──────────────────────────────────────────
        // GetBookByIdAsync
        // ──────────────────────────────────────────

        /// <summary>
        /// Verifica se o serviço retorna o BookDto correto quando um livro
        /// com o ID informado existe no repositório.
        /// </summary>
        [Fact]
        public async Task BookExists_GetById_ReturnsBookDto()
        {
            var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", "A handbook.", 2008);

            var repository = Substitute.For<IBookRepository>();
            repository.GetByIdAsync(1).Returns(book);

            var service = new BookService(repository);

            var result = await service.GetBookByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Clean Code", result.Title);
            Assert.Equal("Robert C. Martin", result.Author);
            await repository.Received(1).GetByIdAsync(1);
        }

        /// <summary>
        /// Verifica se o serviço retorna null quando nenhum livro
        /// corresponde ao ID informado.
        /// </summary>
        [Fact]
        public async Task BookDoesNotExist_GetById_ReturnsNull()
        {
            var repository = Substitute.For<IBookRepository>();
            repository.GetByIdAsync(Arg.Any<int>()).Returns((Book?)null);

            var service = new BookService(repository);

            var result = await service.GetBookByIdAsync(999);

            Assert.Null(result);
            await repository.Received(1).GetByIdAsync(999);
        }

        // ──────────────────────────────────────────
        // CreateBookAsync
        // ──────────────────────────────────────────

        /// <summary>
        /// Verifica se o serviço chama o repositório corretamente e retorna um BookDto
        /// com todas as propriedades preenchidas conforme o DTO de criação informado.
        /// </summary>
        [Fact]
        public async Task ValidData_CreateBook_ReturnsBookDto()
        {
            var createDto = new CreateBookDto("Clean Code", "Robert C. Martin", "9780132350884", "A handbook.", 2008);

            var repository = Substitute.For<IBookRepository>();
            repository.CreateAsync(Arg.Any<Book>()).Returns(Task.CompletedTask);

            var service = new BookService(repository);

            var result = await service.CreateBookAsync(createDto);

            Assert.NotNull(result);
            Assert.Equal("Clean Code", result.Title);
            Assert.Equal("Robert C. Martin", result.Author);
            Assert.Equal("9780132350884", result.ISBN);
            Assert.Equal(2008, result.PublicationYear);
            Assert.True(result.IsAvailable);
            Assert.True(result.Active);
            await repository.Received(1).CreateAsync(Arg.Any<Book>());
        }

        /// <summary>
        /// Verifica se o serviço chama o repositório corretamente e retorna um BookDto
        /// com todas as propriedades preenchidas conforme o DTO de criação informado (MOQ).
        /// </summary>
        [Fact]
        public async Task ValidData_CreateBook_ReturnsBookDto_MOQ()
        {
            var createDto = new CreateBookDto("Clean Code", "Robert C. Martin", "9780132350884", "A handbook.", 2008);

            var mockRepository = new Mock<IBookRepository>();
            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);

            var service = new BookService(mockRepository.Object);

            var result = await service.CreateBookAsync(createDto);

            Assert.NotNull(result);
            Assert.Equal("Clean Code", result.Title);
            Assert.True(result.IsAvailable);
            mockRepository.Verify(r => r.CreateAsync(It.IsAny<Book>()), Times.Once);
        }

        // ──────────────────────────────────────────
        // UpdateBookAsync
        // ──────────────────────────────────────────

        /// <summary>
        /// Verifica se o serviço atualiza corretamente as propriedades do livro existente
        /// e retorna um BookDto com os novos valores informados no DTO de atualização.
        /// </summary>
        [Fact]
        public async Task BookExists_Update_ReturnsUpdatedBookDto()
        {
            var existingBook = new Book("Old Title", "Old Author", "0000000000000", "Old desc.", 2000);
            var updateDto = new UpdateBookDto("New Title", "New Author", "1111111111111", "New desc.", 2024);

            var repository = Substitute.For<IBookRepository>();
            repository.GetByIdAsync(1).Returns(existingBook);
            repository.UpdateAsync(Arg.Any<Book>()).Returns(Task.CompletedTask);

            var service = new BookService(repository);

            var result = await service.UpdateBookAsync(1, updateDto);

            Assert.NotNull(result);
            Assert.Equal("New Title", result.Title);
            Assert.Equal("New Author", result.Author);
            Assert.Equal("1111111111111", result.ISBN);
            Assert.Equal(2024, result.PublicationYear);
            await repository.Received(1).GetByIdAsync(1);
            await repository.Received(1).UpdateAsync(Arg.Any<Book>());
        }

        /// <summary>
        /// Verifica se o serviço lança uma KeyNotFoundException contendo o ID informado
        /// quando tenta atualizar um livro que não existe, garantindo que o repositório
        /// não chame o método de atualização desnecessariamente.
        /// </summary>
        [Fact]
        public async Task BookDoesNotExist_Update_ThrowsKeyNotFoundException()
        {
            var updateDto = new UpdateBookDto("Title", "Author", "ISBN", "Desc", 2024);

            var repository = Substitute.For<IBookRepository>();
            repository.GetByIdAsync(999).Returns((Book?)null);

            var service = new BookService(repository);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.UpdateBookAsync(999, updateDto)
            );

            Assert.Contains("999", exception.Message);
            await repository.Received(1).GetByIdAsync(999);
            await repository.DidNotReceive().UpdateAsync(Arg.Any<Book>());
        }

        /// <summary>
        /// Verifica se o serviço lança uma KeyNotFoundException contendo o ID informado
        /// quando tenta atualizar um livro que não existe, garantindo que o repositório
        /// não chame o método de atualização desnecessariamente (MOQ).
        /// </summary>
        [Fact]
        public async Task BookDoesNotExist_Update_ThrowsKeyNotFoundException_MOQ()
        {
            var updateDto = new UpdateBookDto("Title", "Author", "ISBN", "Desc", 2024);

            var mockRepository = new Mock<IBookRepository>();
            mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Book?)null);

            var service = new BookService(mockRepository.Object);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.UpdateBookAsync(999, updateDto)
            );

            Assert.Contains("999", exception.Message);
            mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
            mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
        }

        // ──────────────────────────────────────────
        // DeleteBookAsync
        // ──────────────────────────────────────────

        /// <summary>
        /// Verifica se o serviço desativa o livro e chama o método UpdateAsync do repositório
        /// exatamente uma vez quando o livro com o ID informado existe e está ativo.
        /// </summary>
        [Fact]
        public async Task BookExists_Delete_Success()
        {
            var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", "A handbook.", 2008);

            var repository = Substitute.For<IBookRepository>();
            repository.GetByIdAsync(1).Returns(book);
            repository.UpdateAsync(Arg.Any<Book>()).Returns(Task.CompletedTask);

            var service = new BookService(repository);

            await service.DeleteBookAsync(1);

            Assert.False(book.Active);
            await repository.Received(1).GetByIdAsync(1);
            await repository.Received(1).UpdateAsync(book);
        }

        /// <summary>
        /// Verifica se o serviço lança uma KeyNotFoundException contendo o ID informado
        /// quando tenta excluir um livro que não existe, garantindo que o método
        /// UpdateAsync do repositório não seja chamado.
        /// </summary>
        [Fact]
        public async Task BookDoesNotExist_Delete_ThrowsKeyNotFoundException()
        {
            var repository = Substitute.For<IBookRepository>();
            repository.GetByIdAsync(Arg.Any<int>()).Returns((Book?)null);

            var service = new BookService(repository);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.DeleteBookAsync(999)
            );

            Assert.Contains("999", exception.Message);
            await repository.Received(1).GetByIdAsync(999);
            await repository.DidNotReceive().UpdateAsync(Arg.Any<Book>());
        }

        /// <summary>
        /// Verifica se o serviço lança uma InvalidOperationException
        /// quando tenta excluir um livro que já está inativo.
        /// </summary>
        [Fact]
        public async Task BookAlreadyInactive_Delete_ThrowsInvalidOperationException()
        {
            var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", "A handbook.", 2008);
            book.Deactivate();

            var repository = Substitute.For<IBookRepository>();
            repository.GetByIdAsync(1).Returns(book);

            var service = new BookService(repository);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteBookAsync(1)
            );

            Assert.Equal("Book is already inactive.", exception.Message);
            await repository.Received(1).GetByIdAsync(1);
            await repository.DidNotReceive().UpdateAsync(Arg.Any<Book>());
        }

        /// <summary>
        /// Verifica se o serviço lança uma KeyNotFoundException contendo o ID informado
        /// quando tenta excluir um livro que não existe, garantindo que o método
        /// UpdateAsync do repositório não seja chamado (MOQ).
        /// </summary>
        [Fact]
        public async Task BookDoesNotExist_Delete_ThrowsKeyNotFoundException_MOQ()
        {
            var mockRepository = new Mock<IBookRepository>();
            mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Book?)null);

            var service = new BookService(mockRepository.Object);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.DeleteBookAsync(999)
            );

            Assert.Contains("999", exception.Message);
            mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
            mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
        }
    }
}