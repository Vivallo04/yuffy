using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Graphics;

public class Animation
{
    public List<TextureRegion> Frames { get; }
    public TimeSpan Delay { get; }

    public Animation(List<TextureRegion> frames, TimeSpan delay)
    {
        Frames = frames;
        Delay = delay;
    }

    public static Animation CreateFromSpriteStrip(Texture2D texture, int frameCount, TimeSpan delay)
    {
        int frameWidth = texture.Width / frameCount;
        int frameHeight = texture.Height;

        List<TextureRegion> frames = new List<TextureRegion>();
        for (int i = 0; i < frameCount; i++)
        {
            Rectangle sourceRect = new Rectangle(i * frameWidth, 0, frameWidth, frameHeight);
            frames.Add(new TextureRegion(texture, sourceRect));
        }

        return new Animation(frames, delay);
    }
}
