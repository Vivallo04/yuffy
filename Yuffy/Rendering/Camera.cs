using System;
using Microsoft.Xna.Framework;

namespace Yuffy.Rendering;

public class Camera
{
    public Vector2 Position { get; private set; }

    private readonly VirtualViewport _viewport;

    public Camera(VirtualViewport viewport)
    {
        _viewport = viewport;
    }

    public void Follow(Vector2 target, int worldWidth, int worldHeight)
    {
        float x = target.X - _viewport.Width / 2f;
        float y = target.Y - _viewport.Height / 2f;

        x = MathHelper.Clamp(x, 0, Math.Max(0, worldWidth - _viewport.Width));
        y = MathHelper.Clamp(y, 0, Math.Max(0, worldHeight - _viewport.Height));

        Position = new Vector2(x, y);
    }

    public Matrix GetTransformMatrix()
    {
        return Matrix.CreateTranslation(-(int)Position.X, -(int)Position.Y, 0f);
    }
}
