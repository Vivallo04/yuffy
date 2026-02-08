using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Yuffy.Rendering;

namespace Yuffy.UI;

public enum MenuAction { None, Play, Exit }

public class MainMenuUI
{
    private readonly NineSliceBox _panelBox;
    private readonly NineSliceBox _buttonBox;
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly VirtualViewport _viewport;

    private const int ButtonWidth = 180;
    private const int ButtonHeight = 44;
    private const int ButtonGap = 14;
    private const float TitleScale = 2.0f;
    private static readonly Color BgColor = new(20, 20, 35);
    private static readonly Color TextColor = Color.White;
    private static readonly Color ShadowColor = new(10, 10, 20);

    public MainMenuUI(NineSliceBox panelBox, NineSliceBox buttonBox, SpriteFont font,
        Texture2D pixel, VirtualViewport viewport)
    {
        _panelBox = panelBox;
        _buttonBox = buttonBox;
        _font = font;
        _pixel = pixel;
        _viewport = viewport;
    }

    public MenuAction Update(Vector2 virtualMouse, MouseState mouse, MouseState prevMouse)
    {
        float s = _viewport.OverlayScale;
        int btnW = (int)(ButtonWidth * s);
        int btnH = (int)(ButtonHeight * s);
        int gap = (int)(ButtonGap * s);

        int centerX = _viewport.Width / 2;
        int startY = _viewport.Height / 2;

        var playRect = new Rectangle(centerX - btnW / 2, startY, btnW, btnH);
        var exitRect = new Rectangle(centerX - btnW / 2, startY + btnH + gap, btnW, btnH);

        bool clicked = mouse.LeftButton == ButtonState.Pressed &&
                       prevMouse.LeftButton == ButtonState.Released;

        if (clicked && playRect.Contains(virtualMouse))
            return MenuAction.Play;
        if (clicked && exitRect.Contains(virtualMouse))
            return MenuAction.Exit;

        return MenuAction.None;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 virtualMouse)
    {
        float s = _viewport.OverlayScale;
        int btnW = (int)(ButtonWidth * s);
        int btnH = (int)(ButtonHeight * s);
        int gap = (int)(ButtonGap * s);

        // Dark background
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _viewport.Width, _viewport.Height), BgColor);

        // Title
        float titleScale = TitleScale * s;
        var titleSize = _font.MeasureString("YUFFY") * titleScale;
        var titlePos = new Vector2(
            (_viewport.Width - titleSize.X) / 2f,
            _viewport.Height * 0.25f - titleSize.Y / 2f);

        // Shadow
        spriteBatch.DrawString(_font, "YUFFY", titlePos + new Vector2(2 * s, 2 * s),
            ShadowColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
        // Title text
        spriteBatch.DrawString(_font, "YUFFY", titlePos,
            TextColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

        // Buttons
        int centerX = _viewport.Width / 2;
        int startY = _viewport.Height / 2;

        var playRect = new Rectangle(centerX - btnW / 2, startY, btnW, btnH);
        var exitRect = new Rectangle(centerX - btnW / 2, startY + btnH + gap, btnW, btnH);

        DrawButton(spriteBatch, playRect, "PLAY", virtualMouse, s);
        DrawButton(spriteBatch, exitRect, "EXIT", virtualMouse, s);
    }

    private void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string text,
        Vector2 virtualMouse, float scale)
    {
        bool hovered = rect.Contains(virtualMouse);

        if (hovered)
            _panelBox.Draw(spriteBatch, rect);
        else
            _buttonBox.Draw(spriteBatch, rect);

        float fontScale = 1.0f * scale;
        var textSize = _font.MeasureString(text) * fontScale;
        var textPos = new Vector2(
            rect.X + (rect.Width - textSize.X) / 2f,
            rect.Y + (rect.Height - textSize.Y) / 2f + 3 * scale);

        // Shadow
        spriteBatch.DrawString(_font, text, textPos + new Vector2(1, 1),
            ShadowColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_font, text, textPos,
            TextColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
    }
}
