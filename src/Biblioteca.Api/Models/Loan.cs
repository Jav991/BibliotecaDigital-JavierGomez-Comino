namespace Biblioteca.Api.Models;

public sealed class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string Borrower { get; set; } = "";
    public DateOnly LoanDate { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public string Status { get; set; } = "Activo";
}
