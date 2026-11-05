namespace Biblioteca.Api.Models;

public sealed class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Isbn { get; set; } = "";
    public int PublishedYear { get; set; }
    public int AvailableCopies { get; set; }
}
