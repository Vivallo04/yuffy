using System;
using Microsoft.Xna.Framework;

namespace Yuffy;

public class Camera
{
    public Vector2 Position { get; private set; }

    private readonly int _virtualWidth;
    private readonly int _virtualHeight;

    public Camera(int virtualWidth, int virtualHeight)
    {
        _virtualWidth = virtualWidth;
        _virtualHeight = virtualHeight;
    }

    public void Follow(Vector2 target, int worldWidth, int worldHeight)
    {
        float x = target.X - _virtualWidth / 2f;
        float y = target.Y - _virtualHeight / 2f;

        x = MathHelper.Clamp(x, 0, Math.Max(0, worldWidth - _virtualWidth));
        y = MathHelper.Clamp(y, 0, Math.Max(0, worldHeight - _virtualHeight));

        Position = new Vector2(x, y);
    }

    public Matrix GetTransformMatrix()
    {
        return Matrix.CreateTranslation(-(int)Position.X, -(int)Position.Y, 0f);
    }
}
