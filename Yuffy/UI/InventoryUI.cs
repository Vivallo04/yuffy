using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Yuffy.Gameplay;
using Yuffy.Rendering;

namespace Yuffy.UI;

public class InventoryUI
{
    private readonly NineSliceBox _box;
    private readonly Texture2D _pixel;
    private readonly Dictionary<ItemType, Texture2D> _itemTextures;
    private readonly Inventory _inventory;
    private readonly VirtualViewport _viewport;

    private const int BaseSlotSize = 40;
    private const int BaseSlotGap = 4;
    private const int BasePadding = 16;
    private const int Bevel = 2;
    private const int BasePanelWidth = Inventory.Columns * BaseSlotSize + (Inventory.Columns - 1) * BaseSlotGap + BasePadding * 2;
    private const int BasePanelHeight = Inventory.Rows * BaseSlotSize + (Inventory.Rows - 1) * BaseSlotGap + BasePadding * 2;
    private const float BaseIconScale = 2f;

    // Beveled wooden square colors
    private static readonly Color SlotBorder = new(85, 55, 25);
    private static readonly Color SlotFill = new(139, 90, 43);
    private static readonly Color SlotHighlight = new(180, 130, 70);

    public InventoryUI(NineSliceBox box, Texture2D pixel,
        Dictionary<ItemType, Texture2D> itemTextures, Inventory inventory, VirtualViewport viewport)
    {
        _box = box;
        _pixel = pixel;
        _itemTextures = itemTextures;
        _inventory = inventory;
        _viewport = viewport;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        float s = _viewport.Height / (float)VirtualViewport.BaseHeight;
        int panelW = (int)(BasePanelWidth * s);
        int panelH = (int)(BasePanelHeight * s);
        int panelX = (_viewport.Width - panelW) / 2;
        int panelY = (_viewport.Height - panelH) / 2;

        int slotSize = (int)(BaseSlotSize * s);
        int slotGap = (int)(BaseSlotGap * s);
        int padding = (int)(BasePadding * s);
        float iconScale = BaseIconScale * s;

        _box.Draw(spriteBatch, new Rectangle(panelX, panelY, panelW, panelH));

        for (int row = 0; row < Inventory.Rows; row++)
        {
            for (int col = 0; col < Inventory.Columns; col++)
            {
                int slotX = panelX + padding + col * (slotSize + slotGap);
                int slotY = panelY + padding + row * (slotSize + slotGap);

                // Beveled wooden square background
                int bevel = Math.Max(1, (int)(Bevel * s));
                DrawBeveledSquare(spriteBatch, slotX, slotY, slotSize, bevel);

                // Draw item icon if slot is not empty
                var slot = _inventory.Slots[col, row];
                if (!slot.IsEmpty && _itemTextures.TryGetValue(slot.Item, out var tex))
                {
                    int iconSize = (int)(tex.Width * iconScale);
                    int iconOffset = (slotSize - iconSize) / 2;
                    spriteBatch.Draw(tex,
                        new Rectangle(slotX + iconOffset, slotY + iconOffset, iconSize, iconSize),
                        Color.White);
                }
            }
        }
    }

    private void DrawBeveledSquare(SpriteBatch sb, int x, int y, int size, int bevel)
    {
        // 1. Outer border (dark shadow)
        sb.Draw(_pixel, new Rectangle(x, y, size, size), SlotBorder);
        // 2. Inner fill
        sb.Draw(_pixel, new Rectangle(x + bevel, y + bevel, size - bevel * 2, size - bevel * 2), SlotFill);
        // 3. Top highlight bevel
        sb.Draw(_pixel, new Rectangle(x, y, size, bevel), SlotHighlight);
        // 4. Left highlight bevel
        sb.Draw(_pixel, new Rectangle(x, y, bevel, size), SlotHighlight);
    }
}
