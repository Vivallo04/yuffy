using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Gameplay;

public class Collectible
{
    private readonly Texture2D _texture;
    private const float Scale = 3f;
    private const float MushroomScale = 2.5f;
    private const float MagnetRadius = 80f;
    private const float CollectRadius = 20f;
    private const float MagnetSpeed = 200f;

    public Vector2 Position;
    public bool Collected;
    public float Y => Position.Y;

    public Collectible(Texture2D texture, Vector2 position)
    {
        _texture = texture;
        Position = position;
    }

    public void Update(float deltaTime, Vector2 playerPos)
    {
        if (Collected) return;

        Vector2 diff = playerPos - Position;
        float dist = diff.Length();

        if (dist < CollectRadius)
        {
            Collected = true;
            return;
        }

        if (dist < MagnetRadius && dist > 0)
        {
            Vector2 dir = diff / dist;
            Position += dir * MagnetSpeed * deltaTime;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Collected) return;

        var origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
        spriteBatch.Draw(_texture, Position, null, Color.White,
            0f, origin, MushroomScale, SpriteEffects.None, 0f);
    }
}
