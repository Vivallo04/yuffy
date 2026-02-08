using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Yuffy.Graphics;

namespace Yuffy;

public class InventoryUI
{
    private readonly NineSliceBox _box;
    private readonly Texture2D _itemDiscTexture;
    private readonly Dictionary<ItemType, Texture2D> _itemTextures;
    private readonly Inventory _inventory;

    private const int SlotSize = 40;
    private const int SlotGap = 4;
    private const int Padding = 16;
    private const int PanelWidth = Inventory.Columns * SlotSize + (Inventory.Columns - 1) * SlotGap + Padding * 2;
    private const int PanelHeight = Inventory.Rows * SlotSize + (Inventory.Rows - 1) * SlotGap + Padding * 2;
    private const int PanelX = (960 - PanelWidth) / 2;
    private const int PanelY = (540 - PanelHeight) / 2;
    private const float IconScale = 2f;

    public InventoryUI(NineSliceBox box, Texture2D itemDisc,
        Dictionary<ItemType, Texture2D> itemTextures, Inventory inventory)
    {
        _box = box;
        _itemDiscTexture = itemDisc;
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

                // Draw disc background centered in slot
                int discSize = (int)(_itemDiscTexture.Width * IconScale);
                int discOffset = (SlotSize - discSize) / 2;
                spriteBatch.Draw(_itemDiscTexture,
                    new Rectangle(slotX + discOffset, slotY + discOffset, discSize, discSize),
                    Color.White);

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
}
