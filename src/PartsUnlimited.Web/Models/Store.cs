using System.ComponentModel.DataAnnotations;

namespace PartsUnlimited.Models;

public class Store
{
    public int StoreId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
}
