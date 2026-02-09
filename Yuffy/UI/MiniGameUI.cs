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
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(icon);
        ArgumentNullException.ThrowIfNull(disc);
        ArgumentNullException.ThrowIfNull(labelLeft);
        ArgumentNullException.ThrowIfNull(labelMiddle);
        ArgumentNullException.ThrowIfNull(labelRight);
        ArgumentNullException.ThrowIfNull(sandTimerIcon);
        ArgumentNullException.ThrowIfNull(viewport);

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
        if (!IsActive || Lost) return;
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

        var textSize = _font.MeasureString(timerText) * FontScale;
        int capW = _labelLeft.Width * Scale;
        int barInnerW = (int)textSize.X + TextPad * 2;
        int barTotalW = capW * 2 + barInnerW;

        int barX = _viewport.Width - 10 - barTotalW;
        int barY = 8;

        DrawLabelBar(spriteBatch, timerText, _sandTimerIcon, barX, barY, drawShadow: true);
    }

    private void DrawCountLabel(SpriteBatch spriteBatch)
    {
        int discW = _disc.Width * Scale;
        int discOverlap = discW / 2;

        int barX = 10 + discW - discOverlap;
        int barY = 64;

        DrawLabelBar(spriteBatch, $"{Collected}/{Target}", _icon, barX, barY);
    }

    private const int TextPad = 8;
    private const int IconScale = 3;

    private void DrawLabelBar(SpriteBatch spriteBatch, string text, Texture2D icon,
        int barX, int barY, bool drawShadow = false)
    {
        var textSize = _font.MeasureString(text) * FontScale;
        int capW = _labelLeft.Width * Scale;
        int barH = _labelLeft.Height * Scale;
        int barInnerW = (int)textSize.X + TextPad * 2;
        int barTotalW = capW * 2 + barInnerW;

        int discW = _disc.Width * Scale;
        int discH = _disc.Height * Scale;
        int discOverlap = discW / 2;

        int discX = barX - discW + discOverlap;
        int discY = barY + (barH - discH) / 2;

        // 3-slice bar
        spriteBatch.Draw(_labelLeft, new Rectangle(barX, barY, capW, barH), Color.White);
        spriteBatch.Draw(_labelMiddle, new Rectangle(barX + capW, barY, barInnerW, barH), Color.White);
        spriteBatch.Draw(_labelRight, new Rectangle(barX + capW + barInnerW, barY, capW, barH), Color.White);

        // Text centered in visible area (right of disc overlap)
        int visibleLeft = barX + discOverlap;
        int visibleRight = barX + barTotalW;
        float textX = visibleLeft + (visibleRight - visibleLeft - textSize.X) / 2f;
        float textY = barY + (barH - textSize.Y) / 2f + 5;

        if (drawShadow)
            spriteBatch.DrawString(_font, text, new Vector2(textX + 1, textY + 1), TimerShadowColor,
                0f, Vector2.Zero, FontScale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_font, text, new Vector2(textX, textY), TextColor,
            0f, Vector2.Zero, FontScale, SpriteEffects.None, 0f);

        // Disc and icon
        spriteBatch.Draw(_disc, new Rectangle(discX, discY, discW, discH), Color.White);
        int iconW = icon.Width * IconScale;
        int iconH = icon.Height * IconScale;
        int iconX = discX + (discW - iconW) / 2;
        int iconY = discY + (discH - iconH) / 2;
        spriteBatch.Draw(icon, new Rectangle(iconX, iconY, iconW, iconH), Color.White);
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
