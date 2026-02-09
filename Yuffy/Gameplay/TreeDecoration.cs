using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Yuffy.Rendering;

namespace Yuffy.Gameplay;

public class TreeDecoration
{
    private readonly AnimatedSprite _sprite;
    private readonly Vector2 _position;

    public Point BlockedTile { get; }
    public float Y => _position.Y;

    public TreeDecoration(Texture2D texture, int frameCount, Vector2 position, Point blockedTile, Random rng)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentNullException.ThrowIfNull(rng);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(frameCount, 0);

        BlockedTile = blockedTile;
        _position = position;

        var animation = Animation.CreateFromSpriteStrip(texture, frameCount, TimeSpan.FromSeconds(1.0 / 3.0));
        _sprite = new AnimatedSprite(animation);
        _sprite.Scale = new Vector2(3f, 3f);
        _sprite.Origin = new Vector2(animation.Frames[0].Width / 2f, animation.Frames[0].Height);

        int startFrame = rng.Next(frameCount);
        double elapsedFraction = rng.NextDouble();
        _sprite.SetFrame(startFrame, TimeSpan.FromSeconds(elapsedFraction / 3.0));
    }

    public void Update(GameTime gameTime)
    {
        _sprite.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _sprite.Draw(spriteBatch, _position);
    }
}
