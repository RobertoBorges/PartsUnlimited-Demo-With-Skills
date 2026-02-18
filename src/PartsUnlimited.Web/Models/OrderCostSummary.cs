namespace PartsUnlimited.Models;

public class OrderCostSummary
{
    public string CartSubTotal { get; set; } = string.Empty;
    public string CartShipping { get; set; } = string.Empty;
    public string CartTax { get; set; } = string.Empty;
    public string CartTotal { get; set; } = string.Empty;
}
