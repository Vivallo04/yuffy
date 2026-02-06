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

    public void Draw(SpriteBatch spriteBatch, Rectangle? visibleArea = null)
    {
        int scaledTile = (int)(_tileSize * Scale);

        int startCol = 0, endCol = _map.GetLength(1);
        int startRow = 0, endRow = _map.GetLength(0);

        if (visibleArea.HasValue)
        {
            var area = visibleArea.Value;
            startCol = Math.Max(0, area.X / scaledTile);
            startRow = Math.Max(0, area.Y / scaledTile);
            endCol = Math.Min(_map.GetLength(1), area.Right / scaledTile + 2);
            endRow = Math.Min(_map.GetLength(0), area.Bottom / scaledTile + 2);
        }

        for (int row = startRow; row < endRow; row++)
        {
            for (int col = startCol; col < endCol; col++)
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
    /// Creates a natural grass map with water ponds.
    /// </summary>
    public static int[,] CreateGrassWithPondMap(int width, int height, int seed = 42)
    {
        int solidGreen1 = TileId(1, 1);
        int solidGreen2 = TileId(2, 1);
        int flowerGrass = TileId(4, 2);

        int[] weightedGrass =
        {
            solidGreen1, solidGreen1, solidGreen1, solidGreen1, solidGreen1,
            solidGreen1, solidGreen1, solidGreen1, solidGreen1,
            solidGreen2, solidGreen2, solidGreen2, solidGreen2, solidGreen2,
            solidGreen2, solidGreen2, solidGreen2, solidGreen2,
            flowerGrass,
        };

        int water = TileId(12, 19);

        int[,] map = new int[height, width];
        Random rng = new Random(seed);

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                map[row, col] = weightedGrass[rng.Next(weightedGrass.Length)];
            }
        }

        string[][] lakeTemplates =
        {
            new[] { "..WWW.", ".WWWWW", ".WWWWW", "..WWWW", "...WW." },
            new[] { ".WW.", "WWWW", "WWWW", ".WW." },
            new[] { "..WWWWWW..", ".WWWWWWWW.", "..WWWWWW.." },
            new[] { "WWW...", "WWWW..", ".WWWW.", "..WWWW", "...WWW" },
        };

        int numPonds = Math.Max(1, (width * height) / 200);

        for (int i = 0; i < numPonds; i++)
        {
            var template = lakeTemplates[rng.Next(lakeTemplates.Length)];
            int templateH = template.Length;
            int templateW = template[0].Length;

            int margin = 3;
            int px = rng.Next(margin, Math.Max(margin + 1, width - templateW - margin));
            int py = rng.Next(margin, Math.Max(margin + 1, height - templateH - margin));

            for (int r = 0; r < templateH; r++)
            {
                for (int c = 0; c < template[r].Length; c++)
                {
                    if (template[r][c] == 'W')
                    {
                        int mapR = py + r;
                        int mapC = px + c;
                        if (mapR >= 0 && mapR < height && mapC >= 0 && mapC < width)
                            map[mapR, mapC] = water;
                    }
                }
            }
        }

        return map;
    }
}
