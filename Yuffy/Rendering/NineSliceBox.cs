using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Rendering;

public class NineSliceBox
{
    private readonly Texture2D _tl, _tc, _tr;
    private readonly Texture2D _lc, _c, _rc;
    private readonly Texture2D _bl, _bc, _br;
    private readonly int _cornerSize;

    public NineSliceBox(
        Texture2D tl, Texture2D tc, Texture2D tr,
        Texture2D lc, Texture2D c, Texture2D rc,
        Texture2D bl, Texture2D bc, Texture2D br,
        int scale)
    {
        ArgumentNullException.ThrowIfNull(tl);
        ArgumentNullException.ThrowIfNull(tc);
        ArgumentNullException.ThrowIfNull(tr);
        ArgumentNullException.ThrowIfNull(lc);
        ArgumentNullException.ThrowIfNull(c);
        ArgumentNullException.ThrowIfNull(rc);
        ArgumentNullException.ThrowIfNull(bl);
        ArgumentNullException.ThrowIfNull(bc);
        ArgumentNullException.ThrowIfNull(br);

        _tl = tl; _tc = tc; _tr = tr;
        _lc = lc; _c = c;   _rc = rc;
        _bl = bl; _bc = bc; _br = br;
        _cornerSize = tl.Width * scale;
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle bounds, Color? tint = null)
    {
        var color = tint ?? Color.White;
        int cs = _cornerSize;
        int innerW = Math.Max(0, bounds.Width - 2 * cs);
        int innerH = Math.Max(0, bounds.Height - 2 * cs);
        int x = bounds.X;
        int y = bounds.Y;

        // Corners
        spriteBatch.Draw(_tl, new Rectangle(x, y, cs, cs), color);
        spriteBatch.Draw(_tr, new Rectangle(x + cs + innerW, y, cs, cs), color);
        spriteBatch.Draw(_bl, new Rectangle(x, y + cs + innerH, cs, cs), color);
        spriteBatch.Draw(_br, new Rectangle(x + cs + innerW, y + cs + innerH, cs, cs), color);

        // Edges
        spriteBatch.Draw(_tc, new Rectangle(x + cs, y, innerW, cs), color);
        spriteBatch.Draw(_bc, new Rectangle(x + cs, y + cs + innerH, innerW, cs), color);
        spriteBatch.Draw(_lc, new Rectangle(x, y + cs, cs, innerH), color);
        spriteBatch.Draw(_rc, new Rectangle(x + cs + innerW, y + cs, cs, innerH), color);

        // Center
        spriteBatch.Draw(_c, new Rectangle(x + cs, y + cs, innerW, innerH), color);
    }
}
