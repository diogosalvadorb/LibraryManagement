using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Application.Mapping;
using LibraryManagement.Domain.Interfaces;

namespace LibraryManagement.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var books = await _bookRepository.GetAllAsync();

            return books.Select(b => b.ToDto());
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);

            return book?.ToDto();
        }

        public async Task<BookDto> CreateBookAsync(CreateBookDto createBookDto)
        {
            var existing = await _bookRepository.GetByISBNAsync(createBookDto.ISBN);

            if (existing is not null)
                throw new InvalidOperationException($"A book with ISBN '{createBookDto.ISBN}' already exists.");

            var book = createBookDto.ToEntity();

            await _bookRepository.CreateAsync(book);

            return book.ToDto();
        }

        public async Task<BookDto> UpdateBookAsync(int id, UpdateBookDto updateBookDto)
        {
            var book = await _bookRepository.GetByIdAsync(id);

            if (book is null)
                throw new KeyNotFoundException($"Book with id {id} was not found.");

            if (!book.Active)
                throw new InvalidOperationException("Cannot update an inactive book.");

            if (!string.Equals(book.ISBN, updateBookDto.ISBN, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _bookRepository.GetByISBNAsync(updateBookDto.ISBN);

                if (existing is not null)
                    throw new InvalidOperationException($"A book with ISBN '{updateBookDto.ISBN}' already exists.");
            }

            book.Update(
                updateBookDto.Title,
                updateBookDto.Author,
                updateBookDto.ISBN,
                updateBookDto.Description,
                updateBookDto.PublicationYear);

            await _bookRepository.UpdateAsync(book);

            return book.ToDto();
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);

            if (book is null)
                throw new KeyNotFoundException($"Book with id {id} was not found.");

            if (!book.Active)
                throw new InvalidOperationException("Book is already inactive.");

            book.Deactivate();

            await _bookRepository.UpdateAsync(book);
        }
    }
}