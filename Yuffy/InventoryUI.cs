using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Yuffy.Graphics;

namespace Yuffy;

public class InventoryUI
{
    private readonly NineSliceBox _box;
    private readonly Texture2D _pixel;
    private readonly Dictionary<ItemType, Texture2D> _itemTextures;
    private readonly Inventory _inventory;

    private const int SlotSize = 40;
    private const int SlotGap = 4;
    private const int Padding = 16;
    private const int Bevel = 2;
    private const int PanelWidth = Inventory.Columns * SlotSize + (Inventory.Columns - 1) * SlotGap + Padding * 2;
    private const int PanelHeight = Inventory.Rows * SlotSize + (Inventory.Rows - 1) * SlotGap + Padding * 2;
    private const int PanelX = (960 - PanelWidth) / 2;
    private const int PanelY = (540 - PanelHeight) / 2;
    private const float IconScale = 2f;

    // Beveled wooden square colors
    private static readonly Color SlotBorder = new(85, 55, 25);
    private static readonly Color SlotFill = new(139, 90, 43);
    private static readonly Color SlotHighlight = new(180, 130, 70);

    public InventoryUI(NineSliceBox box, Texture2D pixel,
        Dictionary<ItemType, Texture2D> itemTextures, Inventory inventory)
    {
        _box = box;
        _pixel = pixel;
        _itemTextures = itemTextures;
        _inventory = inventory;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _box.Draw(spriteBatch, new Rectangle(PanelX, PanelY, PanelWidth, PanelHeight));

        for (int row = 0; row < Inventory.Rows; row++)
        {
            for (int col = 0; col < Inventory.Columns; col++)
            {
                int slotX = PanelX + Padding + col * (SlotSize + SlotGap);
                int slotY = PanelY + Padding + row * (SlotSize + SlotGap);

                // Beveled wooden square background
                DrawBeveledSquare(spriteBatch, slotX, slotY, SlotSize);

                // Draw item icon if slot is not empty
                var slot = _inventory.Slots[col, row];
                if (!slot.IsEmpty && _itemTextures.TryGetValue(slot.Item, out var tex))
                {
                    int iconSize = (int)(tex.Width * IconScale);
                    int iconOffset = (SlotSize - iconSize) / 2;
                    spriteBatch.Draw(tex,
                        new Rectangle(slotX + iconOffset, slotY + iconOffset, iconSize, iconSize),
                        Color.White);
                }
            }
        }
    }

    private void DrawBeveledSquare(SpriteBatch sb, int x, int y, int size)
    {
        // 1. Outer border (dark shadow)
        sb.Draw(_pixel, new Rectangle(x, y, size, size), SlotBorder);
        // 2. Inner fill
        sb.Draw(_pixel, new Rectangle(x + Bevel, y + Bevel, size - Bevel * 2, size - Bevel * 2), SlotFill);
        // 3. Top highlight bevel
        sb.Draw(_pixel, new Rectangle(x, y, size, Bevel), SlotHighlight);
        // 4. Left highlight bevel
        sb.Draw(_pixel, new Rectangle(x, y, Bevel, size), SlotHighlight);
    }
}
