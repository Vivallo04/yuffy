using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Yuffy.Rendering;

namespace Yuffy.UI;

public class MiniGameUI
{
    private readonly SpriteFont _font;
    private readonly Texture2D _icon;
    private readonly Texture2D _disc;
    private readonly Texture2D _labelLeft;
    private readonly Texture2D _labelMiddle;
    private readonly Texture2D _labelRight;
    private readonly Texture2D _sandTimerIcon;
    private readonly VirtualViewport _viewport;

    public int Collected { get; set; }
    public int Target { get; set; } = 20;
    public float TimeRemaining { get; set; } = 180f;
    public bool IsActive { get; set; }
    public bool Won { get; set; }
    public bool Lost { get; set; }

    private float _resultTextTimer;

    private const float FontScale = 0.8f;
    private const int Scale = 2;
    private static readonly Color TextColor = Color.White;
    private static readonly Color TimerTextColor = Color.White;
    private static readonly Color TimerShadowColor = new(40, 25, 20);

    public MiniGameUI(SpriteFont font, Texture2D icon, Texture2D disc,
        Texture2D labelLeft, Texture2D labelMiddle, Texture2D labelRight,
        Texture2D sandTimerIcon, VirtualViewport viewport)
    {
        _font = font;
        _icon = icon;
        _disc = disc;
        _labelLeft = labelLeft;
        _labelMiddle = labelMiddle;
        _labelRight = labelRight;
        _sandTimerIcon = sandTimerIcon;
        _viewport = viewport;
    }

    public void Update(float deltaTime)
    {
        if (!IsActive) return;

        if (Won || Lost)
        {
            if (_resultTextTimer > 0)
                _resultTextTimer -= deltaTime;
            return;
        }

        TimeRemaining -= deltaTime;
        if (TimeRemaining <= 0)
        {
            TimeRemaining = 0;
            Lost = true;
            _resultTextTimer = 5f;
        }
    }

    public void TriggerWin()
    {
        Won = true;
        _resultTextTimer = 5f;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsActive) return;

        // Timer with wooden backing (disc + 3-slice bar, centered at top)
        DrawTimerLabel(spriteBatch);

        // Mushroom count label (disc + 3-slice bar + text)
        DrawCountLabel(spriteBatch);

