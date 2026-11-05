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

    [Fact]
    public async Task CompleteLoanLifecycle_TracksCopiesAndRejectsSecondReturn()
    {
        var book = await CreateBookAsync();
        var initial = await _client.GetFromJsonAsync<BookDto>($"/api/books/{book.Id}");
        Assert.NotNull(initial);
        Assert.Equal(1, initial.AvailableCopies);

        var loan = await CreateLoanAsync(book.Id);
        var afterLoan = await _client.GetFromJsonAsync<BookDto>($"/api/books/{book.Id}");
        Assert.NotNull(afterLoan);
        Assert.Equal(0, afterLoan.AvailableCopies);

        var retrievedLoan = await _client.GetFromJsonAsync<LoanDto>($"/api/loans/{loan.Id}");
        Assert.NotNull(retrievedLoan);
        Assert.Equal("Activo", retrievedLoan.Status);

        var returned = await _client.PutAsync($"/api/loans/{loan.Id}/return", null);
        Assert.Equal(HttpStatusCode.OK, returned.StatusCode);
        var returnedLoan = await _client.GetFromJsonAsync<LoanDto>($"/api/loans/{loan.Id}");
        Assert.NotNull(returnedLoan);
        Assert.Equal("Devuelto", returnedLoan.Status);

        var restoredBook = await _client.GetFromJsonAsync<BookDto>($"/api/books/{book.Id}");
        Assert.NotNull(restoredBook);
        Assert.Equal(1, restoredBook.AvailableCopies);

        var secondReturn = await _client.PutAsync($"/api/loans/{loan.Id}/return", null);
        Assert.Equal(HttpStatusCode.Conflict, secondReturn.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/books/{book.Id}")).StatusCode);
    }

    [Fact]
    public async Task DeleteBook_WithActiveLoan_IsRejectedUntilLoanIsReturned()
    {
        var book = await CreateBookAsync();
        var loan = await CreateLoanAsync(book.Id);

        var deleteWhileLoanIsActive = await _client.DeleteAsync($"/api/books/{book.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteWhileLoanIsActive.StatusCode);

        var returned = await _client.PutAsync($"/api/loans/{loan.Id}/return", null);
        Assert.Equal(HttpStatusCode.OK, returned.StatusCode);

        var deleteAfterReturn = await _client.DeleteAsync($"/api/books/{book.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteAfterReturn.StatusCode);
        var getAfterDelete = await _client.GetAsync($"/api/books/{book.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    private async Task<BookDto> CreateBookAsync()
    {
        var request = new CreateBookRequest
        {
            Title = $"Libro E2E {Guid.NewGuid():N}"[..20],
            Author = "Autor de pruebas",
            Isbn = $"978{Random.Shared.Next(1_000_000_000, 1_999_999_999)}",
            PublishedYear = 2022,
            AvailableCopies = 1
        };

        var response = await _client.PostAsJsonAsync("/api/books", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        return book;
    }

    private async Task<LoanDto> CreateLoanAsync(int bookId)
    {
        var request = new CreateLoanRequest
        {
            BookId = bookId,
            Borrower = $"E2E {Guid.NewGuid():N}"[..12],
            LoanDate = DateOnly.FromDateTime(DateTime.Today),
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(14))
        };

        var response = await _client.PostAsJsonAsync("/api/loans", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var loan = await response.Content.ReadFromJsonAsync<LoanDto>();
        Assert.NotNull(loan);
        return loan;
    }

    private sealed record BookDto(int Id, string Title, string Author, string Isbn, int PublishedYear, int AvailableCopies);
    private sealed record LoanDto(int Id, int BookId, string Borrower, DateOnly LoanDate, DateOnly DueDate, DateOnly? ReturnDate, string Status);
}
