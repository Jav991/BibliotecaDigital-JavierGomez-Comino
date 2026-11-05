using Biblioteca.Api.Contracts;
using Biblioteca.Api.Data;
using Biblioteca.Api.Models;

namespace Biblioteca.Api.Services;

public sealed class LibraryService(IBookRepository books, ILoanRepository loans)
{
    public IReadOnlyCollection<Book> GetBooks() => books.GetAll();
    public Book? GetBook(int id) => books.GetById(id);

    public (Book? Book, string? Error) CreateBook(CreateBookRequest request)
    {
        if (books.GetAll().Any(x => string.Equals(x.Isbn, request.Isbn, StringComparison.OrdinalIgnoreCase)))
            return (null, "Ya existe un libro con ese ISBN.");

        var book = books.Add(new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Isbn = request.Isbn.Trim(),
            PublishedYear = request.PublishedYear,
            AvailableCopies = request.AvailableCopies
        });
        return (book, null);
    }

    public bool DeleteBook(int id)
    {
        if (loans.GetAll().Any(x => x.BookId == id && x.Status == "Activo"))
            return false;
        return books.Delete(id);
    }

    public IReadOnlyCollection<Loan> GetLoans() => loans.GetAll();
    public Loan? GetLoan(int id) => loans.GetById(id);

    public (Loan? Loan, string? Error) CreateLoan(CreateLoanRequest request)
    {
        var book = books.GetById(request.BookId);
        if (book is null) return (null, "No existe el libro indicado.");
        if (book.AvailableCopies <= 0) return (null, "No hay ejemplares disponibles.");
        if (request.DueDate <= request.LoanDate) return (null, "La fecha de devolución debe ser posterior al préstamo.");

        book.AvailableCopies--;
        if (books is IBookUpdateRepository bookUpdater)
            bookUpdater.Update(book);
        var loan = loans.Add(new Loan
        {
            BookId = request.BookId,
            Borrower = request.Borrower.Trim(),
            LoanDate = request.LoanDate,
            DueDate = request.DueDate,
            Status = "Activo"
        });
        return (loan, null);
    }

    public (Loan? Loan, string? Error) ReturnLoan(int id)
    {
        var loan = loans.GetById(id);
        if (loan is null) return (null, "No existe el préstamo indicado.");
        if (loan.Status == "Devuelto") return (null, "El préstamo ya está devuelto.");

        loan.Status = "Devuelto";
        loan.ReturnDate = DateOnly.FromDateTime(DateTime.Today);
        loans.Update(loan);
        var book = books.GetById(loan.BookId);
        if (book is not null && books is IBookUpdateRepository bookUpdater)
        {
            book.AvailableCopies++;
            bookUpdater.Update(book);
        }
        return (loan, null);
    }
}
