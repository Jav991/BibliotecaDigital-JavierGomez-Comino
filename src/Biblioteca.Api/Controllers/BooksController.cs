using Biblioteca.Api.Contracts;
using Biblioteca.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers;

[ApiController]
[Route("api/books")]
public sealed class BooksController(LibraryService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(service.GetBooks());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var book = service.GetBook(id);
        return book is null ? NotFound(new { message = "Libro no encontrado." }) : Ok(book);
    }

    [HttpPost]
    public IActionResult Create(CreateBookRequest request)
    {
        var result = service.CreateBook(request);
        return result.Error is not null
            ? Conflict(new { message = result.Error })
            : CreatedAtAction(nameof(GetById), new { id = result.Book!.Id }, result.Book);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (service.GetBook(id) is null)
            return NotFound(new { message = "Libro no encontrado." });

        if (!service.DeleteBook(id))
            return Conflict(new { message = "No se puede eliminar un libro con préstamos activos." });

        return NoContent();
    }
}
