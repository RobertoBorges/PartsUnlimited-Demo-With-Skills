using System.ComponentModel.DataAnnotations;

namespace PartsUnlimited.Models;

public class Order
{
    [ScaffoldColumn(false)]
    public int OrderId { get; set; }

    [ScaffoldColumn(false)]
    public DateTime OrderDate { get; set; }

    [ScaffoldColumn(false)]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Name is required"), StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required"), StringLength(70)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required"), StringLength(40)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required"), StringLength(40)]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "Postal Code is required"), StringLength(10)]
    public string PostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required"), StringLength(40)]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required"), StringLength(24)]
    [DataType(DataType.PhoneNumber)]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [RegularExpression(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,4}",
        ErrorMessage = "Email is is not valid.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [ScaffoldColumn(false)]
    public decimal Total { get; set; }

    public List<OrderDetail> OrderDetails { get; set; } = [];
}
