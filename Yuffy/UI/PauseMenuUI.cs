using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Yuffy.Rendering;

namespace Yuffy.UI;

public enum PauseAction { None, Resume, ExitToMenu }

public class PauseMenuUI
{
    private readonly NineSliceBox _panelBox;
    private readonly NineSliceBox _buttonBox;
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly VirtualViewport _viewport;

    private const int PanelWidth = 300;
    private const int PanelHeight = 340;
    private const int ButtonWidth = 150;
    private const int ButtonHeight = 32;
    private const int ButtonGap = 10;
    private const int Padding = 16;
    private static readonly Color TextColor = Color.White;
    private static readonly Color DimTextColor = new(200, 200, 200);
    private static readonly Color ShadowColor = new(10, 10, 20);

    private static readonly string[] InstrKeys =
        { "WASD / Arrows", "Shift", "Tab", "E", "1-7", "Esc" };
    private static readonly string[] InstrActions =
        { "Move", "Sprint", "Inventory", "Read Letter", "Toolbar Slot", "Pause" };

    public PauseMenuUI(NineSliceBox panelBox, NineSliceBox buttonBox, SpriteFont font,
        Texture2D pixel, VirtualViewport viewport)
    {
        _panelBox = panelBox;
        _buttonBox = buttonBox;
        _font = font;
        _pixel = pixel;
        _viewport = viewport;
    }

    public PauseAction Update(Vector2 virtualMouse, MouseState mouse, MouseState prevMouse)
    {
        float s = _viewport.Height / (float)VirtualViewport.BaseHeight;
        int panelW = (int)(PanelWidth * s);
        int panelH = (int)(PanelHeight * s);
        int btnW = (int)(ButtonWidth * s);
        int btnH = (int)(ButtonHeight * s);
        int gap = (int)(ButtonGap * s);
        int padding = (int)(Padding * s);

        int panelX = (_viewport.Width - panelW) / 2;
        int panelY = (_viewport.Height - panelH) / 2;

        int btnX = panelX + (panelW - btnW) / 2;
        int btnAreaBottom = panelY + panelH - padding;
        int resumeY = btnAreaBottom - btnH * 2 - gap;
        int exitY = btnAreaBottom - btnH;

        var resumeRect = new Rectangle(btnX, resumeY, btnW, btnH);
        var exitRect = new Rectangle(btnX, exitY, btnW, btnH);

        bool clicked = mouse.LeftButton == ButtonState.Pressed &&
                       prevMouse.LeftButton == ButtonState.Released;

        if (clicked && resumeRect.Contains(virtualMouse))
            return PauseAction.Resume;
        if (clicked && exitRect.Contains(virtualMouse))
            return PauseAction.ExitToMenu;

        return PauseAction.None;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 virtualMouse)
    {
        float s = _viewport.Height / (float)VirtualViewport.BaseHeight;
        int panelW = (int)(PanelWidth * s);
        int panelH = (int)(PanelHeight * s);
        int btnW = (int)(ButtonWidth * s);
        int btnH = (int)(ButtonHeight * s);
        int gap = (int)(ButtonGap * s);
        int padding = (int)(Padding * s);

        // Dark overlay
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _viewport.Width, _viewport.Height),
            new Color(0, 0, 0, 180));

        // Panel
        int panelX = (_viewport.Width - panelW) / 2;
        int panelY = (_viewport.Height - panelH) / 2;
        _panelBox.Draw(spriteBatch, new Rectangle(panelX, panelY, panelW, panelH));

        // "PAUSED" title
        float titleScale = 1.4f * s;
        var titleSize = _font.MeasureString("PAUSED") * titleScale;
        var titlePos = new Vector2(
            panelX + (panelW - titleSize.X) / 2f,
            panelY + padding);

        spriteBatch.DrawString(_font, "PAUSED", titlePos + new Vector2(1, 1),
            ShadowColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_font, "PAUSED", titlePos,
            TextColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

        // Instructions (two-column: key left, action right)
        float instrScale = 0.65f * s;
        float lineH = _font.MeasureString("A").Y * instrScale + 4 * s;
        float instrY = titlePos.Y + titleSize.Y + padding;
        float colLeft = panelX + padding * 2;
        float colRight = panelX + panelW - padding * 2;

        for (int i = 0; i < InstrKeys.Length; i++)
        {
            float y = instrY + i * lineH;

            // Key (left-aligned)
            spriteBatch.DrawString(_font, InstrKeys[i], new Vector2(colLeft, y),
                TextColor, 0f, Vector2.Zero, instrScale, SpriteEffects.None, 0f);

            // Action (right-aligned)
            float actionW = _font.MeasureString(InstrActions[i]).X * instrScale;
            spriteBatch.DrawString(_font, InstrActions[i], new Vector2(colRight - actionW, y),
                DimTextColor, 0f, Vector2.Zero, instrScale, SpriteEffects.None, 0f);
        }

        // Buttons
        int btnX = panelX + (panelW - btnW) / 2;
        int btnAreaBottom = panelY + panelH - padding;
        int resumeY = btnAreaBottom - btnH * 2 - gap;
        int exitY = btnAreaBottom - btnH;

        var resumeRect = new Rectangle(btnX, resumeY, btnW, btnH);
        var exitRect = new Rectangle(btnX, exitY, btnW, btnH);

        DrawButton(spriteBatch, resumeRect, "RESUME", virtualMouse, s);
        DrawButton(spriteBatch, exitRect, "EXIT TO MENU", virtualMouse, s);
    }

    private void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string text,
        Vector2 virtualMouse, float scale)
    {
        bool hovered = rect.Contains(virtualMouse);

        if (hovered)
            _panelBox.Draw(spriteBatch, rect);
        else
            _buttonBox.Draw(spriteBatch, rect);

        float fontScale = 0.65f * scale;
        var textSize = _font.MeasureString(text) * fontScale;
        var textPos = new Vector2(
            rect.X + (rect.Width - textSize.X) / 2f,
            rect.Y + (rect.Height - textSize.Y) / 2f + 2 * scale);

        spriteBatch.DrawString(_font, text, textPos + new Vector2(1, 1),
            ShadowColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_font, text, textPos,
            TextColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
    }
}
