using System;

namespace Yuffy.Rendering;

public class VirtualViewport
{
    public const int BaseWidth = 960;
    public const int BaseHeight = 540;
    private const float BaseAspect = 16f / 9f;

    public int Width { get; private set; } = BaseWidth;
    public int Height { get; private set; } = BaseHeight;
    public float OverlayScale { get; private set; } = 1.0f;
    public bool SizeChanged { get; set; }

    public void Refresh(int clientWidth, int clientHeight)
    {
        if (clientWidth <= 0 || clientHeight <= 0) return;

        int vw, vh;
        if ((float)clientWidth / clientHeight >= BaseAspect)
        {
            vh = clientHeight;
            vw = (int)(vh * BaseAspect);
        }
        else
        {
            vw = clientWidth;
            vh = (int)(vw / BaseAspect);
        }

        if (vw < BaseWidth || vh < BaseHeight)
        {
            vw = BaseWidth;
            vh = BaseHeight;
        }

        if (vw != Width || vh != Height)
        {
            Width = vw;
            Height = vh;
            OverlayScale = Math.Clamp(Height / (float)BaseHeight, 0.85f, 1.35f);
            SizeChanged = true;
        }
    }
}
