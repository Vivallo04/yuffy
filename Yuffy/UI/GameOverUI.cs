using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Yuffy.Rendering;

namespace Yuffy.UI;

public enum GameOverAction { None, BackToMenu }

public class GameOverUI
{
    private readonly NineSliceBox _panelBox;
    private readonly NineSliceBox _buttonBox;
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly VirtualViewport _viewport;

    private const int PanelWidth = 300;
    private const int PanelHeight = 160;
    private const int ButtonWidth = 180;
    private const int ButtonHeight = 32;
    private const int Padding = 16;
    private static readonly Color TextColor = Color.White;
    private static readonly Color ShadowColor = new(10, 10, 20);

    public GameOverUI(NineSliceBox panelBox, NineSliceBox buttonBox, SpriteFont font,
        Texture2D pixel, VirtualViewport viewport)
    {
        _panelBox = panelBox;
        _buttonBox = buttonBox;
        _font = font;
        _pixel = pixel;
        _viewport = viewport;
    }

    public GameOverAction Update(Vector2 virtualMouse, MouseState mouse, MouseState prevMouse)
    {
        float s = _viewport.Height / (float)VirtualViewport.BaseHeight;
        int panelW = (int)(PanelWidth * s);
        int panelH = (int)(PanelHeight * s);
        int btnW = (int)(ButtonWidth * s);
        int btnH = (int)(ButtonHeight * s);
        int padding = (int)(Padding * s);

        int panelX = (_viewport.Width - panelW) / 2;
        int panelY = (_viewport.Height - panelH) / 2;

        int btnX = panelX + (panelW - btnW) / 2;
        int btnY = panelY + panelH - padding - btnH;

        var btnRect = new Rectangle(btnX, btnY, btnW, btnH);

        bool clicked = mouse.LeftButton == ButtonState.Pressed &&
                       prevMouse.LeftButton == ButtonState.Released;

        if (clicked && btnRect.Contains(virtualMouse))
            return GameOverAction.BackToMenu;

        return GameOverAction.None;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 virtualMouse)
    {
        float s = _viewport.Height / (float)VirtualViewport.BaseHeight;
        int panelW = (int)(PanelWidth * s);
        int panelH = (int)(PanelHeight * s);
        int btnW = (int)(ButtonWidth * s);
        int btnH = (int)(ButtonHeight * s);
        int padding = (int)(Padding * s);

        // Dark overlay
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _viewport.Width, _viewport.Height),
            new Color(0, 0, 0, 200));

        // Panel
        int panelX = (_viewport.Width - panelW) / 2;
        int panelY = (_viewport.Height - panelH) / 2;
        _panelBox.Draw(spriteBatch, new Rectangle(panelX, panelY, panelW, panelH));

        // "GAME OVER" title
        float titleScale = 1.4f * s;
        var titleSize = _font.MeasureString("GAME OVER") * titleScale;
        var titlePos = new Vector2(
            panelX + (panelW - titleSize.X) / 2f,
            panelY + padding);

        spriteBatch.DrawString(_font, "GAME OVER", titlePos + new Vector2(1, 1),
            ShadowColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_font, "GAME OVER", titlePos,
            Color.Salmon, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

        // Button
        int btnX = panelX + (panelW - btnW) / 2;
        int btnY = panelY + panelH - padding - btnH;
        var btnRect = new Rectangle(btnX, btnY, btnW, btnH);

        bool hovered = btnRect.Contains(virtualMouse);
        if (hovered)
            _panelBox.Draw(spriteBatch, btnRect);
        else
            _buttonBox.Draw(spriteBatch, btnRect);

        float fontScale = 0.65f * s;
        var textSize = _font.MeasureString("BACK TO MENU") * fontScale;
        var textPos = new Vector2(
            btnRect.X + (btnRect.Width - textSize.X) / 2f,
            btnRect.Y + (btnRect.Height - textSize.Y) / 2f + 2 * s);

        spriteBatch.DrawString(_font, "BACK TO MENU", textPos + new Vector2(1, 1),
            ShadowColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_font, "BACK TO MENU", textPos,
            TextColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
    }
}
