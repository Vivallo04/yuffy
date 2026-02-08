namespace Yuffy;

public class Inventory
{
    public const int Columns = 8;
    public const int Rows = 3;

    public ItemSlot[,] Slots { get; } = new ItemSlot[Columns, Rows];

    public Inventory()
    {
        for (int x = 0; x < Columns; x++)
            for (int y = 0; y < Rows; y++)
                Slots[x, y] = new ItemSlot();
    }

    public void SetSlot(int col, int row, ItemType type, int count)
    {
        Slots[col, row].Item = type;
        Slots[col, row].Count = count;
    }

    public void AddItem(ItemType type, int count = 1)
    {
        // Try stacking into existing slot
        for (int y = 0; y < Rows; y++)
            for (int x = 0; x < Columns; x++)
                if (Slots[x, y].Item == type)
                {
                    Slots[x, y].Count += count;
                    return;
                }

        // Find first empty slot
        for (int y = 0; y < Rows; y++)
            for (int x = 0; x < Columns; x++)
                if (Slots[x, y].IsEmpty)
                {
                    Slots[x, y].Item = type;
                    Slots[x, y].Count = count;
                    return;
                }
    }
}
