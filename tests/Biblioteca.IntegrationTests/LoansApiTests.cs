using System.Net;
using System.Net.Http.Json;
using Biblioteca.Api.Contracts;
using Xunit;

namespace Biblioteca.IntegrationTests;

public class LoansApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateLoan_WithAvailableBook_ReturnsCreated()
    {
        var request = new CreateLoanRequest
        {
            BookId = 1, Borrower = "María Pérez",
            LoanDate = new DateOnly(2026, 11, 3),
            DueDate = new DateOnly(2026, 11, 17)
        };

        var response = await _client.PostAsJsonAsync("/api/loans", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateLoan_WithUnavailableBook_ReturnsBadRequest()
    {
        var request = new CreateLoanRequest
        {
            BookId = 3, Borrower = "María Pérez",
            LoanDate = new DateOnly(2026, 11, 3),
            DueDate = new DateOnly(2026, 11, 17)
        };

        var response = await _client.PostAsJsonAsync("/api/loans", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReturnLoan_UnknownId_ReturnsNotFound()
    {
        var response = await _client.PutAsync("/api/loans/9999/return", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateLoan_WithUnknownBook_ReturnsBadRequest()
    {
        var request = new CreateLoanRequest
        {
            BookId = 9999, Borrower = "María Pérez",
            LoanDate = new DateOnly(2026, 11, 3),
            DueDate = new DateOnly(2026, 11, 17)
        };

        var response = await _client.PostAsJsonAsync("/api/loans", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReturnLoan_WhenAlreadyReturned_ReturnsConflict()
    {
        var createBook = await _client.PostAsJsonAsync("/api/books", new CreateBookRequest
        {
            Title = "Libro de integración", Author = "Autor de pruebas",
            Isbn = $"978{Random.Shared.Next(1_000_000_000, 1_999_999_999)}",
            PublishedYear = 2022, AvailableCopies = 1
        });
        Assert.Equal(HttpStatusCode.Created, createBook.StatusCode);
        var book = await createBook.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);

        var createLoan = await _client.PostAsJsonAsync("/api/loans", new CreateLoanRequest
        {
            BookId = book.Id, Borrower = "María Pérez",
            LoanDate = new DateOnly(2026, 11, 3),
            DueDate = new DateOnly(2026, 11, 17)
        });
        Assert.Equal(HttpStatusCode.Created, createLoan.StatusCode);
        var loan = await createLoan.Content.ReadFromJsonAsync<LoanDto>();
        Assert.NotNull(loan);

        var firstReturn = await _client.PutAsync($"/api/loans/{loan.Id}/return", null);
        Assert.Equal(HttpStatusCode.OK, firstReturn.StatusCode);

        var secondReturn = await _client.PutAsync($"/api/loans/{loan.Id}/return", null);

        Assert.Equal(HttpStatusCode.Conflict, secondReturn.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/books/{book.Id}")).StatusCode);
    }

    private sealed record BookDto(int Id, string Title, string Author, string Isbn, int PublishedYear, int AvailableCopies);
    private sealed record LoanDto(int Id, int BookId, string Borrower, DateOnly LoanDate, DateOnly DueDate, DateOnly? ReturnDate, string Status);
}
