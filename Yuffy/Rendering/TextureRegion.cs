using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Rendering;

public class TextureRegion
{
    public Texture2D Texture { get; }
    public Rectangle SourceRect { get; }

    public int Width => SourceRect.Width;
    public int Height => SourceRect.Height;

    public TextureRegion(Texture2D texture, Rectangle sourceRect)
    {
        ArgumentNullException.ThrowIfNull(texture);
        Texture = texture;
        SourceRect = sourceRect;
    }

    public TextureRegion(Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        Texture = texture;
        SourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
    }
}
