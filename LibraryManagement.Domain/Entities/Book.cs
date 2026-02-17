namespace LibraryManagement.Domain.Entities
{
    public class Book
    {
        public int Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Author { get; private set; } = string.Empty;
        public string ISBN { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public int PublicationYear { get; private set; }
        public bool IsAvailable { get; private set; }
        public bool Active { get; private set; }
    }
}
