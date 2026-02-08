using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Yuffy.Graphics;

public class Tilemap
{
    private readonly Texture2D _tileset;
    private readonly int[,] _map;
    private readonly int _tileSize;
    private readonly int _tilesPerRow;
    private readonly HashSet<Point> _blockedTiles = new();

    public static readonly int WaterTileId = TileId(12, 19);
    public static readonly int PlainWaterTileId = TileId(4, 1);

    public static readonly int SandTile1 = TileId(5, 1);
    public static readonly int SandTile2 = TileId(7, 1);
    public static readonly int SandTile3 = TileId(8, 1);

    public static readonly int OceanWater1 = TileId(11, 19);
    public static readonly int OceanWater3 = TileId(13, 19);

    public static readonly int SandWaterEdgeRight = TileId(19, 5); // \ sand NW, water SE
    public static readonly int SandWaterEdgeFlat  = TileId(20, 5); // — sand top, water bottom
    public static readonly int SandWaterEdgeLeft  = TileId(21, 5); // / sand NE, water SW

    public static bool IsWaterTile(int tileId)
    {
        return tileId == WaterTileId || tileId == PlainWaterTileId
            || tileId == OceanWater1 || tileId == OceanWater3;
    }

    public static bool IsSandWaterEdgeTile(int tileId)
    {
        return tileId == SandWaterEdgeRight || tileId == SandWaterEdgeFlat || tileId == SandWaterEdgeLeft;
    }

    public float Scale { get; set; } = 1f;

    public int MapWidth => _map.GetLength(1);
    public int MapHeight => _map.GetLength(0);
    public int ScaledTileSize => (int)(_tileSize * Scale);

    public bool IsGrassBiomeRow(int row)
    {
        return row > MapHeight / 2 + 5;
    }

    public bool IsBeachRow(int row)
    {
        return row >= MapHeight - 8;
    }

    public int GetTileAt(int col, int row)
    {
        if (row < 0 || row >= MapHeight || col < 0 || col >= MapWidth)
            return -1;
        return _map[row, col];
    }

    public void SetTileAt(int col, int row, int tileId)
    {
        if (row >= 0 && row < MapHeight && col >= 0 && col < MapWidth)
            _map[row, col] = tileId;
    }

    public void BlockTile(int col, int row)
    {
        _blockedTiles.Add(new Point(col, row));
    }

