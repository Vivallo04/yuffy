using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Rendering;

public class TextureRegion
{
    public Texture2D Texture { get; set; }
    public Rectangle SourceRect { get; set; }

    public int Width => SourceRect.Width;
    public int Height => SourceRect.Height;

    public TextureRegion(Texture2D texture, Rectangle sourceRect)
    {
        Texture = texture;
        SourceRect = sourceRect;
    }

    public TextureRegion(Texture2D texture)
    {
        Texture = texture;
        SourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
    }
}
