using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Api.Contracts;

public sealed class CreateBookRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Title { get; init; } = "";

    [Required, StringLength(100, MinimumLength = 2)]
    public string Author { get; init; } = "";

    [Required, RegularExpression(@"^[0-9Xx-]{10,17}$")]
    public string Isbn { get; init; } = "";

    [Range(1450, 2100)]
    public int PublishedYear { get; init; }

    [Range(0, 1000)]
    public int AvailableCopies { get; init; }
}
