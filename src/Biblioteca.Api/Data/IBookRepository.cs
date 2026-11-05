using Biblioteca.Api.Models;

namespace Biblioteca.Api.Data;

public interface IBookRepository
{
    IReadOnlyCollection<Book> GetAll();
    Book? GetById(int id);
    Book Add(Book book);
    bool Delete(int id);
}
