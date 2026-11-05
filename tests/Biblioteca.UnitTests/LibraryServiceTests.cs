using Biblioteca.Api.Contracts;
using Biblioteca.Api.Data;
using Biblioteca.Api.Models;
using Biblioteca.Api.Services;
using Moq;
using Xunit;

namespace Biblioteca.UnitTests;

public class LibraryServiceTests
{
    private readonly Mock<IBookRepository> _books = new();
    private readonly Mock<ILoanRepository> _loans = new();

    private LibraryService CreateService() => new(_books.Object, _loans.Object);

    [Fact]
    public void CreateBook_WithUniqueIsbn_ReturnsBook()
    {
        _books.Setup(x => x.GetAll()).Returns([]);
        _books.Setup(x => x.Add(It.IsAny<Book>()))
            .Returns((Book b) => { b.Id = 10; return b; });

        var result = CreateService().CreateBook(new CreateBookRequest
        {
            Title = "Clean Code", Author = "Robert Martin",
            Isbn = "9780132350884", PublishedYear = 2008, AvailableCopies = 2
        });

        Assert.Null(result.Error);
        Assert.Equal(10, result.Book!.Id);
        Assert.Equal("Clean Code", result.Book.Title);
    }

    [Fact]
    public void CreateBook_WithExistingIsbn_ReturnsConflictError()
    {
        _books.Setup(x => x.GetAll()).Returns([
            new Book { Id = 1, Isbn = "9780132350884" }
        ]);

        var result = CreateService().CreateBook(new CreateBookRequest
        {
            Title = "Another book", Author = "Author",
            Isbn = "9780132350884", PublishedYear = 2020, AvailableCopies = 1
        });

        Assert.Null(result.Book);
        Assert.Contains("ISBN", result.Error);
        _books.Verify(x => x.Add(It.IsAny<Book>()), Times.Never);
    }

    [Fact]
    public void DeleteBook_WithActiveLoan_ReturnsFalse()
    {
        _books.Setup(x => x.GetAll()).Returns([new Book { Id = 2 }]);
        _loans.Setup(x => x.GetAll()).Returns([
            new Loan { BookId = 2, Status = "Activo" }
        ]);

        Assert.False(CreateService().DeleteBook(2));
        _books.Verify(x => x.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void CreateLoan_WhenNoCopiesAvailable_ReturnsError()
    {
        _books.Setup(x => x.GetById(3)).Returns(new Book { Id = 3, AvailableCopies = 0 });

        var result = CreateService().CreateLoan(new CreateLoanRequest
        {
            BookId = 3, Borrower = "Ana López",
            LoanDate = new DateOnly(2026, 11, 3),
            DueDate = new DateOnly(2026, 11, 10)
        });

        Assert.Null(result.Loan);
        Assert.Contains("No hay ejemplares", result.Error);
    }

    [Fact]
    public void CreateLoan_WhenDueDateIsNotAfterLoanDate_ReturnsError()
    {
        _books.Setup(x => x.GetById(1)).Returns(new Book { Id = 1, AvailableCopies = 1 });

        var result = CreateService().CreateLoan(new CreateLoanRequest
        {
            BookId = 1, Borrower = "Ana López",
            LoanDate = new DateOnly(2026, 11, 10),
            DueDate = new DateOnly(2026, 11, 10)
        });

        Assert.Null(result.Loan);
        Assert.Contains("posterior", result.Error);
    }
}
