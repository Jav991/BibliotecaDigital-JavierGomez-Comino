using Biblioteca.Api.Models;

namespace Biblioteca.Api.Data;

public sealed class InMemoryBookRepository : IBookRepository, IBookUpdateRepository
{
    private readonly List<Book> _books =
    [
        new() { Id = 1, Title = "Cien años de soledad", Author = "Gabriel García Márquez", Isbn = "9780307474728", PublishedYear = 1967, AvailableCopies = 3 },
        new() { Id = 2, Title = "La sombra del viento", Author = "Carlos Ruiz Zafón", Isbn = "9788408079545", PublishedYear = 2001, AvailableCopies = 2 },
        new() { Id = 3, Title = "El nombre del viento", Author = "Patrick Rothfuss", Isbn = "9788401352836", PublishedYear = 2007, AvailableCopies = 0 }
    ];
    private int _nextId = 4;

    public IReadOnlyCollection<Book> GetAll() => _books.Select(Clone).ToArray();
    public Book? GetById(int id) => _books.FirstOrDefault(x => x.Id == id) is { } book ? Clone(book) : null;

    public Book Add(Book book)
    {
        book.Id = _nextId++;
        _books.Add(Clone(book));
        return Clone(book);
    }

    public void Update(Book book)
    {
        var existing = _books.FirstOrDefault(x => x.Id == book.Id);
        if (existing is null) return;
        existing.Title = book.Title; existing.Author = book.Author; existing.Isbn = book.Isbn;
        existing.PublishedYear = book.PublishedYear; existing.AvailableCopies = book.AvailableCopies;
    }

    public bool Delete(int id)
    {
        var book = _books.FirstOrDefault(x => x.Id == id);
        return book is not null && _books.Remove(book);
    }

    private static Book Clone(Book b) => new()
    {
        Id = b.Id, Title = b.Title, Author = b.Author, Isbn = b.Isbn,
        PublishedYear = b.PublishedYear, AvailableCopies = b.AvailableCopies
    };
}
