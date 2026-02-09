using System;
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
    private readonly Texture2D _arrowLeft;
    private readonly Texture2D _arrowRight;

    public float MusicVolume { get; set; }
    public float SfxVolume { get; set; }

    private int _activeTab; // 0 = Controls, 1 = Sound

    private const int PanelWidth = 300;
    private const int PanelHeight = 400;
    private const int ButtonWidth = 150;
    private const int ButtonHeight = 32;
    private const int ButtonGap = 10;
    private const int Padding = 16;
    private const float ArrowScale = 3f;
    private const float VolumeStep = 0.10f;
    private const int TabPaddingH = 10;
    private const int TabPaddingV = 4;
    private const int TabGap = 6;
    private static readonly Color TextColor = Color.White;
    private static readonly Color DimTextColor = new(200, 200, 200);
    private static readonly Color ShadowColor = new(10, 10, 20);

    private static readonly string[] TabNames = { "Controls", "Sound" };

    private static readonly string[] InstrKeys =
        { "WASD / Arrows", "Shift", "Tab", "E", "1-7", "Esc", "F11" };
    private static readonly string[] InstrActions =
        { "Move", "Sprint", "Inventory", "Interact", "Tools", "Pause", "Fullscreen" };
    private static readonly string[] VolumeLabels = { "Music", "SFX" };

    private readonly Texture2D _controlsIcon;
    private readonly Texture2D _soundIcon;

    public PauseMenuUI(NineSliceBox panelBox, NineSliceBox buttonBox, SpriteFont font,
        Texture2D pixel, VirtualViewport viewport, float musicVolume, float sfxVolume,
        Texture2D arrowLeft, Texture2D arrowRight,
        Texture2D controlsIcon, Texture2D soundIcon)
    {
        ArgumentNullException.ThrowIfNull(arrowLeft);
        ArgumentNullException.ThrowIfNull(arrowRight);
        ArgumentNullException.ThrowIfNull(controlsIcon);
        ArgumentNullException.ThrowIfNull(soundIcon);

        _panelBox = panelBox;
        _buttonBox = buttonBox;
        _font = font;
        _pixel = pixel;
        _viewport = viewport;
        MusicVolume = musicVolume;
        SfxVolume = sfxVolume;
        _arrowLeft = arrowLeft;
        _arrowRight = arrowRight;
        _controlsIcon = controlsIcon;
        _soundIcon = soundIcon;
    }
    private (Rectangle tab0, Rectangle tab1, float contentY) ComputeTabLayout(float s, int panelX, int panelY, int panelW, int padding)
    {
        float titleScale = 1.4f * s;
        var titleSize = _font.MeasureString("PAUSED") * titleScale;
        float tabScale = 0.7f * s;
        float tabTextH = _font.MeasureString("A").Y * tabScale;
        int padH = (int)(TabPaddingH * s);
        int padV = (int)(TabPaddingV * s);
        int tabGap = (int)(TabGap * s);

        float tab0TextW = _font.MeasureString(TabNames[0]).X * tabScale;
        float tab1TextW = _font.MeasureString(TabNames[1]).X * tabScale;

        int tab0W = padH + (int)tab0TextW + padH;
        int tab1W = padH + (int)tab1TextW + padH;
        int tabH = padV + (int)tabTextH + padV;

        int totalW = tab0W + tabGap + tab1W;
        int tabStartX = panelX + (panelW - totalW) / 2;
        int tabY = (int)(panelY + padding + titleSize.Y + padding * 0.5f);

        var tab0Rect = new Rectangle(tabStartX, tabY, tab0W, tabH);
        var tab1Rect = new Rectangle(tabStartX + tab0W + tabGap, tabY, tab1W, tabH);
        float contentYOut = tabY + tabH + padding;

        return (tab0Rect, tab1Rect, contentYOut);
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

        bool clicked = mouse.LeftButton == ButtonState.Pressed &&
                       prevMouse.LeftButton == ButtonState.Released;

        if (clicked)
        {
            var (tab0Rect, tab1Rect, contentY) = ComputeTabLayout(s, panelX, panelY, panelW, padding);

            if (tab0Rect.Contains(virtualMouse))
                _activeTab = 0;
            else if (tab1Rect.Contains(virtualMouse))
                _activeTab = 1;

            // Sound tab arrow click detection
            if (_activeTab == 1)
            {
                float volScale = 0.65f * s;
                float arrowS = ArrowScale * s;
                int arrowW = (int)(_arrowLeft.Width * arrowS);
                int arrowH = (int)(_arrowLeft.Height * arrowS);
                float lineH = arrowH + 12 * s;

                for (int row = 0; row < 2; row++)
                {
                    float rowY = contentY + row * lineH;
                    string label = row == 0 ? "Music" : "SFX";
                    float labelW = _font.MeasureString(label).X * volScale;
                    string pctText = $"{(int)Math.Round((row == 0 ? MusicVolume : SfxVolume) * 100)}%";
                    float pctW = _font.MeasureString(pctText).X * volScale;
                    float gap2 = 8 * s;
                    float totalW = labelW + gap2 + arrowW + gap2 + pctW + gap2 + arrowW;
                    float startX = panelX + (panelW - totalW) / 2f;

                    var leftRect = new Rectangle((int)(startX + labelW + gap2), (int)rowY, arrowW, arrowH);
                    var rightRect = new Rectangle((int)(startX + labelW + gap2 + arrowW + gap2 + pctW + gap2), (int)rowY, arrowW, arrowH);

                    if (leftRect.Contains(virtualMouse))
                    {
                        if (row == 0)
                            MusicVolume = MathF.Round(MathHelper.Clamp(MusicVolume - VolumeStep, 0f, 1f), 2);
                        else
                            SfxVolume = MathF.Round(MathHelper.Clamp(SfxVolume - VolumeStep, 0f, 1f), 2);
                    }
                    else if (rightRect.Contains(virtualMouse))
                    {
                        if (row == 0)
                            MusicVolume = MathF.Round(MathHelper.Clamp(MusicVolume + VolumeStep, 0f, 1f), 2);
                        else
                            SfxVolume = MathF.Round(MathHelper.Clamp(SfxVolume + VolumeStep, 0f, 1f), 2);
                    }
                }
            }
        }

        // Button click detection
        int btnX = panelX + (panelW - btnW) / 2;
        int btnAreaBottom = panelY + panelH - padding;
        int resumeY = btnAreaBottom - btnH * 2 - gap;
        int exitY = btnAreaBottom - btnH;

        var resumeRect = new Rectangle(btnX, resumeY, btnW, btnH);
        var exitRect = new Rectangle(btnX, exitY, btnW, btnH);

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

        // Tabs (nine-slice box with text only)
        var (tab0Rect, tab1Rect, contentY) = ComputeTabLayout(s, panelX, panelY, panelW, padding);

        for (int i = 0; i < 2; i++)
        {
            var tabRect = i == 0 ? tab0Rect : tab1Rect;
            bool active = i == _activeTab;

            if (active)
                _panelBox.Draw(spriteBatch, tabRect);
            else
                _buttonBox.Draw(spriteBatch, tabRect);

            float tabScale = 0.7f * s;
            float tabTextH = _font.MeasureString("A").Y * tabScale;
            float tabTextW = _font.MeasureString(TabNames[i]).X * tabScale;
            float textX = tabRect.X + (tabRect.Width - tabTextW) / 2f;
            float textY = tabRect.Y + (tabRect.Height - tabTextH) / 2f;
            Color textColor = active ? TextColor : DimTextColor;

            spriteBatch.DrawString(_font, TabNames[i], new Vector2(textX + 1, textY + 1),
                ShadowColor, 0f, Vector2.Zero, tabScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, TabNames[i], new Vector2(textX, textY),
                textColor, 0f, Vector2.Zero, tabScale, SpriteEffects.None, 0f);
        }

        // Content area
        if (_activeTab == 0)
            DrawControlsTab(spriteBatch, panelX, panelW, contentY, padding, s);
        else
            DrawSoundTab(spriteBatch, panelX, panelW, contentY, padding, s);

        // Buttons (both tabs)
        int btnX = panelX + (panelW - btnW) / 2;
        int btnAreaBottom = panelY + panelH - padding;
        int resumeY = btnAreaBottom - btnH * 2 - gap;
        int exitY = btnAreaBottom - btnH;

        DrawButton(spriteBatch, new Rectangle(btnX, resumeY, btnW, btnH), "RESUME", virtualMouse, s);
        DrawButton(spriteBatch, new Rectangle(btnX, exitY, btnW, btnH), "EXIT TO MENU", virtualMouse, s);
    }

    private void DrawControlsTab(SpriteBatch spriteBatch, int panelX, int panelW,
        float contentY, int padding, float s)
    {
        float instrScale = 0.65f * s;
        float lineH = _font.MeasureString("A").Y * instrScale + 4 * s;
        float colLeft = panelX + padding * 2;
        float colRight = panelX + panelW - padding * 2;

        for (int i = 0; i < InstrKeys.Length; i++)
        {
            float y = contentY + i * lineH;

            spriteBatch.DrawString(_font, InstrKeys[i], new Vector2(colLeft, y),
                TextColor, 0f, Vector2.Zero, instrScale, SpriteEffects.None, 0f);

            float actionW = _font.MeasureString(InstrActions[i]).X * instrScale;
            spriteBatch.DrawString(_font, InstrActions[i], new Vector2(colRight - actionW, y),
                DimTextColor, 0f, Vector2.Zero, instrScale, SpriteEffects.None, 0f);
        }
    }

    private void DrawSoundTab(SpriteBatch spriteBatch, int panelX, int panelW,
        float contentY, int padding, float s)
    {
        float volScale = 0.65f * s;
        float arrowS = ArrowScale * s;
        int arrowW = (int)(_arrowLeft.Width * arrowS);
        int arrowH = (int)(_arrowLeft.Height * arrowS);
        float lineH = arrowH + 12 * s;
        float gap2 = 8 * s;

        for (int row = 0; row < 2; row++)
        {
            float rowY = contentY + row * lineH;
            float labelW = _font.MeasureString(VolumeLabels[row]).X * volScale;
            float volume = row == 0 ? MusicVolume : SfxVolume;
            string pctText = $"{(int)Math.Round(volume * 100)}%";
            float pctW = _font.MeasureString(pctText).X * volScale;
            float totalW = labelW + gap2 + arrowW + gap2 + pctW + gap2 + arrowW;
            float startX = panelX + (panelW - totalW) / 2f;

            // Label
            float labelY = rowY + (arrowH - _font.MeasureString("A").Y * volScale) / 2f;
            spriteBatch.DrawString(_font, VolumeLabels[row], new Vector2(startX, labelY),
                TextColor, 0f, Vector2.Zero, volScale, SpriteEffects.None, 0f);

            // Left arrow
            float leftX = startX + labelW + gap2;
            spriteBatch.Draw(_arrowLeft, new Rectangle((int)leftX, (int)rowY, arrowW, arrowH), Color.White);

            // Percentage text (centered in its slot)
            float pctX = leftX + arrowW + gap2;
            float pctY = rowY + (arrowH - _font.MeasureString("A").Y * volScale) / 2f;
            spriteBatch.DrawString(_font, pctText, new Vector2(pctX, pctY),
                TextColor, 0f, Vector2.Zero, volScale, SpriteEffects.None, 0f);

            // Right arrow
            float rightX = pctX + pctW + gap2;
            spriteBatch.Draw(_arrowRight, new Rectangle((int)rightX, (int)rowY, arrowW, arrowH), Color.White);
        }
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
