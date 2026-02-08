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

    // Coast autotile set (tileset rows 28-32, cols 4-8)
    // Straight edges
    public static readonly int CoastN  = TileId(6, 28);
    public static readonly int CoastS  = TileId(6, 32);
    public static readonly int CoastE  = TileId(8, 30);
    public static readonly int CoastW  = TileId(4, 30);
    // Outer corners (sand with water peeking in)
    public static readonly int CoastNW = TileId(5, 29);
    public static readonly int CoastNE = TileId(7, 29);
    public static readonly int CoastSW = TileId(5, 31);
    public static readonly int CoastSE = TileId(7, 31);
    // Inner corners (tiny water overlays on sand)
    public static readonly int CoastInnerSE = TileId(5, 28);
    public static readonly int CoastInnerSW = TileId(7, 28);
    public static readonly int CoastInnerNE = TileId(4, 31);
    public static readonly int CoastInnerNW = TileId(8, 31);

    public static bool IsWaterTile(int tileId)
    {
        return tileId == WaterTileId || tileId == PlainWaterTileId
            || tileId == OceanWater1 || tileId == OceanWater3;
    }

    public static bool IsSandTile(int tileId)
    {
        return tileId == SandTile1 || tileId == SandTile2 || tileId == SandTile3;
    }

    private static readonly HashSet<int> CoastEdgeAndOuterCorners = new()
    {
        CoastN, CoastS, CoastE, CoastW,
        CoastNW, CoastNE, CoastSW, CoastSE
    };

    private static readonly HashSet<int> CoastInnerCorners = new()
    {
        CoastInnerSE, CoastInnerSW, CoastInnerNE, CoastInnerNW
    };

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

                Vector2 basePosition = new Vector2(col * scaledTile, row * scaledTile);
                Vector2 position = basePosition;

                // Visual-only lift for south-facing diagonals by one tile-size unit.
                bool isSouthDiagonal = tileId == CoastSW || tileId == CoastSE
                    || tileId == CoastInnerSW || tileId == CoastInnerSE;
                if (isSouthDiagonal && row < MapHeight - 1 && IsWaterTile(_map[row + 1, col]))
                {
                    position.Y -= _tileSize * Scale;
                    int fillWaterTileId = IsWaterTile(_map[row + 1, col]) ? _map[row + 1, col] : WaterTileId;
                    int fillWaterSrcCol = fillWaterTileId % _tilesPerRow;
                    int fillWaterSrcRow = fillWaterTileId / _tilesPerRow;
                    var fillWaterRect = new Rectangle(
                        fillWaterSrcCol * _tileSize, fillWaterSrcRow * _tileSize,
                        _tileSize, _tileSize);
                    Vector2 fillPosition = basePosition;
                    spriteBatch.Draw(_tileset, fillPosition, fillWaterRect, Color.White,
                        0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                }

                // Coast tiles: draw appropriate base behind transparent areas
                if (CoastEdgeAndOuterCorners.Contains(tileId))
                {
                    int waterSrcCol = PlainWaterTileId % _tilesPerRow;
                    int waterSrcRow = PlainWaterTileId / _tilesPerRow;
                    var waterRect = new Rectangle(
                        waterSrcCol * _tileSize, waterSrcRow * _tileSize,
                        _tileSize, _tileSize);
                    spriteBatch.Draw(_tileset, position, waterRect, Color.White,
                        0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                }
                else if (CoastInnerCorners.Contains(tileId))
                {
                    int sandSrcCol = SandTile1 % _tilesPerRow;
                    int sandSrcRow = SandTile1 / _tilesPerRow;
                    var sandRect = new Rectangle(
                        sandSrcCol * _tileSize, sandSrcRow * _tileSize,
                        _tileSize, _tileSize);
                    spriteBatch.Draw(_tileset, position, sandRect, Color.White,
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

    private readonly struct WaterComponent
    {
        public readonly int MinCol;
        public readonly int MaxCol;
        public readonly List<Point> Cells;

        public WaterComponent(int minCol, int maxCol, List<Point> cells)
        {
            MinCol = minCol;
            MaxCol = maxCol;
            Cells = cells;
        }
    }

    private static void SmoothAllWaterProfiles(int[,] map, int width, int height)
    {
        for (int pass = 0; pass < 2; pass++)
        {
            var components = CollectWaterComponents(map, width, height);
            foreach (var component in components)
            {
                int[] originalProfile = BuildTopProfile(component, map, height);
                int[] smoothedProfile = (int[])originalProfile.Clone();
                SmoothProfileStrict(smoothedProfile);
                ApplySmoothedTopProfile(component, originalProfile, smoothedProfile, map);
            }
        }
    }

    private static List<WaterComponent> CollectWaterComponents(int[,] map, int width, int height)
    {
        var components = new List<WaterComponent>();
        bool[,] visited = new bool[height, width];
        int[] dRow = { -1, 1, 0, 0 };
        int[] dCol = { 0, 0, -1, 1 };

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (visited[row, col] || !IsWaterTile(map[row, col]))
                    continue;

                var queue = new Queue<Point>();
                var cells = new List<Point>();
                int minCol = col;
                int maxCol = col;

                visited[row, col] = true;
                queue.Enqueue(new Point(col, row));

                while (queue.Count > 0)
                {
                    Point p = queue.Dequeue();
                    cells.Add(p);
                    minCol = Math.Min(minCol, p.X);
                    maxCol = Math.Max(maxCol, p.X);

                    for (int i = 0; i < 4; i++)
                    {
                        int nr = p.Y + dRow[i];
                        int nc = p.X + dCol[i];
                        if (nr < 0 || nr >= height || nc < 0 || nc >= width)
                            continue;
                        if (visited[nr, nc] || !IsWaterTile(map[nr, nc]))
                            continue;

                        visited[nr, nc] = true;
                        queue.Enqueue(new Point(nc, nr));
                    }
                }

                components.Add(new WaterComponent(minCol, maxCol, cells));
            }
        }

        return components;
    }

    private static int[] BuildTopProfile(WaterComponent component, int[,] map, int height)
    {
        int profileWidth = component.MaxCol - component.MinCol + 1;
        int[] topProfile = new int[profileWidth];
        Array.Fill(topProfile, -1);

        foreach (Point p in component.Cells)
        {
            int idx = p.X - component.MinCol;
            if (topProfile[idx] == -1 || p.Y < topProfile[idx])
                topProfile[idx] = p.Y;
        }

        return topProfile;
    }

    private static void ClampProfileSlope(int[] topProfile)
    {
        int prev = -1;
        for (int i = 0; i < topProfile.Length; i++)
        {
            if (topProfile[i] < 0) continue;
            if (prev == -1)
            {
                prev = i;
                continue;
            }

            topProfile[i] = Math.Clamp(topProfile[i], topProfile[prev] - 1, topProfile[prev] + 1);
            prev = i;
        }

        prev = -1;
        for (int i = topProfile.Length - 1; i >= 0; i--)
        {
            if (topProfile[i] < 0) continue;
            if (prev == -1)
            {
                prev = i;
                continue;
            }

            topProfile[i] = Math.Clamp(topProfile[i], topProfile[prev] - 1, topProfile[prev] + 1);
            prev = i;
        }
    }

    private static void RemoveOneColumnExtrema(int[] topProfile)
    {
        for (int i = 1; i < topProfile.Length - 1; i++)
        {
            if (topProfile[i - 1] < 0 || topProfile[i] < 0 || topProfile[i + 1] < 0)
                continue;
            if (topProfile[i - 1] == topProfile[i + 1] && topProfile[i] != topProfile[i - 1])
                topProfile[i] = topProfile[i - 1];
        }
    }

    private static void RemoveShortReversals(int[] topProfile)
    {
        var validCols = new List<int>();
        for (int i = 0; i < topProfile.Length; i++)
        {
            if (topProfile[i] >= 0)
                validCols.Add(i);
        }

        int lastNonZeroDirection = 0;
        int lastDirectionStart = 1;

        for (int i = 1; i < validCols.Count; i++)
        {
            int prevCol = validCols[i - 1];
            int col = validCols[i];
            int direction = Math.Sign(topProfile[col] - topProfile[prevCol]);

            if (direction == 0)
                continue;

            if (lastNonZeroDirection == 0)
            {
                lastNonZeroDirection = direction;
                lastDirectionStart = i;
                continue;
            }

            if (direction != lastNonZeroDirection)
            {
                int runLength = i - lastDirectionStart;
                if (runLength < 2)
                {
                    topProfile[col] = topProfile[prevCol];
                    continue;
                }

                lastNonZeroDirection = direction;
                lastDirectionStart = i;
            }
        }
    }

    private static void SmoothProfileStrict(int[] topProfile)
    {
        if (topProfile.Length < 3)
            return;

        ClampProfileSlope(topProfile);
        RemoveShortReversals(topProfile);
        ClampProfileSlope(topProfile);
        RemoveOneColumnExtrema(topProfile);
        RemoveOneColumnExtrema(topProfile);
    }

    private static void ApplySmoothedTopProfile(
        WaterComponent component,
        int[] original,
        int[] smoothed,
        int[,] map)
    {
        int height = map.GetLength(0);
        for (int localCol = 0; localCol < original.Length; localCol++)
        {
            int originalTop = original[localCol];
            int smoothedTop = smoothed[localCol];
            if (originalTop < 0 || smoothedTop < 0 || originalTop == smoothedTop)
                continue;

            int mapCol = component.MinCol + localCol;
            if (smoothedTop < originalTop)
            {
                for (int row = smoothedTop; row < originalTop; row++)
                {
                    if (row >= 0 && row < height)
                        map[row, mapCol] = PlainWaterTileId;
                }
            }
            else
            {
                for (int row = originalTop; row < smoothedTop; row++)
                {
                    if (row >= 0 && row < height)
                        map[row, mapCol] = SandTile1;
                }
            }
        }
    }

    private static void ApplyCoastAutotile(int[,] map, int width, int height)
    {
        bool[,] isWater = new bool[height, width];
        for (int r = 0; r < height; r++)
            for (int c = 0; c < width; c++)
                isWater[r, c] = IsWaterTile(map[r, c]);

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (!IsSandTile(map[row, col])) continue;

                bool wN  = row > 0 && isWater[row - 1, col];
                bool wS  = row < height - 1 && isWater[row + 1, col];
                bool wE  = col < width - 1 && isWater[row, col + 1];
                bool wW  = col > 0 && isWater[row, col - 1];
                bool wNE = row > 0 && col < width - 1 && isWater[row - 1, col + 1];
                bool wNW = row > 0 && col > 0 && isWater[row - 1, col - 1];
                bool wSE = row < height - 1 && col < width - 1 && isWater[row + 1, col + 1];
                bool wSW = row < height - 1 && col > 0 && isWater[row + 1, col - 1];

                int tile = -1;
                int cardinalWaterCount = (wN ? 1 : 0) + (wS ? 1 : 0) + (wE ? 1 : 0) + (wW ? 1 : 0);

                // Avoid 1-tile spikes and slivers by collapsing high-water sand to water.
                if (cardinalWaterCount >= 3)
                    tile = PlainWaterTileId;
                else if (cardinalWaterCount == 2)
                {
                    if (wS && wE) tile = CoastSE;
                    else if (wS && wW) tile = CoastSW;
                    else if (wN && wE) tile = CoastNE;
                    else if (wN && wW) tile = CoastNW;
                }
                else if (cardinalWaterCount == 1)
                {
                    if (wS) tile = CoastS;
                    else if (wN) tile = CoastN;
                    else if (wE) tile = CoastE;
                    else if (wW) tile = CoastW;
                }
                else
                {
                    int diagonalWaterCount = (wNE ? 1 : 0) + (wNW ? 1 : 0) + (wSE ? 1 : 0) + (wSW ? 1 : 0);

                    // Ambiguous diagonal-only neighborhoods are left as sand to avoid forced wrong corners.
                    if (diagonalWaterCount == 1)
                    {
                        if (wSE) tile = CoastInnerSE;
                        else if (wSW) tile = CoastInnerSW;
                        else if (wNE) tile = CoastInnerNE;
                        else if (wNW) tile = CoastInnerNW;
                    }
                }

                if (tile >= 0)
                    map[row, col] = tile;
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

        int baseOceanRow = height - 5;
        int minOceanRow = height - 9;
        int maxOceanRow = height - 2;
        int minBeachWidth = 3;
        int maxBeachWidth = 7;
        int baseBeachWidth = 5;
        int baseSandRow = baseOceanRow - baseBeachWidth;

        int[] sandEdge = new int[width];
        int[] oceanEdge = new int[width];

        float coastlineWalk = 0f;
        float beachWidthWalk = 0f;
        float coastPhaseA = (float)(rng.NextDouble() * Math.PI * 2.0);
        float coastPhaseB = (float)(rng.NextDouble() * Math.PI * 2.0);
        float beachPhase = (float)(rng.NextDouble() * Math.PI * 2.0);

        for (int col = 0; col < width; col++)
        {
            coastlineWalk += (float)(rng.NextDouble() * 2.0 - 1.0) * 0.35f;
            coastlineWalk = Math.Clamp(coastlineWalk, -3.5f, 3.5f);

            float coastLongWave = (float)Math.Sin(col * 0.09f + coastPhaseA) * 2.6f;
            float coastShortWave = (float)Math.Sin(col * 0.27f + coastPhaseB) * 0.8f;
            int oceanRow = baseOceanRow + (int)Math.Round(coastLongWave + coastShortWave + coastlineWalk);
            oceanEdge[col] = Math.Clamp(oceanRow, minOceanRow, maxOceanRow);

            beachWidthWalk += (float)(rng.NextDouble() * 2.0 - 1.0) * 0.4f;
            beachWidthWalk = Math.Clamp(beachWidthWalk, -2f, 2f);
            float beachWave = (float)Math.Sin(col * 0.16f + beachPhase) * 1.3f;
            int beachWidth = Math.Clamp(
                baseBeachWidth + (int)Math.Round(beachWidthWalk + beachWave),
                minBeachWidth,
                maxBeachWidth
            );
            sandEdge[col] = Math.Clamp(oceanEdge[col] - beachWidth, midRow + transitionRadius + 2, oceanEdge[col] - 1);
        }

        // Soften harsh column-to-column changes into broad shoreline arcs.
        for (int pass = 0; pass < 2; pass++)
        {
            int[] smoothed = new int[width];
            for (int col = 0; col < width; col++)
            {
                int left = oceanEdge[Math.Max(0, col - 1)];
                int center = oceanEdge[col];
                int right = oceanEdge[Math.Min(width - 1, col + 1)];
                smoothed[col] = (left + (center * 2) + right + 2) / 4;
            }
            oceanEdge = smoothed;
        }

        // Keep shoreline connected while preserving smooth gradients.
        for (int col = 1; col < width; col++)
        {
            oceanEdge[col] = Math.Clamp(oceanEdge[col], oceanEdge[col - 1] - 1, oceanEdge[col - 1] + 1);
        }
        for (int col = width - 2; col >= 0; col--)
        {
            oceanEdge[col] = Math.Clamp(oceanEdge[col], oceanEdge[col + 1] - 1, oceanEdge[col + 1] + 1);
        }

        // Enforce a minimum non-zero slope run length to reduce stair-step wedge repetition.
        int lastNonZeroDirection = 0;
        int lastDirectionStartCol = 1;
        for (int col = 1; col < width; col++)
        {
            int diff = oceanEdge[col] - oceanEdge[col - 1];
            int direction = Math.Sign(diff);

            if (direction == 0)
                continue;

            if (lastNonZeroDirection == 0)
            {
                lastNonZeroDirection = direction;
                lastDirectionStartCol = col;
                continue;
            }

            if (direction != lastNonZeroDirection)
            {
                int runLength = col - lastDirectionStartCol;
                if (runLength < 2)
                {
                    oceanEdge[col] = oceanEdge[col - 1];
                    continue;
                }

                lastNonZeroDirection = direction;
                lastDirectionStartCol = col;
            }
        }

        // Re-apply local continuity constraints after anti-stair adjustments.
        for (int col = 1; col < width; col++)
        {
            oceanEdge[col] = Math.Clamp(oceanEdge[col], oceanEdge[col - 1] - 1, oceanEdge[col - 1] + 1);
        }
        for (int col = width - 2; col >= 0; col--)
        {
            oceanEdge[col] = Math.Clamp(oceanEdge[col], oceanEdge[col + 1] - 1, oceanEdge[col + 1] + 1);
        }

        // Remove isolated one-column spikes/holes that create vertical water "pillars".
        for (int col = 1; col < width - 1; col++)
        {
            int left = oceanEdge[col - 1];
            int current = oceanEdge[col];
            int right = oceanEdge[col + 1];

            if (left == right && current != left)
                oceanEdge[col] = left;
        }
        for (int col = 1; col < width - 1; col++)
        {
            int left = oceanEdge[col - 1];
            int current = oceanEdge[col];
            int right = oceanEdge[col + 1];

            if (left == right && current != left)
                oceanEdge[col] = left;
        }

        for (int col = 0; col < width; col++)
        {
            sandEdge[col] = Math.Clamp(sandEdge[col], midRow + transitionRadius + 2, oceanEdge[col] - 1);
            for (int row = sandEdge[col]; row < height; row++)
            {
                if (row >= oceanEdge[col])
                {
                    if (row >= oceanEdge[col] + 2 && rng.NextDouble() < 0.25)
                        map[row, col] = rng.NextDouble() < 0.5 ? OceanWater1 : OceanWater3;
                    else
                        map[row, col] = PlainWaterTileId;
                }
                else
                {
                    bool nearSandTop = row <= sandEdge[col] + 1;
                    if (!nearSandTop || rng.NextDouble() > 0.35)
                        map[row, col] = weightedSand[rng.Next(weightedSand.Length)];
                }
            }
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

        SmoothAllWaterProfiles(map, width, height);
        ApplyCoastAutotile(map, width, height);

        return map;
    }
}
