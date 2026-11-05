using System.Net;
using System.Net.Http.Json;
using Biblioteca.Api.Contracts;
using Xunit;

namespace Biblioteca.E2ETests;

// These tests call a separately running API over real HTTP (Kestrel).
// Set E2E_BASE_URL to override the default address.
public class LibraryWorkflowE2ETests
{
    private readonly HttpClient _client = new()
    {
        BaseAddress = new Uri(
            Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:5080")
    };

    [Fact]
    public async Task CompleteBookLifecycle_CreateReadAndDelete()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new CreateBookRequest
        {
            Title = $"Libro E2E {suffix}",
            Author = "Autor de pruebas",
            Isbn = $"978{Random.Shared.Next(1000000000, 1999999999)}",
            PublishedYear = 2022,
            AvailableCopies = 1
        };

        var create = await _client.PostAsJsonAsync("/api/books", request);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(created);

        var get = await _client.GetAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var delete = await _client.DeleteAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var getAfterDelete = await _client.GetAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task LoanLifecycle_CreateAndReturnLoan()
    {
        var request = new CreateLoanRequest
        {
            BookId = 2,
            Borrower = $"E2E {Guid.NewGuid():N}"[..12],
            LoanDate = DateOnly.FromDateTime(DateTime.Today),
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(14))
        };

        var create = await _client.PostAsJsonAsync("/api/loans", request);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var loan = await create.Content.ReadFromJsonAsync<LoanDto>();
        Assert.NotNull(loan);

        var returned = await _client.PutAsync($"/api/loans/{loan.Id}/return", null);
        Assert.Equal(HttpStatusCode.OK, returned.StatusCode);
    }

    private sealed record BookDto(int Id, string Title, string Author, string Isbn, int PublishedYear, int AvailableCopies);
    private sealed record LoanDto(int Id, int BookId, string Borrower, DateOnly LoanDate, DateOnly DueDate, DateOnly? ReturnDate, string Status);
}
