using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Rendering;

public class HeartParticles
{
    private struct Heart
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Life;
        public float MaxLife;
        public float Scale;
        public float Rotation;
        public float RotationSpeed;
    }

    private readonly Texture2D _texture;
    private readonly VirtualViewport _viewport;
    private readonly Random _rng = new();
    private readonly Heart[] _hearts;
    private int _activeCount;

    private const int MaxHearts = 80;
    private const float Gravity = 120f;

    public bool IsActive => _activeCount > 0;

    public HeartParticles(Texture2D texture, VirtualViewport viewport)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentNullException.ThrowIfNull(viewport);
        _texture = texture;
        _viewport = viewport;
        _hearts = new Heart[MaxHearts];
    }

    public void Trigger()
    {
        float cx = _viewport.Width / 2f;
        float cy = _viewport.Height / 2f;

        _activeCount = MaxHearts;
        for (int i = 0; i < MaxHearts; i++)
        {
            float angle = (float)(_rng.NextDouble() * MathHelper.TwoPi);
            float speed = 150f + (float)_rng.NextDouble() * 250f;

            _hearts[i] = new Heart
            {
                Position = new Vector2(cx, cy),
                Velocity = new Vector2(
                    MathF.Cos(angle) * speed,
                    MathF.Sin(angle) * speed - 180f),
                Life = 1.8f + (float)_rng.NextDouble() * 0.8f,
                Scale = 1.5f + (float)_rng.NextDouble() * 2f,
                Rotation = (float)(_rng.NextDouble() * MathHelper.TwoPi),
                RotationSpeed = -3f + (float)_rng.NextDouble() * 6f
            };
            _hearts[i].MaxLife = _hearts[i].Life;
        }
    }

    public void Update(float deltaTime)
    {
        if (_activeCount <= 0) return;

        int alive = 0;
        for (int i = 0; i < _activeCount; i++)
        {
            _hearts[i].Life -= deltaTime;
            if (_hearts[i].Life <= 0) continue;

            _hearts[i].Velocity.Y += Gravity * deltaTime;
            _hearts[i].Position += _hearts[i].Velocity * deltaTime;
            _hearts[i].Rotation += _hearts[i].RotationSpeed * deltaTime;

            if (alive != i)
                _hearts[alive] = _hearts[i];
            alive++;
        }
        _activeCount = alive;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_activeCount <= 0) return;

        Vector2 origin = new(_texture.Width / 2f, _texture.Height / 2f);

        for (int i = 0; i < _activeCount; i++)
        {
            float alpha = MathHelper.Clamp(_hearts[i].Life / _hearts[i].MaxLife, 0f, 1f);
            spriteBatch.Draw(_texture, _hearts[i].Position, null,
                Color.White * alpha, _hearts[i].Rotation, origin,
                _hearts[i].Scale, SpriteEffects.None, 0f);
        }
    }
}
