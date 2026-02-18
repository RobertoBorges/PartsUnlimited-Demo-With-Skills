using System.ComponentModel.DataAnnotations;

namespace PartsUnlimited.Models;

public class Category
{
    public int CategoryId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public List<Product> Products { get; set; } = [];
}
