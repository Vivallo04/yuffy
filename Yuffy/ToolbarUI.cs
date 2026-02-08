using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy;

public class ToolbarUI
{
    private readonly Texture2D _pixel;
    private readonly Dictionary<ItemType, Texture2D> _itemTextures;
    private readonly Inventory _inventory;
    private readonly SpriteFont _font;

    public int SelectedSlot { get; set; }

    private const int SlotCount = 7;
    private const int SlotSize = 43;
    private const int SlotGap = 2;
    private const int Border = 2;
    private const float IconScale = 2.4f;
    private const float NumberScale = 0.35f;
    private const int ScreenWidth = 960;
    private const int ScreenHeight = 540;

    private static readonly Color BgColor = new(0, 0, 0, 150);
    private static readonly Color BorderColor = new(80, 80, 80, 200);
    private static readonly Color SelectedBorderColor = Color.White;
    private static readonly Color NumberColor = Color.White;
    private static readonly Color NumberShadowColor = new(40, 25, 20);

    // Calculated layout
    private readonly int _barY;
    private readonly int _slotsStartX;
    private readonly int _slotsStartY;

    public ToolbarUI(Texture2D pixel, Dictionary<ItemType, Texture2D> itemTextures, Inventory inventory,
        SpriteFont font)
    {
        _pixel = pixel;
        _itemTextures = itemTextures;
        _inventory = inventory;
        _font = font;

        int barWidth = SlotCount * SlotSize + (SlotCount - 1) * SlotGap;
        int barX = (ScreenWidth - barWidth) / 2;
        _barY = ScreenHeight - SlotSize - 8;
        _slotsStartX = barX;
        _slotsStartY = _barY;
    }

    public int GetSlotCenterX(int slot)
    {
        return _slotsStartX + slot * (SlotSize + SlotGap) + SlotSize / 2;
    }

    public int GetBarTopY() => _barY;

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            int x = _slotsStartX + i * (SlotSize + SlotGap);
            int y = _slotsStartY;

            // Background fill
            spriteBatch.Draw(_pixel, new Rectangle(x, y, SlotSize, SlotSize), BgColor);

            // Border (4 thin rects)
            Color borderCol = i == SelectedSlot ? SelectedBorderColor : BorderColor;
            spriteBatch.Draw(_pixel, new Rectangle(x, y, SlotSize, Border), borderCol);
            spriteBatch.Draw(_pixel, new Rectangle(x, y + SlotSize - Border, SlotSize, Border), borderCol);
            spriteBatch.Draw(_pixel, new Rectangle(x, y, Border, SlotSize), borderCol);
            spriteBatch.Draw(_pixel, new Rectangle(x + SlotSize - Border, y, Border, SlotSize), borderCol);

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

            // Slot number (bottom-right corner) with shadow
            string num = (i + 1).ToString();
            Vector2 numSize = _font.MeasureString(num) * NumberScale;
            float numX = x + SlotSize - numSize.X - 2;
            float numY = y + SlotSize - numSize.Y;

            spriteBatch.DrawString(_font, num, new Vector2(numX + 1, numY + 1), NumberShadowColor,
                0f, Vector2.Zero, NumberScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, num, new Vector2(numX, numY), NumberColor,
                0f, Vector2.Zero, NumberScale, SpriteEffects.None, 0f);
        }
    }
}
