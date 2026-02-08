using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Rendering;

public class Animation
{
    public IReadOnlyList<TextureRegion> Frames { get; }
    public TimeSpan Delay { get; }
    public int FrameCount => Frames.Count;
    public double Duration => FrameCount * Delay.TotalSeconds;

    public Animation(IReadOnlyList<TextureRegion> frames, TimeSpan delay)
    {
        ArgumentNullException.ThrowIfNull(frames);
        if (frames.Count == 0)
            throw new ArgumentException("Animation must have at least one frame.", nameof(frames));
        if (delay <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be greater than zero.");
        
        Frames = frames.ToList().AsReadOnly();
        Delay = delay;
    }

    public static Animation CreateFromSpriteStrip(Texture2D texture, int frameCount, TimeSpan delay)
    {
        ArgumentNullException.ThrowIfNull(texture);
        if (frameCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(frameCount), "Frame count must be greater than zero.");

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
