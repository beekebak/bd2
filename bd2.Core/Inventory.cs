namespace bd2.Core;

public class Inventory(string name, int inventoryId, int totalAmount)
{
    public int InventoryId { get; private set; } = inventoryId;
    public int TotalAmount { get; private set; } = totalAmount;
    public string Name { get; private set; } = name;
}