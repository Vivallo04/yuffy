using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy;

public class ToolbarUI
{
    private readonly Texture2D _pixel;
    private readonly Dictionary<ItemType, Texture2D> _itemTextures;
    private readonly Inventory _inventory;

    public int SelectedSlot { get; set; }

    private const int SlotSize = 43;
    private const int SlotGap = 2;
    private const int Border = 2;
    private const int SlotCount = 7;
    private const int TotalWidth = SlotCount * SlotSize + (SlotCount - 1) * SlotGap;
    private const int StartX = (960 - TotalWidth) / 2;
    private const int StartY = 540 - SlotSize - 8;
    private const float IconScale = 2.4f;

    private static readonly Color BgColor = new(0, 0, 0, 150);
    private static readonly Color BorderColor = new(80, 80, 80, 200);
    private static readonly Color SelectedBorderColor = Color.White;

    public ToolbarUI(Texture2D pixel, Dictionary<ItemType, Texture2D> itemTextures, Inventory inventory)
    {
        _pixel = pixel;
        _itemTextures = itemTextures;
        _inventory = inventory;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            int x = StartX + i * (SlotSize + SlotGap);
            int y = StartY;

            // Background fill
            spriteBatch.Draw(_pixel, new Rectangle(x, y, SlotSize, SlotSize), BgColor);

            // Border (4 thin rects)
            Color borderCol = i == SelectedSlot ? SelectedBorderColor : BorderColor;
            spriteBatch.Draw(_pixel, new Rectangle(x, y, SlotSize, Border), borderCol);                   // top
            spriteBatch.Draw(_pixel, new Rectangle(x, y + SlotSize - Border, SlotSize, Border), borderCol); // bottom
            spriteBatch.Draw(_pixel, new Rectangle(x, y, Border, SlotSize), borderCol);                    // left
            spriteBatch.Draw(_pixel, new Rectangle(x + SlotSize - Border, y, Border, SlotSize), borderCol); // right

            // Item icon
            var slot = _inventory.Slots[i, 0];
            if (!slot.IsEmpty && _itemTextures.TryGetValue(slot.Item, out var tex))
            {
                int iconSize = (int)(tex.Width * IconScale);
                int offset = (SlotSize - iconSize) / 2;
                spriteBatch.Draw(tex,
                    new Rectangle(x + offset, y + offset, iconSize, iconSize),
                    Color.White);
            }
        }
    }
}
