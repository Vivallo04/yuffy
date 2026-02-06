using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Graphics;

public class Tilemap
{
    private readonly Texture2D _tileset;
    private readonly int[,] _map;
    private readonly int _tileSize;
    private readonly int _tilesPerRow;

    public static readonly int WaterTileId = TileId(12, 19);

    public float Scale { get; set; } = 1f;

    public int MapWidth => _map.GetLength(1);
    public int MapHeight => _map.GetLength(0);
    public int ScaledTileSize => (int)(_tileSize * Scale);

    public int GetTileAt(int col, int row)
    {
        if (row < 0 || row >= MapHeight || col < 0 || col >= MapWidth)
            return -1;
        return _map[row, col];
    }

    public Tilemap(Texture2D tileset, int[,] map, int tileSize = 16)
    {
        _tileset = tileset;
        _map = map;
        _tileSize = tileSize;
        _tilesPerRow = tileset.Width / tileSize;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int scaledTile = (int)(_tileSize * Scale);

        for (int row = 0; row < _map.GetLength(0); row++)
        {
            for (int col = 0; col < _map.GetLength(1); col++)
            {
                int tileId = _map[row, col];
                if (tileId < 0) continue;

                int srcCol = tileId % _tilesPerRow;
                int srcRow = tileId / _tilesPerRow;

                Rectangle sourceRect = new Rectangle(
                    srcCol * _tileSize,
                    srcRow * _tileSize,
                    _tileSize,
                    _tileSize
                );

                Vector2 position = new Vector2(col * scaledTile, row * scaledTile);

                spriteBatch.Draw(
                    _tileset,
                    position,
                    sourceRect,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }

    /// <summary>
    /// Helper to convert (column, row) in the tileset grid to a flat tile ID.
    /// </summary>
    public static int TileId(int col, int row, int tilesPerRow = 64)
    {
        return row * tilesPerRow + col;
    }

    /// <summary>
    /// Creates a natural grass map with a water pond.
    /// </summary>
    public static int[,] CreateGrassWithPondMap(int width, int height, int seed = 42)
    {
        // Solid green tiles from color reference row (row 1, cols 1-2) — no decorations
        int solidGreen1 = TileId(1, 1);
        int solidGreen2 = TileId(2, 1);

        // Decorated grass for rare accents
        int flowerGrass = TileId(4, 2);

        int[] weightedGrass =
        {
            solidGreen1, solidGreen1, solidGreen1, solidGreen1, solidGreen1,
            solidGreen1, solidGreen1, solidGreen1, solidGreen1,
            solidGreen2, solidGreen2, solidGreen2, solidGreen2, solidGreen2,
            solidGreen2, solidGreen2, solidGreen2, solidGreen2,
            flowerGrass,
        };

        // Water center tile (solid blue block at cols 11-14, rows 18-21)
        int water = TileId(12, 19);

        int[,] map = new int[height, width];
        Random rng = new Random(seed);

        // Fill with mostly solid green
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                map[row, col] = weightedGrass[rng.Next(weightedGrass.Length)];
            }
        }

        // Hand-crafted lake shape — organic blob positioned center-right
        // '.' = keep grass, 'W' = water
        string[] lakeShape =
        {
            "..WWW.",
            ".WWWWW",
            ".WWWWW",
            "..WWWW",
            "...WW.",
        };

        int lakeX = width - 10;
        int lakeY = (height - lakeShape.Length) / 2;

        for (int r = 0; r < lakeShape.Length; r++)
        {
            for (int c = 0; c < lakeShape[r].Length; c++)
            {
                int mapR = lakeY + r;
                int mapC = lakeX + c;
                if (mapR < 0 || mapR >= height || mapC < 0 || mapC >= width)
                    continue;

                if (lakeShape[r][c] == 'W')
                    map[mapR, mapC] = water;
            }
        }

        return map;
    }
}
