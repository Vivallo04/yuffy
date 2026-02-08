using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Yuffy.Graphics;

namespace Yuffy;

public class SkeletonNpc
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

    private const float WanderSpeed = 45f;
    private const float ChaseSpeed = 110f;
    private const float DetectionRadius = 250f;
    private const float AttackRange = 55f;
    private const int AttackDamage = 1;
    private const float AttackCooldownSeconds = 0.8f;

    public SkeletonNpc(Texture2D idleTex, Texture2D walkTex, Texture2D attackTex,
        Texture2D hurtTex, Texture2D deathTex, Texture2D jumpTex,
        Texture2D alertTex, Tilemap tilemap, Random rng)
    {
        _tilemap = tilemap;
        _rng = rng;
        _alertTexture = alertTex;

        _idleAnimation = Animation.CreateFromSpriteStrip(idleTex, 6, TimeSpan.FromSeconds(1.0 / 6.0));
        _walkAnimation = Animation.CreateFromSpriteStrip(walkTex, 8, TimeSpan.FromSeconds(1.0 / 8.0));
        _attackAnimation = Animation.CreateFromSpriteStrip(attackTex, 7, TimeSpan.FromSeconds(1.0 / 10.0));
        _hurtAnimation = Animation.CreateFromSpriteStrip(hurtTex, 7, TimeSpan.FromSeconds(1.0 / 8.0));
        _deathAnimation = Animation.CreateFromSpriteStrip(deathTex, 10, TimeSpan.FromSeconds(1.0 / 8.0));
        _jumpAnimation = Animation.CreateFromSpriteStrip(jumpTex, 10, TimeSpan.FromSeconds(1.0 / 10.0));

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

            if (Vector2.Distance(candidate, playerPos) < 300f) continue;

            _position = candidate;
            return;
        }

        _position = new Vector2(scaledTile / 2f, scaledTile / 2f);
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
                else if (distToPlayer > DetectionRadius * 1.5f)
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
        _deathAnimTimer = 10 * (1.0f / 8.0f);
    }

    private void EnterIdleState()
    {
        _state = State.Idle;
        _stateTimer = RandomRange(1f, 3f);
        _sprite.Animation = _idleAnimation;
        _direction = Vector2.Zero;
    }

    private void EnterWanderState()
    {
        _state = State.Wander;
        _stateTimer = RandomRange(2f, 4f);
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
        _stateTimer = 7 * (1.0f / 10.0f);
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
