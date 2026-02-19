using System;
using System.Collections.Generic;
using System.Linq;
namespace LibraryManagement.Application.DTOs
{
    public sealed record BookDto(
        int Id,
        string Title,
        string Author,
        string ISBN,
        string Description,
        int PublicationYear,
        bool IsAvailable,
        bool Active);

    public sealed record CreateBookDto(
        string Title,
        string Author,
        string ISBN,
        string Description,
        int PublicationYear);

    public sealed record UpdateBookDto(
        string Title,
        string Author,
        string ISBN,
        string Description,
        int PublicationYear);
}
