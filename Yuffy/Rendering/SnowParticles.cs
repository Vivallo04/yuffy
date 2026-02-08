using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Rendering;

public class SnowParticles
{
    private struct Flake
    {
        public Vector2 Position;
        public float SpeedY;
        public float DriftX;
        public int Size;
        public float Phase;
    }

    private readonly Texture2D _pixel;
    private readonly VirtualViewport _viewport;
    private readonly Random _rng;
    private readonly Flake[] _flakes;
    private float _time;

    private static readonly Color FlakeColor = new(170, 200, 255, 200);
    private const int Count = 200;

    public SnowParticles(Texture2D pixel, VirtualViewport viewport)
    {
        _pixel = pixel;
        _viewport = viewport;
        _rng = new Random();
        _flakes = new Flake[Count];

        for (int i = 0; i < Count; i++)
            _flakes[i] = SpawnFlake(randomY: true);
    }

    private Flake SpawnFlake(bool randomY)
    {
        return new Flake
        {
            Position = new Vector2(
                _rng.Next(_viewport.Width),
                randomY ? _rng.Next(_viewport.Height) : -_rng.Next(20)),
            SpeedY = 20f + (float)_rng.NextDouble() * 30f,
            DriftX = 5f + (float)_rng.NextDouble() * 15f,
            Size = _rng.Next(2, 6),
            Phase = (float)_rng.NextDouble() * MathF.PI * 2f
        };
    }

    public void Update(float deltaTime)
    {
        _time += deltaTime;

        for (int i = 0; i < _flakes.Length; i++)
        {
            _flakes[i].Position.Y += _flakes[i].SpeedY * deltaTime;
            _flakes[i].Position.X += _flakes[i].DriftX * MathF.Sin(_time + _flakes[i].Phase) * deltaTime;

            // Wrap X coordinate to keep flakes on screen
            _flakes[i].Position.X = (_flakes[i].Position.X % _viewport.Width + _viewport.Width) % _viewport.Width;

            if (_flakes[i].Position.Y > _viewport.Height)
                _flakes[i] = SpawnFlake(randomY: false);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
    {
        for (int i = 0; i < _flakes.Length; i++)
        {
            int x = (int)(_flakes[i].Position.X + cameraPosition.X);
            int y = (int)(_flakes[i].Position.Y + cameraPosition.Y);
            int s = _flakes[i].Size;
            spriteBatch.Draw(_pixel, new Rectangle(x, y, s, s), FlakeColor);
        }
    }
}
