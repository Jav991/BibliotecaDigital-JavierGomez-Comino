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
}
