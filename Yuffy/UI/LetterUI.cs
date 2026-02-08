using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Yuffy.Rendering;

namespace Yuffy.UI;

public class LetterUI
{
    private readonly NineSliceBox _box;
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly VirtualViewport _viewport;

    public string Text { get; set; } = "";
    public bool IsOpen { get; set; }

    private float _scrollOffset;
    private float _totalContentHeight;
    private List<string> _wrappedLines;
    private bool _linesCached;
    private int _previousScrollValue;
    private int _lastViewportWidth;
    private float _cachedLineHeight;
    private bool _firstFrameAfterOpen;

    private const int BasePanelWidth = 750;
    private const int BasePanelHeight = 430;
    private const int Padding = 24;
    private const float FontScale = 1.0f;
    private const float ScrollSpeed = 120f;
    private static readonly Color TextColor = Color.White;
    public LetterUI(NineSliceBox box, SpriteFont font, Texture2D pixel, VirtualViewport viewport)
    {
        _box = box ?? throw new ArgumentNullException(nameof(box));
        _font = font ?? throw new ArgumentNullException(nameof(font));
        _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
        _viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));
        _wrappedLines = new List<string>();
        _lastViewportWidth = viewport.Width;
    }

    public void Update(float deltaTime, KeyboardState keyState, MouseState mouseState)
    {
        if (!IsOpen) return;

        // Invalidate line cache if viewport changed
        if (_viewport.Width != _lastViewportWidth)
        {
            _linesCached = false;
            _lastViewportWidth = _viewport.Width;
        }

        float s = _viewport.Height / (float)VirtualViewport.BaseHeight;
        int panelW = (int)(BasePanelWidth * s);
        int panelH = (int)(BasePanelHeight * s);
        int padding = (int)(Padding * s);
        float fontScale = FontScale * s;

        // Ensure wrapped lines and content height are up-to-date
        if (!_linesCached)
        {
            float maxWidth = panelW - padding * 2;
            _wrappedLines = WrapText(Text, maxWidth, fontScale);
            _cachedLineHeight = _font.MeasureString("A").Y * fontScale + 2 * s;
            _totalContentHeight = _wrappedLines.Count * _cachedLineHeight;
            _linesCached = true;
        }

        float maxScroll = Math.Max(0, _totalContentHeight - (panelH - padding * 2));

        if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
            _scrollOffset += ScrollSpeed * deltaTime;
        if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
            _scrollOffset -= ScrollSpeed * deltaTime;

        // Mouse scroll wheel (scroll up = positive delta, should decrease offset)
        if (_firstFrameAfterOpen)
        {
            _previousScrollValue = mouseState.ScrollWheelValue;
            _firstFrameAfterOpen = false;
        }
        else
        {
            int scrollDelta = mouseState.ScrollWheelValue - _previousScrollValue;
            _previousScrollValue = mouseState.ScrollWheelValue;
            _scrollOffset -= scrollDelta * 0.5f;
        }

        _scrollOffset = MathHelper.Clamp(_scrollOffset, 0, maxScroll);
    }

    public void Open()
    {
        IsOpen = true;
        _scrollOffset = 0;
        _linesCached = false;
        _firstFrameAfterOpen = true;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen) return;

        float s = _viewport.Height / (float)VirtualViewport.BaseHeight;
        int panelW = (int)(BasePanelWidth * s);
        int panelH = (int)(BasePanelHeight * s);
        int padding = (int)(Padding * s);
        float fontScale = FontScale * s;

        // Cache wrapped lines
        if (!_linesCached)
        {
            float maxWidth = panelW - padding * 2;
            _wrappedLines = WrapText(Text, maxWidth, fontScale);
            _cachedLineHeight = _font.MeasureString("A").Y * fontScale + 2 * s;
            _totalContentHeight = _wrappedLines.Count * _cachedLineHeight;
            _linesCached = true;
        }

        // Dark overlay
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _viewport.Width, _viewport.Height),
            new Color(0, 0, 0, 150));

        // White panel centered
        int panelX = (_viewport.Width - panelW) / 2;
        int panelY = (_viewport.Height - panelH) / 2;
        _box.Draw(spriteBatch, new Rectangle(panelX, panelY, panelW, panelH));

        // Visible content area
        float contentTop = panelY + padding;
        float contentBottom = panelY + panelH - padding;

        for (int i = 0; i < _wrappedLines.Count; i++)
        {
            float lineY = contentTop + i * _cachedLineHeight - _scrollOffset;

            // Skip lines outside visible area
            if (lineY + _cachedLineHeight < contentTop) continue;
            if (lineY > contentBottom) break;

            spriteBatch.DrawString(_font, _wrappedLines[i],
                new Vector2(panelX + padding, lineY),
                TextColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        }
    }

    private List<string> WrapText(string text, float maxWidth, float fontScale)
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
                // Handle words longer than maxWidth
                if (_font.MeasureString(word).X * fontScale > maxWidth)
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine);
                        currentLine = "";
                    }
                    // Add long word as-is (or implement character-level breaking)
                    lines.Add(word);
                    continue;
                }

                string testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                float testWidth = _font.MeasureString(testLine).X * fontScale;

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
