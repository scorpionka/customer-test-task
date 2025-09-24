using System.ComponentModel.DataAnnotations;

namespace WebApiTestApp.ApiModels;

public sealed record Product
{
    public Guid Id { get; init; } = Guid.NewGuid();
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;
    [StringLength(500)]
    public string Description { get; init; } = string.Empty;
    [Range(0, 9999999)]
    public decimal Price { get; init; }
    [Required]
    [StringLength(100)]
    public string Category { get; init; } = string.Empty;
}