    public bool IsTileBlocked(int col, int row)
    {
        if (row < 0 || row >= MapHeight || col < 0 || col >= MapWidth)
            return true;
        return IsWaterTile(_map[row, col]) || _blockedTiles.Contains(new Point(col, row));
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

                // Draw water base behind edge tiles (they have transparent areas)
                if (IsSandWaterEdgeTile(tileId))
                {
                    int waterSrcCol = PlainWaterTileId % _tilesPerRow;
                    int waterSrcRow = PlainWaterTileId / _tilesPerRow;
                    Rectangle waterRect = new Rectangle(
                        waterSrcCol * _tileSize, waterSrcRow * _tileSize,
                        _tileSize, _tileSize);
                    spriteBatch.Draw(_tileset, position, waterRect, Color.White,
                        0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                }

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
    /// Creates a biome map with grass in the south, snow in the north, and a gradual transition.
    /// </summary>
    public static int[,] CreateBiomeMap(int width, int height, int seed = 42)
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
            flowerGrass
        };

        int[] weightedSnow =
        {
            TileId(1, 18)
        };

        int[] waterTiles = { WaterTileId, PlainWaterTileId };

        int[,] map = new int[height, width];
        Random rng = new Random(seed);

        int transitionRadius = 5;
        int midRow = height / 2;

        for (int row = 0; row < height; row++)
        {
            float snowProb;
            if (row < midRow - transitionRadius)
                snowProb = 1.0f;
            else if (row > midRow + transitionRadius)
                snowProb = 0.0f;
            else
                snowProb = 1.0f - (float)(row - (midRow - transitionRadius)) / (transitionRadius * 2);

            for (int col = 0; col < width; col++)
            {
                if (rng.NextDouble() < snowProb)
                    map[row, col] = weightedSnow[rng.Next(weightedSnow.Length)];
                else
                    map[row, col] = weightedGrass[rng.Next(weightedGrass.Length)];
            }
        }

        // Beach biome: natural coastline at the south
        int[] weightedSand = { SandTile1, SandTile1, SandTile2, SandTile2, SandTile3 };

        int baseOceanRow = height - 4;
        int baseSandRow = height - 8;

        int[] sandEdge = new int[width];
        int[] oceanEdge = new int[width];

        float sandWalk = 0f;
        float oceanWalk = 0f;
        for (int col = 0; col < width; col++)
        {
            sandWalk += (float)(rng.NextDouble() * 2.0 - 1.0) * 1.5f;
            sandWalk = Math.Clamp(sandWalk, -3f, 3f);
            oceanWalk += (float)(rng.NextDouble() * 2.0 - 1.0) * 1.0f;
            oceanWalk = Math.Clamp(oceanWalk, -2f, 2f);

            sandEdge[col] = Math.Clamp(baseSandRow + (int)Math.Round(sandWalk), baseSandRow - 3, baseOceanRow - 1);
            oceanEdge[col] = Math.Clamp(baseOceanRow + (int)Math.Round(oceanWalk), sandEdge[col] + 1, height - 1);
        }

        // Ensure ocean edge changes by at most 1 row per column for connected coastline
        for (int col = 1; col < width; col++)
        {
            int smoothed = Math.Clamp(oceanEdge[col], oceanEdge[col - 1] - 1, oceanEdge[col - 1] + 1);
            oceanEdge[col] = Math.Max(smoothed, sandEdge[col] + 1);
        }

        for (int col = 0; col < width; col++)
        {
            for (int row = sandEdge[col]; row < height; row++)
            {
                if (row >= oceanEdge[col])
                    map[row, col] = PlainWaterTileId;
                else
                    map[row, col] = weightedSand[rng.Next(weightedSand.Length)];
            }
        }

        // Place interconnected sand-water edge tiles at the coastline
        for (int col = 0; col < width; col++)
        {
            int edgeRow = oceanEdge[col];
            if (edgeRow < 0 || edgeRow >= height) continue;

            int prevEdge = col > 0 ? oceanEdge[col - 1] : edgeRow;
            int diff = edgeRow - prevEdge;

            if (diff > 0)
                map[edgeRow, col] = SandWaterEdgeRight; // \ coast descends
            else if (diff < 0)
                map[edgeRow, col] = SandWaterEdgeLeft;  // / coast ascends
            else
                map[edgeRow, col] = SandWaterEdgeFlat;  // — flat coast

            // Plain water tile directly below every edge
            if (edgeRow + 1 < height)
                map[edgeRow + 1, col] = PlainWaterTileId;
        }

        string[][] lakeTemplates =
        {
            new[] { "..WWW.", ".WWWWW", ".WWWWW", "..WWWW", "...WW." },
            new[] { ".WW.", "WWWW", "WWWW", ".WW." },
            new[] { "..WWWWWW..", ".WWWWWWWW.", "..WWWWWW.." },
            new[] { "WWW...", "WWWW..", ".WWWW.", "..WWWW", "...WWW" }
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

            int pondCenterRow = py + templateH / 2;
            bool isSnowPond = pondCenterRow < midRow - transitionRadius;
            if (isSnowPond) continue;
            if (py + templateH > baseSandRow - 3) continue;
            int[] pondTiles = waterTiles;

            for (int r = 0; r < templateH; r++)
            {
                for (int c = 0; c < template[r].Length; c++)
                {
                    if (template[r][c] == 'W')
                    {
                        int mapR = py + r;
                        int mapC = px + c;
                        if (mapR >= 0 && mapR < height && mapC >= 0 && mapC < width)
                            map[mapR, mapC] = pondTiles[rng.Next(pondTiles.Length)];
                    }
                }
            }
        }

        return map;
    }
}
