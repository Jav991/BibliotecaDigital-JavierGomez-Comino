using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Api.Contracts;

public sealed class CreateLoanRequest
{
    [Range(1, int.MaxValue)]
    public int BookId { get; init; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Borrower { get; init; } = "";

    public DateOnly LoanDate { get; init; }
    public DateOnly DueDate { get; init; }
}