        // Win/lose message centered (auto-hides after 5s)
        if (_resultTextTimer > 0)
        {
            if (Won)
                DrawCentered(spriteBatch, "YOU WIN!", TextColor);
            else if (Lost)
                DrawCentered(spriteBatch, "TIME'S UP!", TextColor);
        }
    }

    private void DrawTimerLabel(SpriteBatch spriteBatch)
    {
        int minutes = (int)TimeRemaining / 60;
        int seconds = (int)TimeRemaining % 60;
        string timerText = $"{minutes}:{seconds:D2}";
        Vector2 textSize = _font.MeasureString(timerText) * FontScale;

        int capW = _labelLeft.Width * Scale;
        int barH = _labelLeft.Height * Scale;
        int textPad = 8;
        int barInnerW = (int)textSize.X + textPad * 2;
        int barTotalW = capW * 2 + barInnerW;

        int discW = _disc.Width * Scale;
        int discH = _disc.Height * Scale;
        int discOverlap = discW / 2;

        int topMargin = 8;

        // Position: bar anchored top-right
        int rightMargin = 10;
        int barX = _viewport.Width - rightMargin - barTotalW;
        int barY = topMargin;

        // Disc overlaps left edge of bar
        int discX = barX - discW + discOverlap;
        int discY = barY + (barH - discH) / 2;

        // Draw 3-slice label bar
        spriteBatch.Draw(_labelLeft, new Rectangle(barX, barY, capW, barH), Color.White);
        spriteBatch.Draw(_labelMiddle, new Rectangle(barX + capW, barY, barInnerW, barH), Color.White);
        spriteBatch.Draw(_labelRight, new Rectangle(barX + capW + barInnerW, barY, capW, barH), Color.White);

        // Draw text centered in visible bar area (right of disc overlap)
        int visibleLeft = barX + discOverlap;
        int visibleRight = barX + barTotalW;
        float textX = visibleLeft + (visibleRight - visibleLeft - textSize.X) / 2f;
        float textY = barY + (barH - textSize.Y) / 2f + 5;

        // Shadow
        spriteBatch.DrawString(_font, timerText, new Vector2(textX + 1, textY + 1), TimerShadowColor,
            0f, Vector2.Zero, FontScale, SpriteEffects.None, 0f);
        // Main text
        spriteBatch.DrawString(_font, timerText, new Vector2(textX, textY), TimerTextColor,
            0f, Vector2.Zero, FontScale, SpriteEffects.None, 0f);

        // Draw disc (in front of bar)
        spriteBatch.Draw(_disc, new Rectangle(discX, discY, discW, discH), Color.White);

        // Draw sandtimer icon centered on disc
        int iconScale = 3;
        int iconW = _sandTimerIcon.Width * iconScale;
        int iconH = _sandTimerIcon.Height * iconScale;
        int iconX = discX + (discW - iconW) / 2;
        int iconY = discY + (discH - iconH) / 2;
        spriteBatch.Draw(_sandTimerIcon, new Rectangle(iconX, iconY, iconW, iconH), Color.White);
    }

    private void DrawCountLabel(SpriteBatch spriteBatch)
    {
        string countText = $"{Collected}/{Target}";
        Vector2 textSize = _font.MeasureString(countText) * FontScale;

        int capW = _labelLeft.Width * Scale;
        int barH = _labelLeft.Height * Scale;
        int textPad = 8;
        int barInnerW = (int)textSize.X + textPad * 2;
        int barTotalW = capW * 2 + barInnerW;

        int discW = _disc.Width * Scale;
        int discH = _disc.Height * Scale;
        int discOverlap = discW / 2;

        // Position: below hearts (top-left)
        int barX = 10 + discW - discOverlap;
        int barY = 64;

        // Disc overlaps left edge of bar, centered vertically on bar
        int discX = barX - discW + discOverlap;
        int discY = barY + (barH - discH) / 2;

        // Draw 3-slice label bar (behind disc)
        spriteBatch.Draw(_labelLeft, new Rectangle(barX, barY, capW, barH), Color.White);
        spriteBatch.Draw(_labelMiddle, new Rectangle(barX + capW, barY, barInnerW, barH), Color.White);
        spriteBatch.Draw(_labelRight, new Rectangle(barX + capW + barInnerW, barY, capW, barH), Color.White);

        // Draw text centered in visible bar area (right of disc overlap)
        int visibleLeft = barX + discOverlap;
        int visibleRight = barX + barTotalW;
        float textX = visibleLeft + (visibleRight - visibleLeft - textSize.X) / 2f;
        float textY = barY + (barH - textSize.Y) / 2f + 5;
        spriteBatch.DrawString(_font, countText, new Vector2(textX, textY), TextColor,
            0f, Vector2.Zero, FontScale, SpriteEffects.None, 0f);

        // Draw disc (in front of bar)
        spriteBatch.Draw(_disc, new Rectangle(discX, discY, discW, discH), Color.White);

        // Draw mushroom icon centered on disc (smaller than disc)
        int iconScale = 3;
        int iconW = _icon.Width * iconScale;
        int iconH = _icon.Height * iconScale;
        int iconX = discX + (discW - iconW) / 2;
        int iconY = discY + (discH - iconH) / 2;
        spriteBatch.Draw(_icon, new Rectangle(iconX, iconY, iconW, iconH), Color.White);
    }

    private void DrawCentered(SpriteBatch spriteBatch, string text, Color color)
    {
        float bigScale = 3.0f;
        Vector2 size = _font.MeasureString(text) * bigScale;
        Vector2 pos = new(_viewport.Width / 2f - size.X / 2f, _viewport.Height / 2f - size.Y / 2f);
        spriteBatch.DrawString(_font, text, pos, color,
            0f, Vector2.Zero, bigScale, SpriteEffects.None, 0f);
    }
}
