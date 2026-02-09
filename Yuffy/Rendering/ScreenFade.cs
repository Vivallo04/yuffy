using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Rendering;

public enum TransitionMode { Fade, Wipe }

public class ScreenFade
{
    public bool IsActive { get; private set; }

    private float _timer;
    private float _duration;
    private float _halfDuration;
    private Action _onMidpoint;
    private bool _midpointFired;
    private TransitionMode _mode;

    // Wipe: horizontal rows with jagged left-to-right edge
    private const int RowHeight = 16;
    private const float EdgeJitter = 0.06f; // ±6% of screen width per row
    private float[] _rowJitter;
    private int _rowCount;
    private static readonly Random Rng = new();

    public void Start(float duration, Action onMidpoint, TransitionMode mode = TransitionMode.Fade, int viewportHeight = 540)
    {
        _duration = duration;
        _halfDuration = duration / 2f;
        _timer = 0f;
        _onMidpoint = onMidpoint;
        _midpointFired = false;
        _mode = mode;
        IsActive = true;

        if (mode == TransitionMode.Wipe)
        {
            _rowCount = (viewportHeight + RowHeight - 1) / RowHeight;
            _rowJitter = new float[_rowCount];
            for (int i = 0; i < _rowCount; i++)
                _rowJitter[i] = ((float)Rng.NextDouble() * 2f - 1f) * EdgeJitter;
        }
    }

    public void Update(float deltaTime)
    {
        if (!IsActive) return;

        _timer += deltaTime;

        if (!_midpointFired && _timer >= _halfDuration)
        {
            _midpointFired = true;
            _onMidpoint?.Invoke();
        }

        if (_timer >= _duration)
            IsActive = false;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, int viewportWidth, int viewportHeight)
    {
        if (!IsActive) return;

        if (_mode == TransitionMode.Fade)
            DrawFade(spriteBatch, pixel, viewportWidth, viewportHeight);
        else
            DrawWipe(spriteBatch, pixel, viewportWidth, viewportHeight);
    }

    private void DrawFade(SpriteBatch spriteBatch, Texture2D pixel, int viewportWidth, int viewportHeight)
    {
        float alpha;
        if (_timer < _halfDuration)
            alpha = _timer / _halfDuration;
        else
            alpha = 1f - (_timer - _halfDuration) / _halfDuration;

        alpha = MathHelper.Clamp(alpha, 0f, 1f);

        spriteBatch.Draw(pixel, new Rectangle(0, 0, viewportWidth, viewportHeight),
            new Color(0, 0, 0, (int)(alpha * 255)));
    }

    private void DrawWipe(SpriteBatch spriteBatch, Texture2D pixel, int viewportWidth, int viewportHeight)
    {
        bool covering = _timer < _halfDuration;
        float phaseTime = covering ? _timer : _timer - _halfDuration;

        // sweepT: 0→1 across each half, with ease-in-out for smooth acceleration
        float sweepT = MathHelper.Clamp(phaseTime / _halfDuration, 0f, 1f);
        // Ease-in-out (smoothstep) for natural sweep momentum
        sweepT = sweepT * sweepT * (3f - 2f * sweepT);

        // Overshoot range so edge clears fully off-screen despite jitter
        float sweepRange = 1f + EdgeJitter * 2f;
        float sweepPos = -EdgeJitter + sweepT * sweepRange;

        int rows = (viewportHeight + RowHeight - 1) / RowHeight;
        int count = Math.Min(rows, _rowCount);

        for (int i = 0; i < count; i++)
        {
            float edgeNorm = sweepPos + _rowJitter[i];
            int edgeX = (int)(edgeNorm * viewportWidth);
            edgeX = Math.Clamp(edgeX, 0, viewportWidth);

            int y = i * RowHeight;
            int h = Math.Min(RowHeight, viewportHeight - y);

            if (covering)
            {
                // Black fills from left to edge
                if (edgeX > 0)
                    spriteBatch.Draw(pixel, new Rectangle(0, y, edgeX, h), Color.Black);
            }
            else
            {
                // Black remains from edge to right, sweeping away
                int remaining = viewportWidth - edgeX;
                if (remaining > 0)
                    spriteBatch.Draw(pixel, new Rectangle(edgeX, y, remaining, h), Color.Black);
            }
        }
    }
}
