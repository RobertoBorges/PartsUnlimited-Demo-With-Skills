using PartsUnlimited.Models;

namespace PartsUnlimited.Utils;

public interface IShippingTaxCalculator
{
    Task<int> CalculateShipping(Order order);
    Task<int> CalculateTax(Order order);
    Task<OrderCostSummary> CalculateOrderCost(Order order);
}

/// <summary>
/// Shipping = itemsCount × $5.00. Tax = 5% of (subtotal + shipping).
/// Business rules preserved verbatim from legacy DefaultShippingTaxCalculator.
/// </summary>
public class DefaultShippingTaxCalculator : IShippingTaxCalculator
{
    private const decimal TaxRate = 0.05m;
    private const decimal ShippingRatePerItem = 5.00m;

    public Task<int> CalculateShipping(Order order)
    {
        var shipping = (int)(order.OrderDetails.Sum(od => od.Count) * ShippingRatePerItem);
        return Task.FromResult(shipping);
    }

    public Task<int> CalculateTax(Order order)
    {
        var subTotal = order.OrderDetails.Sum(od => od.Count * od.UnitPrice);
        var shipping = order.OrderDetails.Sum(od => od.Count) * ShippingRatePerItem;
        var tax = (int)Math.Round((subTotal + shipping) * TaxRate, 0, MidpointRounding.AwayFromZero);
        return Task.FromResult(tax);
    }

    public async Task<OrderCostSummary> CalculateOrderCost(Order order)
    {
        var subTotal = order.OrderDetails.Sum(od => od.Count * od.UnitPrice);
        var shipping = await CalculateShipping(order);
        var tax = await CalculateTax(order);
        var total = subTotal + shipping + tax;

        return new OrderCostSummary
        {
            CartSubTotal = subTotal.ToString("C"),
            CartShipping = shipping.ToString("C"),
            CartTax = tax.ToString("C"),
            CartTotal = total.ToString("C")
        };
    }
}
