using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Yuffy.Rendering;

namespace Yuffy.Gameplay;

public class CropDecoration
{
    private readonly Sprite _soilSprite;
    private readonly Sprite _cropSprite;
    private readonly Vector2 _position;

    public float Y => _position.Y;

    public CropDecoration(Texture2D soilTexture, TextureRegion cropRegion, Vector2 position)
    {
        ArgumentNullException.ThrowIfNull(soilTexture);
        ArgumentNullException.ThrowIfNull(cropRegion);
        _position = position;

        var soilRegion = new TextureRegion(soilTexture);
        _soilSprite = new Sprite(soilRegion);
        _soilSprite.Scale = new Vector2(3f, 3f);
        _soilSprite.CenterOrigin();

        _cropSprite = new Sprite(cropRegion);
        _cropSprite.Scale = new Vector2(3f, 3f);
        _cropSprite.CenterOrigin();
    }

    public void DrawSoil(SpriteBatch spriteBatch)
    {
        _soilSprite.Draw(spriteBatch, _position);
    }

    public void DrawCrop(SpriteBatch spriteBatch)
    {
        _cropSprite.Draw(spriteBatch, _position);
    }
}
