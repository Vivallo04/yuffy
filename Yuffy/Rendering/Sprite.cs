using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Rendering;

public class Sprite
{
    public TextureRegion Region { get; set; }
    public Color Color { get; set; } = Color.White;
    public float Rotation { get; set; } = 0.0f;
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;
    public float LayerDepth { get; set; } = 0.0f;

    public Sprite() { }

    public Sprite(TextureRegion region)
    {
        Region = region;
    }

    public void CenterOrigin()
    {
        if (Region != null)
        {
            Origin = new Vector2(Region.Width * 0.5f, Region.Height * 0.5f);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        if (Region == null) return;

        spriteBatch.Draw(
            Region.Texture,
            position,
            Region.SourceRect,
            Color,
            Rotation,
            Origin,
            Scale,
            Effects,
            LayerDepth
        );
    }
}
