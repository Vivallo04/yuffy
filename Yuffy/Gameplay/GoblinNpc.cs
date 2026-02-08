using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Yuffy.Rendering;
using Yuffy.World;

namespace Yuffy.Gameplay;

public class GoblinNpc
{
    private enum State
    {
        Idle,
        Wander,
        Chase,
        Attack,
        Dying,
        Dead
    }

    private readonly AnimatedSprite _sprite;
    private readonly Animation _idleAnimation;
    private readonly Animation _walkAnimation;
    private readonly Animation _attackAnimation;
    private readonly Animation _hurtAnimation;
    private readonly Animation _deathAnimation;
    private readonly Animation _jumpAnimation;
    private readonly Tilemap _tilemap;
    private readonly Random _rng;
    private readonly Texture2D _alertTexture;

    private Vector2 _position;
    public float Y => _position.Y;
    public bool IsDead => _state == State.Dead;
    public bool IsAttacking => _state == State.Attack;
    public Vector2 Position => _position;

    private Vector2 _direction;
    private State _state;
    private float _stateTimer;
    private float _attackCooldown;
    private float _deathAnimTimer;
    private float _alertTimer;

    private const float WanderSpeed = 60f;
    private const float ChaseSpeed = 140f;
    private const float DetectionRadius = 300f;
    private const float AttackRange = 50f;
    private const int AttackDamage = 2;
    private const float AttackCooldownSeconds = 0.6f;

    public GoblinNpc(Texture2D idleTex, Texture2D walkTex, Texture2D attackTex,
        Texture2D hurtTex, Texture2D deathTex, Texture2D jumpTex,
        Texture2D alertTex, Tilemap tilemap, Random rng)
    {
        _tilemap = tilemap;
        _rng = rng;
        _alertTexture = alertTex;

        _idleAnimation = Animation.CreateFromSpriteStrip(idleTex, 8, TimeSpan.FromSeconds(1.0 / 6.0));
        _walkAnimation = Animation.CreateFromSpriteStrip(walkTex, 8, TimeSpan.FromSeconds(1.0 / 8.0));
        _attackAnimation = Animation.CreateFromSpriteStrip(attackTex, 9, TimeSpan.FromSeconds(1.0 / 10.0));
        _hurtAnimation = Animation.CreateFromSpriteStrip(hurtTex, 8, TimeSpan.FromSeconds(1.0 / 8.0));
        _deathAnimation = Animation.CreateFromSpriteStrip(deathTex, 9, TimeSpan.FromSeconds(1.0 / 8.0));
        _jumpAnimation = Animation.CreateFromSpriteStrip(jumpTex, 9, TimeSpan.FromSeconds(1.0 / 10.0));

        _sprite = new AnimatedSprite(_idleAnimation);
        _sprite.Scale = new Vector2(3f, 3f);
        _sprite.CenterOrigin();

        _state = State.Idle;
        _stateTimer = RandomRange(1f, 3f);
    }

