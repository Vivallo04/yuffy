using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Yuffy.Graphics;

namespace Yuffy;

public class AnimalNpc
{
    private enum State
    {
        Idle,
        Walking
    }

    private readonly AnimatedSprite _sprite;
    private readonly Animation _idleAnimation;
    private readonly Animation _walkAnimation;
    private readonly float _moveSpeed;
    private readonly Tilemap _tilemap;
    private readonly Random _rng;

    private Vector2 _position;
    public float Y => _position.Y;
    private Vector2 _direction;
    private State _state;
    private float _stateTimer;

    private const float IdleMinSeconds = 2.0f;
    private const float IdleMaxSeconds = 5.0f;
    private const float WalkMinSeconds = 1.0f;
    private const float WalkMaxSeconds = 3.0f;

    public AnimalNpc(Texture2D texture, Tilemap tilemap, float moveSpeed, Random rng)
    {
        _tilemap = tilemap;
        _moveSpeed = moveSpeed;
        _rng = rng;

        _idleAnimation = Animation.CreateFromSpriteStrip(texture, 4, TimeSpan.FromSeconds(1.0 / 3.0));
        _walkAnimation = Animation.CreateFromSpriteStrip(texture, 4, TimeSpan.FromSeconds(1.0 / 8.0));

        _sprite = new AnimatedSprite(_idleAnimation);
        _sprite.Scale = new Vector2(3f, 3f);
        _sprite.CenterOrigin();

        _state = State.Idle;
        _stateTimer = RandomRange(IdleMinSeconds, IdleMaxSeconds);
    }

    public void SpawnAtRandomGrass()
    {
        int scaledTile = _tilemap.ScaledTileSize;

        for (int attempt = 0; attempt < 100; attempt++)
        {
            int col = _rng.Next(_tilemap.MapWidth);
            int row = _rng.Next(_tilemap.MapHeight);

            if (!_tilemap.IsTileBlocked(col, row))
            {
                _position = new Vector2(
                    col * scaledTile + scaledTile / 2f,
                    row * scaledTile + scaledTile / 2f
                );
                return;
            }
        }

        _position = new Vector2(scaledTile / 2f, scaledTile / 2f);
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _stateTimer -= dt;

        if (_stateTimer <= 0)
        {
            if (_state == State.Idle)
                EnterWalkState();
            else
                EnterIdleState();
        }

        if (_state == State.Walking)
        {
            Vector2 nextPosition = _position + _direction * _moveSpeed * dt;

            if (IsPositionValid(nextPosition))
                _position = nextPosition;
            else
                EnterIdleState();
        }

        _sprite.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _sprite.Draw(spriteBatch, _position);
    }

    private void EnterIdleState()
    {
        _state = State.Idle;
        _stateTimer = RandomRange(IdleMinSeconds, IdleMaxSeconds);
        _sprite.Animation = _idleAnimation;
        _direction = Vector2.Zero;
    }

    private void EnterWalkState()
    {
        _state = State.Walking;
        _stateTimer = RandomRange(WalkMinSeconds, WalkMaxSeconds);
        _sprite.Animation = _walkAnimation;

        float angle = (float)(_rng.NextDouble() * Math.PI * 2.0);
        _direction = new Vector2(
            (float)Math.Cos(angle),
            (float)Math.Sin(angle)
        );

        _sprite.Effects = _direction.X > 0
            ? SpriteEffects.FlipHorizontally
            : SpriteEffects.None;
    }

    private bool IsPositionValid(Vector2 position)
    {
        int scaledTile = _tilemap.ScaledTileSize;
        int totalWidth = _tilemap.MapWidth * scaledTile;
        int totalHeight = _tilemap.MapHeight * scaledTile;

        float margin = scaledTile * 0.5f;
        if (position.X < margin || position.X > totalWidth - margin ||
            position.Y < margin || position.Y > totalHeight - margin)
            return false;

        int col = (int)(position.X / scaledTile);
        int row = (int)(position.Y / scaledTile);

        return !_tilemap.IsTileBlocked(col, row);
    }

    private float RandomRange(float min, float max)
    {
        return min + (float)_rng.NextDouble() * (max - min);
    }
}
