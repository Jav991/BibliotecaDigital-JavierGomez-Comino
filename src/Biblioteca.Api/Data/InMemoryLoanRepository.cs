using Biblioteca.Api.Models;

namespace Biblioteca.Api.Data;

public sealed class InMemoryLoanRepository : ILoanRepository
{
    private readonly List<Loan> _loans = [];
    private int _nextId = 1;

    public IReadOnlyCollection<Loan> GetAll() => _loans.Select(Clone).ToArray();
    public Loan? GetById(int id) => _loans.FirstOrDefault(x => x.Id == id) is { } loan ? Clone(loan) : null;

    public Loan Add(Loan loan)
    {
        loan.Id = _nextId++;
        _loans.Add(Clone(loan));
        return Clone(loan);
    }

    public void Update(Loan loan)
    {
        var existing = _loans.FirstOrDefault(x => x.Id == loan.Id);
        if (existing is null) return;
        existing.Status = loan.Status; existing.ReturnDate = loan.ReturnDate;
    }

    private static Loan Clone(Loan x) => new()
    {
        Id = x.Id, BookId = x.BookId, Borrower = x.Borrower,
        LoanDate = x.LoanDate, DueDate = x.DueDate,
        ReturnDate = x.ReturnDate, Status = x.Status
    };
}