    public void SpawnAtRandom(Vector2 playerPos)
    {
        int scaledTile = _tilemap.ScaledTileSize;

        for (int attempt = 0; attempt < 200; attempt++)
        {
            int col = _rng.Next(_tilemap.MapWidth);
            int row = _rng.Next(_tilemap.MapHeight);

            if (_tilemap.IsTileBlocked(col, row)) continue;

            Vector2 candidate = new Vector2(
                col * scaledTile + scaledTile / 2f,
                row * scaledTile + scaledTile / 2f
            );

            if (Vector2.Distance(candidate, playerPos) < DetectionRadius) continue;

            _position = candidate;
            return;
        }

        // Fallback: spiral search outward from map center
        int centerCol = _tilemap.MapWidth / 2;
        int centerRow = _tilemap.MapHeight / 2;
        for (int radius = 0; radius < Math.Max(_tilemap.MapWidth, _tilemap.MapHeight); radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Math.Abs(dx) != radius && Math.Abs(dy) != radius) continue;

                    int col = centerCol + dx;
                    int row = centerRow + dy;

                    if (col < 0 || col >= _tilemap.MapWidth || row < 0 || row >= _tilemap.MapHeight)
                        continue;

                    if (_tilemap.IsTileBlocked(col, row)) continue;

                    Vector2 candidate = new Vector2(
                        col * scaledTile + scaledTile / 2f,
                        row * scaledTile + scaledTile / 2f
                    );

                    if (Vector2.Distance(candidate, playerPos) < DetectionRadius) continue;

                    _position = candidate;
                    return;
                }
            }
        }

        throw new InvalidOperationException("Unable to find valid spawn location for GoblinNpc");
    }

    public int Update(GameTime gameTime, Vector2 playerPos)
    {
        if (_state == State.Dead) return 0;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        int damageDealt = 0;

        if (_attackCooldown > 0)
            _attackCooldown -= dt;
        if (_alertTimer > 0)
            _alertTimer -= dt;

        float distToPlayer = Vector2.Distance(_position, playerPos);

        switch (_state)
        {
            case State.Idle:
                _stateTimer -= dt;
                if (distToPlayer < DetectionRadius)
                    EnterChaseState(playerPos);
                else if (_stateTimer <= 0)
                    EnterWanderState();
                break;

            case State.Wander:
                _stateTimer -= dt;
                MoveInDirection(dt, WanderSpeed);
                if (distToPlayer < DetectionRadius)
                    EnterChaseState(playerPos);
                else if (_stateTimer <= 0)
                    EnterIdleState();
                break;

            case State.Chase:
                ChasePlayer(dt, playerPos);
                if (distToPlayer < AttackRange && _attackCooldown <= 0)
                    EnterAttackState(playerPos);
                else if (distToPlayer > DetectionRadius * 2.5f)
                    EnterWanderState();
                break;

            case State.Attack:
                _stateTimer -= dt;
                if (_stateTimer <= 0)
                {
                    if (_attackCooldown <= 0)
                    {
                        damageDealt = AttackDamage;
                        _attackCooldown = AttackCooldownSeconds;
                    }
                    EnterChaseState(playerPos);
                }
                break;

            case State.Dying:
                _deathAnimTimer -= dt;
                if (_deathAnimTimer <= 0)
                    _state = State.Dead;
                break;
        }

        _sprite.Update(gameTime);
        return damageDealt;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_state == State.Dead) return;
        _sprite.Draw(spriteBatch, _position);

        if (_alertTimer > 0 && _alertTexture != null)
        {
            float iconScale = 3f;
            Vector2 iconPos = new Vector2(
                _position.X - _alertTexture.Width * iconScale / 2f,
                _position.Y - 80f
            );
            spriteBatch.Draw(_alertTexture, iconPos, null, Color.White, 0f,
                Vector2.Zero, iconScale, SpriteEffects.None, 0f);
        }
    }

    public void TriggerDeath()
    {
        if (_state == State.Dying || _state == State.Dead) return;
        _state = State.Dying;
        _sprite.Animation = _deathAnimation;
        _deathAnimTimer = (float)_deathAnimation.Duration;
    }

    private void EnterIdleState()
    {
        _state = State.Idle;
        _stateTimer = RandomRange(0.5f, 1.5f);
        _sprite.Animation = _idleAnimation;
        _direction = Vector2.Zero;
    }

    private void EnterWanderState()
    {
        _state = State.Wander;
        _stateTimer = RandomRange(1f, 2.5f);
        _sprite.Animation = _walkAnimation;

        float angle = (float)(_rng.NextDouble() * Math.PI * 2.0);
        _direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

        UpdateFacing(_direction);
    }

    private void EnterChaseState(Vector2 playerPos)
    {
        if (_state == State.Idle || _state == State.Wander)
            _alertTimer = 1.0f;
        _state = State.Chase;
        _sprite.Animation = _walkAnimation;
        UpdateDirectionToPlayer(playerPos);
    }

    private void EnterAttackState(Vector2 playerPos)
    {
        _state = State.Attack;
        _sprite.Animation = _attackAnimation;
        _stateTimer = (float)_attackAnimation.Duration;
        UpdateFacing(playerPos - _position);
    }

    private void ChasePlayer(float dt, Vector2 playerPos)
    {
        UpdateDirectionToPlayer(playerPos);
        MoveInDirection(dt, ChaseSpeed);
    }

    private void UpdateDirectionToPlayer(Vector2 playerPos)
    {
        Vector2 diff = playerPos - _position;
        if (diff.LengthSquared() > 0)
        {
            diff.Normalize();
            _direction = diff;
            UpdateFacing(_direction);
        }
    }

    private void MoveInDirection(float dt, float speed)
    {
        Vector2 nextPosition = _position + _direction * speed * dt;
        if (IsPositionValid(nextPosition))
            _position = nextPosition;
        else if (_state == State.Chase)
        {
            Vector2 slideX = _position + new Vector2(_direction.X * speed * dt, 0);
            Vector2 slideY = _position + new Vector2(0, _direction.Y * speed * dt);
            if (IsPositionValid(slideX))
                _position = slideX;
            else if (IsPositionValid(slideY))
                _position = slideY;
        }
        else if (_state == State.Wander)
            EnterIdleState();
    }

    private void UpdateFacing(Vector2 dir)
    {
        if (dir.X < 0)
            _sprite.Effects = SpriteEffects.FlipHorizontally;
        else if (dir.X > 0)
            _sprite.Effects = SpriteEffects.None;
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
