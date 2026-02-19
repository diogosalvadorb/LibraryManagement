using LibraryManagement.Application.DTOs;
using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Application.Mapping
{
    public static class BookDTOExtensions
    {
        public static BookDto ToDto(this Book book)
        {
            return new BookDto(
                book.Id,
                book.Title,
                book.Author,
                book.ISBN,
                book.Description,
                book.PublicationYear,
                book.IsAvailable,
                book.Active);
        }

        public static Book ToEntity(this CreateBookDto dto)
        {
            return new Book(
                dto.Title,
                dto.Author,
                dto.ISBN,
                dto.Description,
                dto.PublicationYear);
        }
    }
}