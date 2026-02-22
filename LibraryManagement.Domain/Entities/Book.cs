namespace LibraryManagement.Domain.Entities
{
    public class Book
    {
        public Book(string title, string author, string iSBN, string description, int publicationYear)
        {
            Title = title;
            Author = author;
            ISBN = iSBN;
            Description = description;
            PublicationYear = publicationYear;
            IsAvailable = true;
            Active = true;
        }

        public int Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Author { get; private set; } = string.Empty;
        public string ISBN { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public int PublicationYear { get; private set; }
        public bool IsAvailable { get; private set; }
        public bool Active { get; private set; }

        public void Deactivate()
        {
            Active = false;
        }

        public void Update(string title, string author, string isbn, string description, int publicationYear)
        {
            Title = title;
            Author = author;
            ISBN = isbn;
            Description = description;
            PublicationYear = publicationYear;
        }
    }
}
