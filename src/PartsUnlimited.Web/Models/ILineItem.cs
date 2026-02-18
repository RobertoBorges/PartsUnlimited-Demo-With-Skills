namespace PartsUnlimited.Models;

public interface ILineItem
{
    int Count { get; set; }
    decimal UnitPrice { get; set; }
}
