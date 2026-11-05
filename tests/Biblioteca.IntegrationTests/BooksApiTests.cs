using System.Net;
using System.Net.Http.Json;
using Biblioteca.Api.Contracts;
using Xunit;

namespace Biblioteca.IntegrationTests;

public class BooksApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetBooks_ReturnsSeededCatalog()
    {
        var response = await _client.GetAsync("/api/books");
        response.EnsureSuccessStatusCode();

        var books = await response.Content.ReadFromJsonAsync<List<BookResponse>>();
        Assert.NotNull(books);
        Assert.True(books.Count >= 3);
        Assert.Contains(books, x => x.Title == "Cien años de soledad");
    }

    [Fact]
    public async Task GetBook_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/books/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_WithValidData_ReturnsCreated()
    {
        var request = new CreateBookRequest
        {
            Title = "El código Da Vinci", Author = "Dan Brown",
            Isbn = "9780307474278", PublishedYear = 2003, AvailableCopies = 4
        };

        var response = await _client.PostAsJsonAsync("/api/books", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateBook_WithDuplicateIsbn_ReturnsConflict()
    {
        var request = new CreateBookRequest
        {
            Title = "Duplicado", Author = "Autor",
            Isbn = "9780307474728", PublishedYear = 2020, AvailableCopies = 1
        };

        var response = await _client.PostAsJsonAsync("/api/books", request);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private sealed record BookResponse(int Id, string Title, string Author, string Isbn, int PublishedYear, int AvailableCopies);
}
