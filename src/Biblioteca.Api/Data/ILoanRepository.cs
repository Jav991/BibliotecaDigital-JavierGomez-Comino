using Biblioteca.Api.Models;

namespace Biblioteca.Api.Data;

public interface ILoanRepository
{
    IReadOnlyCollection<Loan> GetAll();
    Loan? GetById(int id);
    Loan Add(Loan loan);
    void Update(Loan loan);
}
