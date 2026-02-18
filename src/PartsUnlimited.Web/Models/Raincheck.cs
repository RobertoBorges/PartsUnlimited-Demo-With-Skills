using System.ComponentModel.DataAnnotations;

namespace PartsUnlimited.Models;

public class Raincheck
{
    public int RaincheckId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public int StoreId { get; set; }
    public int Quantity { get; set; }
    public double SalePrice { get; set; }

    public virtual Store? Store { get; set; }
    public virtual Product? Product { get; set; }
}
