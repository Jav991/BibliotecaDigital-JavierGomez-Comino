using Biblioteca.Api.Contracts;
using Biblioteca.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers;

[ApiController]
[Route("api/loans")]
public sealed class LoansController(LibraryService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(service.GetLoans());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var loan = service.GetLoan(id);
        return loan is null ? NotFound(new { message = "Préstamo no encontrado." }) : Ok(loan);
    }

    [HttpPost]
    public IActionResult Create(CreateLoanRequest request)
    {
        var result = service.CreateLoan(request);
        return result.Error is not null
            ? BadRequest(new { message = result.Error })
            : CreatedAtAction(nameof(GetById), new { id = result.Loan!.Id }, result.Loan);
    }

    [HttpPut("{id:int}/return")]
    public IActionResult Return(int id)
    {
        var result = service.ReturnLoan(id);
        return result.Error switch
        {
            "No existe el préstamo indicado." => NotFound(new { message = result.Error }),
            not null => Conflict(new { message = result.Error }),
            _ => Ok(result.Loan)
        };
    }
}
