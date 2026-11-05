using Biblioteca.Api.Models;

namespace Biblioteca.Api.Data;

public interface IBookUpdateRepository
{
    void Update(Book book);
}
