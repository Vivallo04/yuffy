using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Yuffy.Graphics;

namespace Yuffy;

public class LetterUI
{
    private readonly NineSliceBox _box;
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;

    public string Text { get; set; } = "";
    public bool IsOpen { get; set; }

    private float _scrollOffset;
    private float _totalContentHeight;
    private List<string> _wrappedLines;
    private bool _linesCached;
    private int _previousScrollValue;

    private const int ScreenWidth = 960;
    private const int ScreenHeight = 540;
    private const int PanelWidth = 750;
    private const int PanelHeight = 430;
    private const int Padding = 24;
    private const float FontScale = 1.0f;
    private const float ScrollSpeed = 120f;
    private static readonly Color TextColor = new(74, 49, 45);

    public LetterUI(NineSliceBox box, SpriteFont font, Texture2D pixel)
    {
        _box = box;
        _font = font;
        _pixel = pixel;
        _wrappedLines = new List<string>();
    }

    public void Update(float deltaTime, KeyboardState keyState, MouseState mouseState)
    {
        if (!IsOpen) return;

        float maxScroll = Math.Max(0, _totalContentHeight - (PanelHeight - Padding * 2));

        if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
            _scrollOffset += ScrollSpeed * deltaTime;
        if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
            _scrollOffset -= ScrollSpeed * deltaTime;

        // Mouse scroll wheel (scroll up = positive delta, should decrease offset)
        int scrollDelta = mouseState.ScrollWheelValue - _previousScrollValue;
        _previousScrollValue = mouseState.ScrollWheelValue;
        _scrollOffset -= scrollDelta * 0.5f;

        _scrollOffset = MathHelper.Clamp(_scrollOffset, 0, maxScroll);
    }

    public void Open()
    {
        IsOpen = true;
        _scrollOffset = 0;
        _linesCached = false;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen) return;

        // Cache wrapped lines
        if (!_linesCached)
        {
            float maxWidth = PanelWidth - Padding * 2;
            _wrappedLines = WrapText(Text, maxWidth);
            float lineHeight = _font.MeasureString("A").Y * FontScale + 2;
            _totalContentHeight = _wrappedLines.Count * lineHeight;
            _linesCached = true;
        }

        // Dark overlay
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, ScreenWidth, ScreenHeight),
            new Color(0, 0, 0, 150));

        // White panel centered
        int panelX = (ScreenWidth - PanelWidth) / 2;
        int panelY = (ScreenHeight - PanelHeight) / 2;
        _box.Draw(spriteBatch, new Rectangle(panelX, panelY, PanelWidth, PanelHeight));

        // Visible content area
        float contentTop = panelY + Padding;
        float contentBottom = panelY + PanelHeight - Padding;
        float lineHeight2 = _font.MeasureString("A").Y * FontScale + 2;

        for (int i = 0; i < _wrappedLines.Count; i++)
        {
            float lineY = contentTop + i * lineHeight2 - _scrollOffset;

            // Skip lines outside visible area
            if (lineY + lineHeight2 < contentTop) continue;
            if (lineY > contentBottom) break;

            spriteBatch.DrawString(_font, _wrappedLines[i],
                new Vector2(panelX + Padding, lineY),
                TextColor, 0f, Vector2.Zero, FontScale, SpriteEffects.None, 0f);
        }
    }

    private List<string> WrapText(string text, float maxWidth)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text)) return lines;

        string[] paragraphs = text.Split('\n');

        foreach (string paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                lines.Add("");
                continue;
            }

            string[] words = paragraph.Split(' ');
            string currentLine = "";

            foreach (string word in words)
            {
                string testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                float testWidth = _font.MeasureString(testLine).X * FontScale;

                if (testWidth > maxWidth && currentLine.Length > 0)
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine);
        }

        return lines;
    }
}
