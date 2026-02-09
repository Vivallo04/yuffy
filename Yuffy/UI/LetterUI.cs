using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Yuffy.Rendering;

namespace Yuffy.UI;

public enum LetterAction { None, Accept, Reject }

public class LetterUI
{
    private readonly NineSliceBox _box;
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly VirtualViewport _viewport;
    private readonly Texture2D _confirmTex;
    private readonly Texture2D _cancelTex;

    private string _text = "";
    public string Text
    {
        get => _text;
        set { _text = SanitizeForFont(value ?? ""); _linesCached = false; }
    }
    public bool IsOpen { get; set; }

    private float _scrollOffset;
    private float _totalContentHeight;
    private List<string> _wrappedLines;
    private bool _linesCached;
    private int _previousScrollValue;
    private int _lastViewportWidth;
    private float _cachedLineHeight;
    private bool _firstFrameAfterOpen;
    private MouseState _prevMouse;

    private const int BasePanelWidth = 750;
    private const int BasePanelHeight = 430;
    private const int Padding = 24;
    private const float FontScale = 1.0f;
    private const float ScrollSpeed = 120f;
    private const float IconScale = 3f;
    private const float ButtonGap = 40f;
    private static readonly Color TextColor = Color.White;

    public LetterUI(NineSliceBox box, SpriteFont font, Texture2D pixel,
                    VirtualViewport viewport, Texture2D confirmTex, Texture2D cancelTex)
    {
        _box = box ?? throw new ArgumentNullException(nameof(box));
        _font = font ?? throw new ArgumentNullException(nameof(font));
        _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
        _viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));
        _confirmTex = confirmTex ?? throw new ArgumentNullException(nameof(confirmTex));
        _cancelTex = cancelTex ?? throw new ArgumentNullException(nameof(cancelTex));
        _wrappedLines = new List<string>();
        _lastViewportWidth = viewport.Width;
    }

    public LetterAction Update(float deltaTime, KeyboardState keyState, MouseState mouseState, Vector2 virtualMouse)
    {
        if (!IsOpen) { _prevMouse = mouseState; return LetterAction.None; }

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
        float iconScale = IconScale * s;
        float buttonGap = ButtonGap * s;

        // Ensure wrapped lines and content height are up-to-date
        if (!_linesCached)
        {
            float maxWidth = panelW - padding * 2;
            _wrappedLines = WrapText(Text, maxWidth, fontScale);
            _cachedLineHeight = _font.MeasureString("A").Y * fontScale + 2 * s;
            float textHeight = _wrappedLines.Count * _cachedLineHeight;
            float iconH = _confirmTex.Height * iconScale;
            _totalContentHeight = textHeight + buttonGap + iconH;
            _linesCached = true;
        }

        float maxScroll = Math.Max(0, _totalContentHeight - (panelH - padding * 2));

        if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
            _scrollOffset += ScrollSpeed * deltaTime;
        if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
            _scrollOffset -= ScrollSpeed * deltaTime;

        // Mouse scroll wheel
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

        // Button click detection
        LetterAction action = LetterAction.None;
        bool clicked = mouseState.LeftButton == ButtonState.Pressed &&
                       _prevMouse.LeftButton == ButtonState.Released;

        if (clicked)
        {
            int panelX = (_viewport.Width - panelW) / 2;
            int panelY = (_viewport.Height - panelH) / 2;
            float contentTop = panelY + padding;
            float contentBottom = panelY + panelH - padding;
            float textHeight = _wrappedLines.Count * _cachedLineHeight;
            float iconW = _confirmTex.Width * iconScale;
            float iconH = _confirmTex.Height * iconScale;

            float buttonsY = contentTop + textHeight + buttonGap - _scrollOffset;

            // Only process clicks if buttons are visible
            if (buttonsY + iconH > contentTop && buttonsY < contentBottom)
            {
                // Accept and Reject label widths
                float acceptLabelW = _font.MeasureString("Accept").X * fontScale;
                float rejectLabelW = _font.MeasureString("Reject").X * fontScale;
                float labelGap = 6 * s;

                float acceptTotalW = iconW + labelGap + acceptLabelW;
                float rejectTotalW = iconW + labelGap + rejectLabelW;
                float spacing = 30 * s;
                float totalW = acceptTotalW + spacing + rejectTotalW;
                float startX = panelX + (panelW - totalW) / 2f;

                var rejectRect = new Rectangle((int)startX, (int)buttonsY, (int)rejectTotalW, (int)iconH);
                var acceptRect = new Rectangle((int)(startX + rejectTotalW + spacing), (int)buttonsY, (int)acceptTotalW, (int)iconH);

                if (rejectRect.Contains(virtualMouse))
                    action = LetterAction.Reject;
                else if (acceptRect.Contains(virtualMouse))
                    action = LetterAction.Accept;
            }
        }

        _prevMouse = mouseState;
        return action;
    }

    public void Open()
    {
        IsOpen = true;
        _scrollOffset = 0;
        _linesCached = false;
        _firstFrameAfterOpen = true;
    }

    private static readonly RasterizerState ScissorRasterizer = new() { ScissorTestEnable = true };

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen) return;

        float s = _viewport.Height / (float)VirtualViewport.BaseHeight;
        int panelW = (int)(BasePanelWidth * s);
        int panelH = (int)(BasePanelHeight * s);
        int padding = (int)(Padding * s);
        float fontScale = FontScale * s;
        float iconScale = IconScale * s;
        float buttonGap = ButtonGap * s;

        // Cache wrapped lines
        if (!_linesCached)
        {
            float maxWidth = panelW - padding * 2;
            _wrappedLines = WrapText(Text, maxWidth, fontScale);
            _cachedLineHeight = _font.MeasureString("A").Y * fontScale + 2 * s;
            float textHeight = _wrappedLines.Count * _cachedLineHeight;
            float iconH = _confirmTex.Height * iconScale;
            _totalContentHeight = textHeight + buttonGap + iconH;
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
        int contentTop = panelY + padding;
        int contentBottom = panelY + panelH - padding;

        // Clip to content area using scissor rectangle
        var device = spriteBatch.GraphicsDevice;
        var oldScissor = device.ScissorRectangle;
        spriteBatch.End();

        device.ScissorRectangle = new Rectangle(panelX, contentTop, panelW, contentBottom - contentTop);
        spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp,
            rasterizerState: ScissorRasterizer);

        // Draw text lines
        for (int i = 0; i < _wrappedLines.Count; i++)
        {
            float lineY = contentTop + i * _cachedLineHeight - _scrollOffset;

            if (lineY + _cachedLineHeight < contentTop) continue;
            if (lineY > contentBottom) break;

            spriteBatch.DrawString(_font, _wrappedLines[i],
                new Vector2(panelX + padding, lineY),
                TextColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        }

        // Draw accept/reject buttons at bottom of scrollable content
        float textHeight2 = _wrappedLines.Count * _cachedLineHeight;
        float iconW = _confirmTex.Width * iconScale;
        float iconH2 = _confirmTex.Height * iconScale;
        float buttonsY = contentTop + textHeight2 + buttonGap - _scrollOffset;

        if (buttonsY < contentBottom && buttonsY + iconH2 > contentTop)
        {
            float acceptLabelW = _font.MeasureString("Accept").X * fontScale;
            float rejectLabelW = _font.MeasureString("Reject").X * fontScale;
            float labelGap = 6 * s;

            float acceptTotalW = iconW + labelGap + acceptLabelW;
            float rejectTotalW = iconW + labelGap + rejectLabelW;
            float spacing = 30 * s;
            float totalW = acceptTotalW + spacing + rejectTotalW;
            float startX = panelX + (panelW - totalW) / 2f;

            // Reject button (left): icon + label
            spriteBatch.Draw(_cancelTex, new Vector2(startX, buttonsY), null,
                Color.White, 0f, Vector2.Zero, iconScale, SpriteEffects.None, 0f);
            float labelY = buttonsY + (iconH2 - _font.MeasureString("A").Y * fontScale) / 2f;
            spriteBatch.DrawString(_font, "Reject",
                new Vector2(startX + iconW + labelGap, labelY),
                Color.Salmon, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            // Accept button (right): icon + label
            float acceptX = startX + rejectTotalW + spacing;
            spriteBatch.Draw(_confirmTex, new Vector2(acceptX, buttonsY), null,
                Color.White, 0f, Vector2.Zero, iconScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, "Accept",
                new Vector2(acceptX + iconW + labelGap, labelY),
                Color.LightGreen, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        }

        // Restore original batch state
        spriteBatch.End();
        device.ScissorRectangle = oldScissor;
        spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
    }

    private static string SanitizeForFont(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            if ((c >= 32 && c <= 126) || c == '\n')
                sb.Append(c);
            else if (c == '\u2014' || c == '\u2013') // em/en dash
                sb.Append('-');
            else if (c == '\u2018' || c == '\u2019') // smart single quotes
                sb.Append('\'');
            else if (c == '\u201C' || c == '\u201D') // smart double quotes
                sb.Append('"');
        }
        return sb.ToString();
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
