using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace PartsUnlimited.Models;

public class Product
{
    [ScaffoldColumn(false)]
    public int ProductId { get; set; }

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public decimal SalePrice { get; set; }

    [Required, StringLength(1024)]
    public string ProductArtUrl { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    [Required, StringLength(10)]
    public string SkuNumber { get; set; } = string.Empty;

    public int RecommendationId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual List<OrderDetail> OrderDetails { get; set; } = [];

    [ScaffoldColumn(false)]
    [Required]
    public DateTime Created { get; set; }

    [Required]
    [Display(Name = "Product Details")]
    public string ProductDetails { get; set; } = string.Empty;

    public int Inventory { get; set; }

    public int LeadTime { get; set; }

    [NotMapped]
    public Dictionary<string, string> ProductDetailList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ProductDetails))
                return new Dictionary<string, string>();

            return JsonSerializer.Deserialize<Dictionary<string, string>>(ProductDetails)
                   ?? new Dictionary<string, string>();
        }
    }
}
