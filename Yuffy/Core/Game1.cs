using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Yuffy.Gameplay;
using Yuffy.Rendering;
using Yuffy.UI;
using Yuffy.World;

namespace Yuffy.Core;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private readonly Random _rng = new();
    private readonly List<(float y, Action<SpriteBatch> draw)> _drawList = new();
    private readonly VirtualViewport _viewport = new();

    private RenderTarget2D _renderTarget;
    private Camera _camera;
    private Rectangle _destinationRect;

    private AnimatedSprite _player;
    private Animation _idleAnimation;
    private Animation _walkAnimation;
    private Animation _runAnimation;

    private AnimatedSprite _hair;
    private Animation _hairIdleAnimation;
    private Animation _hairWalkAnimation;
    private Animation _hairRunAnimation;

    private Animation _hurtAnimation;
    private Animation _hairHurtAnimation;
    private Animation _playerDeathAnimation;
    private Animation _hairDeathAnimation;
    private float _playerReactionTimer;

    private Tilemap _tilemap;

    private List<AnimalNpc> _animals;
    private List<TreeDecoration> _trees;
    private List<CropDecoration> _crops;
    private List<(Sprite sprite, Vector2 position)> _snowDecorations;

    private Vector2 _playerPosition;

    private const float MovementSpeed = 150f;
    private const float RunSpeed = 280f;

    private Texture2D _pixelTexture;
    private float _timeOfDay = 0.35f;
    private const float DayDurationSeconds = 90f;

    private Texture2D _skelIdleTex, _skelWalkTex, _skelAttackTex;
    private Texture2D _skelHurtTex, _skelDeathTex, _skelJumpTex;
    private List<SkeletonNpc> _skeletons;
    private bool _skeletonsSpawnedThisNight;
    private bool _skeletonsDyingTriggered;

    private Texture2D _gobIdleTex, _gobWalkTex, _gobAttackTex;
    private Texture2D _gobHurtTex, _gobDeathTex, _gobJumpTex;
    private List<GoblinNpc> _goblins;
    private bool _goblinsSpawnedThisNight;
    private bool _goblinsDyingTriggered;

    private Texture2D _heartTexture;
    private Texture2D _alertTexture;
    private Texture2D _deathIconTexture;
    private Texture2D _labelLeft, _labelMiddle, _labelRight;
    private int _playerHp = 5;
    private const int MaxPlayerHp = 5;
    private float _playerDamageCooldown;
    private float _deathFadeTimer;

    private Texture2D _cursorTexture;

    private Inventory _inventory;
    private InventoryUI _inventoryUI;
    private bool _inventoryOpen;
    private KeyboardState _previousKeyboardState;
    private ToolbarUI _toolbarUI;
    private SnowParticles _snowParticles;
    private bool _inSnowBiome;

    private SpriteFont _minecraftFont;
    private Texture2D _mushroomTexture;
    private List<Collectible> _collectibles;
    private MiniGameUI _miniGameUI;

    private Texture2D _indicatorTexture;
    private LetterUI _letterUI;
    private bool _letterPlaced;
    private bool _letterRead;
    private float _indicatorBob;

    private enum GameState { MainMenu, Playing, Paused }
    private GameState _gameState = GameState.MainMenu;
    private MainMenuUI _mainMenuUI;
    private PauseMenuUI _pauseMenuUI;
    private NineSliceBox _buttonBox;
    private MouseState _previousMouseState;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += (_, _) => RefreshViewportFromWindow();
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        RefreshViewportFromWindow();
        _renderTarget = new RenderTarget2D(GraphicsDevice, _viewport.Width, _viewport.Height);
        _camera = new Camera(_viewport);

        Texture2D idleTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/IDLE/base_idle_strip9");
        Texture2D walkTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/WALKING/base_walk_strip8");

        _idleAnimation = Animation.CreateFromSpriteStrip(idleTexture, 9, TimeSpan.FromSeconds(1.0 / 8.0));
        _walkAnimation = Animation.CreateFromSpriteStrip(walkTexture, 8, TimeSpan.FromSeconds(1.0 / 10.0));

        Texture2D hairIdleTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/IDLE/longhair_idle_strip9");
        Texture2D hairWalkTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/WALKING/longhair_walk_strip8");

        _hairIdleAnimation = Animation.CreateFromSpriteStrip(hairIdleTexture, 9, TimeSpan.FromSeconds(1.0 / 8.0));
        _hairWalkAnimation = Animation.CreateFromSpriteStrip(hairWalkTexture, 8, TimeSpan.FromSeconds(1.0 / 10.0));

        Texture2D runTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/RUN/base_run_strip8");
        Texture2D hairRunTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/RUN/longhair_run_strip8");

        _runAnimation = Animation.CreateFromSpriteStrip(runTexture, 8, TimeSpan.FromSeconds(1.0 / 12.0));
        _hairRunAnimation = Animation.CreateFromSpriteStrip(hairRunTexture, 8, TimeSpan.FromSeconds(1.0 / 12.0));

        Texture2D hurtTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/HURT/base_hurt_strip8");
        Texture2D hairHurtTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/HURT/longhair_hurt_strip8");
        _hurtAnimation = Animation.CreateFromSpriteStrip(hurtTexture, 8, TimeSpan.FromSeconds(1.0 / 10.0));
        _hairHurtAnimation = Animation.CreateFromSpriteStrip(hairHurtTexture, 8, TimeSpan.FromSeconds(1.0 / 10.0));

        Texture2D deathTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/DEATH/base_death_strip13");
        Texture2D hairDeathTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/DEATH/longhair_death_strip13");
        _playerDeathAnimation = Animation.CreateFromSpriteStrip(deathTexture, 13, TimeSpan.FromSeconds(1.0 / 8.0));
        _hairDeathAnimation = Animation.CreateFromSpriteStrip(hairDeathTexture, 13, TimeSpan.FromSeconds(1.0 / 8.0));

        _player = new AnimatedSprite(_idleAnimation);
        _player.Scale = new Vector2(3f, 3f);
        _player.CenterOrigin();

        _hair = new AnimatedSprite(_hairIdleAnimation);
        _hair.Scale = new Vector2(3f, 3f);
        _hair.CenterOrigin();

        Texture2D tilesetTexture = Content.Load<Texture2D>("images/tilesets/Tileset/spr_tileset_sunnysideworld_16px");
        int[,] mapData = Tilemap.CreateBiomeMap(120, 80);
        _tilemap = new Tilemap(tilesetTexture, mapData);
        _tilemap.Scale = 3f;

        Texture2D tree01Texture = Content.Load<Texture2D>("images/tilesets/Elements/Plants/spr_deco_tree_01_strip4");
        Texture2D tree02Texture = Content.Load<Texture2D>("images/tilesets/Elements/Plants/spr_deco_tree_02_strip4");

        Random treeRng = new Random(123);
        _trees = new List<TreeDecoration>();
        int treeCount = 30;
        int minSpacing = 3;

        for (int i = 0; i < treeCount; i++)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int col = treeRng.Next(2, _tilemap.MapWidth - 2);
                int row = treeRng.Next(2, _tilemap.MapHeight - 2);

                if (_tilemap.IsTileBlocked(col, row) || !_tilemap.IsGrassBiomeRow(row) || _tilemap.IsBeachRow(row))
                    continue;

                bool tooClose = false;
                foreach (var existing in _trees)
                {
                    int dx = Math.Abs(existing.BlockedTile.X - col);
                    int dy = Math.Abs(existing.BlockedTile.Y - row);
                    if (dx < minSpacing && dy < minSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                int scaledTile = _tilemap.ScaledTileSize;
                Vector2 treePos = new Vector2(
                    col * scaledTile + scaledTile / 2f,
                    (row + 1) * scaledTile
                );

                bool isConifer = treeRng.NextDouble() < 0.4;
                var texture = isConifer ? tree02Texture : tree01Texture;
                int frames = 4;

                var tree = new TreeDecoration(texture, frames, treePos, new Point(col, row), treeRng);
                _trees.Add(tree);
                _tilemap.BlockTile(col, row);
                break;
            }
        }

        Texture2D soilTexture = Content.Load<Texture2D>("images/tilesets/Elements/Crops/soil_00");

        // Crop tiles from the tileset - each crop type has 3 growth stages (rows 12, 13, 15)
        int[] cropCols = { 51, 52, 53, 54, 55, 58, 60 };
        int[] stageRows = { 12, 13, 15 };
        TextureRegion[][] cropStages = new TextureRegion[cropCols.Length][];
        for (int i = 0; i < cropCols.Length; i++)
        {
            cropStages[i] = new TextureRegion[stageRows.Length];
            for (int s = 0; s < stageRows.Length; s++)
            {
                int tx = cropCols[i] * 16;
                int ty = stageRows[s] * 16;
                cropStages[i][s] = new TextureRegion(tilesetTexture, new Rectangle(tx, ty, 16, 16));
            }
        }

        Random cropRng = new Random(456);
        _crops = new List<CropDecoration>();
        int patchCount = 7;
        int patchWidth = 5;
        int patchHeight = 5;
        var usedFarmTiles = new HashSet<Point>();

        for (int p = 0; p < patchCount; p++)
        {
            var stages = cropStages[cropRng.Next(cropStages.Length)];

            for (int attempt = 0; attempt < 100; attempt++)
            {
                int startCol = cropRng.Next(3, _tilemap.MapWidth - patchWidth - 3);
                int startRow = cropRng.Next(3, _tilemap.MapHeight - patchHeight - 3);

                bool areaFree = true;
                for (int dy = 0; dy < patchHeight && areaFree; dy++)
                {
                    for (int dx = 0; dx < patchWidth && areaFree; dx++)
                    {
                        int c = startCol + dx;
                        int r = startRow + dy;
                        if (_tilemap.IsTileBlocked(c, r) || usedFarmTiles.Contains(new Point(c, r)))
                            areaFree = false;
                    }
                }
                if (!areaFree) continue;

                bool inGrassBiome = true;
                for (int dy = 0; dy < patchHeight && inGrassBiome; dy++)
                {
                    if (!_tilemap.IsGrassBiomeRow(startRow + dy) || _tilemap.IsBeachRow(startRow + dy))
                        inGrassBiome = false;
                }
                if (!inGrassBiome) continue;

                for (int dy = 0; dy < patchHeight; dy++)
                {
                    bool isWaterRow = dy % 2 == 1;

                    for (int dx = 0; dx < patchWidth; dx++)
                    {
                        int col = startCol + dx;
                        int row = startRow + dy;
                        usedFarmTiles.Add(new Point(col, row));

                        if (isWaterRow)
                        {
                            int waterTile = cropRng.NextDouble() < 0.5 ? Tilemap.WaterTileId : Tilemap.PlainWaterTileId;
                            _tilemap.SetTileAt(col, row, waterTile);
                        }
                        else
                        {
                            _tilemap.SetTileAt(col, row, Tilemap.TileId(3, 1));
                            int scaledTile = _tilemap.ScaledTileSize;
                            Vector2 cropPos = new Vector2(
                                col * scaledTile + scaledTile / 2f,
                                row * scaledTile + scaledTile / 2f
                            );
                            var cropRegion = stages[cropRng.Next(stages.Length)];
                            _crops.Add(new CropDecoration(soilTexture, cropRegion, cropPos));
                        }
                    }
                }
                break;
            }
        }

        _playerPosition = FindSafeSpawnPosition(
            _tilemap.MapWidth / 2,
            (int)(_tilemap.MapHeight * 0.75f)
        );

        Texture2D cowTexture = Content.Load<Texture2D>("images/tilesets/Elements/Animals/spr_deco_cow_strip4");
        Texture2D chickenTexture = Content.Load<Texture2D>("images/tilesets/Elements/Animals/spr_deco_chicken_01_strip4");

        _animals = new List<AnimalNpc>();

        for (int i = 0; i < 6; i++)
        {
            var cow = new AnimalNpc(cowTexture, _tilemap, 30f, _rng);
            cow.SpawnAtRandomGrass();
            _animals.Add(cow);
        }

        for (int i = 0; i < 10; i++)
        {
            var chicken = new AnimalNpc(chickenTexture, _tilemap, 60f, _rng);
            chicken.SpawnAtRandomGrass();
            _animals.Add(chicken);
        }

        // Cut logs from top-middle of tileset (col 37)
        var verticalLogRegion = new TextureRegion(tilesetTexture, new Rectangle(592, 16, 16, 32));
        var stumpRegion = new TextureRegion(tilesetTexture, new Rectangle(592, 48, 16, 16));
        var cutWoodRegion = new TextureRegion(tilesetTexture, new Rectangle(624, 48, 16, 16));

        // Big rocks from right-middle of tileset (grey variant)
        var largeRockRegion = new TextureRegion(tilesetTexture, new Rectangle(784, 336, 32, 32));
        var mediumRockRegion = new TextureRegion(tilesetTexture, new Rectangle(816, 336, 32, 32));
        var smallRockRegion = new TextureRegion(tilesetTexture, new Rectangle(512, 64, 16, 16));

        Random snowRng = new Random(789);
        _snowDecorations = new List<(Sprite sprite, Vector2 position)>();
        int snowDecoCount = 50;
        int snowBiomeEnd = _tilemap.MapHeight / 2 - 5;

        for (int i = 0; i < snowDecoCount; i++)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int col = snowRng.Next(2, _tilemap.MapWidth - 2);
                int row = snowRng.Next(2, snowBiomeEnd);

                if (_tilemap.IsTileBlocked(col, row))
                    continue;

                int scaledTile = _tilemap.ScaledTileSize;
                Vector2 pos = new Vector2(
                    col * scaledTile + scaledTile / 2f,
                    row * scaledTile + scaledTile / 2f
                );

                double roll = snowRng.NextDouble();
                Sprite sprite;
                if (roll < 0.25)
                    sprite = new Sprite(largeRockRegion);
                else if (roll < 0.45)
                    sprite = new Sprite(mediumRockRegion);
                else if (roll < 0.55)
                    sprite = new Sprite(smallRockRegion);
                else if (roll < 0.75)
                    sprite = new Sprite(verticalLogRegion);
                else if (roll < 0.90)
                    sprite = new Sprite(stumpRegion);
                else
                    sprite = new Sprite(cutWoodRegion);

                sprite.Scale = new Vector2(3f, 3f);
                sprite.CenterOrigin();
                _snowDecorations.Add((sprite, pos));
                _tilemap.BlockTile(col, row);
                break;
            }
        }

        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        _skelIdleTex = Content.Load<Texture2D>("images/tilesets/Characters/Skeleton/PNG/skeleton_idle_strip6");
        _skelWalkTex = Content.Load<Texture2D>("images/tilesets/Characters/Skeleton/PNG/skeleton_walk_strip8");
        _skelAttackTex = Content.Load<Texture2D>("images/tilesets/Characters/Skeleton/PNG/skeleton_attack_strip7");
        _skelHurtTex = Content.Load<Texture2D>("images/tilesets/Characters/Skeleton/PNG/skeleton_hurt_strip7");
        _skelDeathTex = Content.Load<Texture2D>("images/tilesets/Characters/Skeleton/PNG/skeleton_death_strip10");
        _skelJumpTex = Content.Load<Texture2D>("images/tilesets/Characters/Skeleton/PNG/skeleton_jump_strip10");
        _skeletons = new List<SkeletonNpc>();

        _gobIdleTex = Content.Load<Texture2D>("images/tilesets/Characters/Goblin/PNG/spr_idle_strip9");
        _gobWalkTex = Content.Load<Texture2D>("images/tilesets/Characters/Goblin/PNG/spr_walk_strip8");
        _gobAttackTex = Content.Load<Texture2D>("images/tilesets/Characters/Goblin/PNG/spr_attack_strip10");
        _gobHurtTex = Content.Load<Texture2D>("images/tilesets/Characters/Goblin/PNG/spr_hurt_strip8");
        _gobDeathTex = Content.Load<Texture2D>("images/tilesets/Characters/Goblin/PNG/spr_death_strip13");
        _gobJumpTex = Content.Load<Texture2D>("images/tilesets/Characters/Goblin/PNG/spr_jump_strip9");
        _goblins = new List<GoblinNpc>();

        _heartTexture = Content.Load<Texture2D>("images/tilesets/UI/expression_love");
        _alertTexture = Content.Load<Texture2D>("images/tilesets/UI/expression_alerted");
        _deathIconTexture = Content.Load<Texture2D>("images/tilesets/UI/expression_attack");
        _cursorTexture = Content.Load<Texture2D>("images/tilesets/UI/cursor_01");

        // 9-slice inventory panel
        var nineSliceBox = new NineSliceBox(
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/dt_box_9slice_tl"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/dt_box_9slice_tc"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/dt_box_9slice_tr"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/dt_box_9slice_lc"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/dt_box_9slice_c"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/dt_box_9slice_rc"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/dt_box_9slice_bl"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/dt_box_9slice_bc"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/dt_box_9slice_br"),
            4);

        var itemDisc = Content.Load<Texture2D>("images/tilesets/UI/itemdisc_01");
        var itemTextures = new Dictionary<ItemType, Texture2D>();
        foreach (var kvp in ItemData.Catalog)
            itemTextures[kvp.Key] = Content.Load<Texture2D>(kvp.Value.TextureKey);

        _inventory = new Inventory();

        _minecraftFont = Content.Load<SpriteFont>("fonts/Minecraft");
        _labelLeft = Content.Load<Texture2D>("images/tilesets/UI/label_left");
        _labelMiddle = Content.Load<Texture2D>("images/tilesets/UI/label_middle");
        _labelRight = Content.Load<Texture2D>("images/tilesets/UI/label_right");

        _inventoryUI = new InventoryUI(nineSliceBox, _pixelTexture, itemTextures, _inventory, _viewport);
        _toolbarUI = new ToolbarUI(_pixelTexture, itemTextures, _inventory, _minecraftFont, _viewport);
        _snowParticles = new SnowParticles(_pixelTexture, _viewport);

        _indicatorTexture = Content.Load<Texture2D>("images/tilesets/UI/indicator");
        _letterUI = new LetterUI(nineSliceBox, _minecraftFont, _pixelTexture, _viewport);
        string letterPath = System.IO.Path.Combine(Content.RootDirectory, "letter.txt");
        try
        {
            _letterUI.Text = System.IO.File.ReadAllText(letterPath);
        }
        catch (System.IO.FileNotFoundException ex)
        {
            _letterUI.Text = "Letter content not found.";
            Console.WriteLine($"Letter file not found: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            _letterUI.Text = "Unable to access letter file due to permissions.";
            Console.WriteLine($"Unauthorized access to letter file: {ex.Message}");
        }
        catch (System.IO.IOException ex)
        {
            _letterUI.Text = "Error reading letter file.";
            Console.WriteLine($"I/O error reading letter file: {ex.Message}\n{ex.StackTrace}");
        }
        _mushroomTexture = Content.Load<Texture2D>("images/tilesets/UI/playercount");

        _collectibles = new List<Collectible>();
        for (int i = 0; i < 35; i++)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int col = _rng.Next(3, _tilemap.MapWidth - 3);
                int row = _rng.Next(3, _tilemap.MapHeight - 3);
                if (_tilemap.IsTileBlocked(col, row)) continue;
                int st = _tilemap.ScaledTileSize;
                var pos = new Vector2(col * st + st / 2f, row * st + st / 2f);
                _collectibles.Add(new Collectible(_mushroomTexture, pos));
                break;
            }
        }

        var sandTimerTex = Content.Load<Texture2D>("images/tilesets/UI/sandtimer");
        _miniGameUI = new MiniGameUI(_minecraftFont, _mushroomTexture, itemDisc,
            _labelLeft, _labelMiddle, _labelRight, sandTimerTex, _viewport);
        _miniGameUI.IsActive = true;

        // Light 9-slice for button backgrounds
        _buttonBox = new NineSliceBox(
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/lt_box_9slice_tl"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/lt_box_9slice_tc"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/lt_box_9slice_tr"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/lt_box_9slice_lc"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/lt_box_9slice_c"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/lt_box_9slice_rc"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/lt_box_9slice_bl"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/lt_box_9slice_bc"),
            Content.Load<Texture2D>("images/tilesets/UI/9slice_box_white/lt_box_9slice_br"),
            4);

        _mainMenuUI = new MainMenuUI(nineSliceBox, _buttonBox, _minecraftFont, _pixelTexture, _viewport);
        _pauseMenuUI = new PauseMenuUI(nineSliceBox, _buttonBox, _minecraftFont, _pixelTexture, _viewport);
    }

    private Vector2 GetVirtualMousePosition(MouseState mouseState)
    {
        if (_destinationRect.Width <= 0 || _destinationRect.Height <= 0)
            return Vector2.Zero;

        float retinaScale = (float)GraphicsDevice.Viewport.Width / Math.Max(1, Window.ClientBounds.Width);
        float logDestX = _destinationRect.X / retinaScale;
        float logDestW = _destinationRect.Width / retinaScale;
        float logDestY = _destinationRect.Y / retinaScale;
        float logDestH = _destinationRect.Height / retinaScale;
        float vx = (mouseState.X - logDestX) * _viewport.Width / logDestW;
        float vy = (mouseState.Y - logDestY) * _viewport.Height / logDestH;
        vx = MathHelper.Clamp(vx, 0, _viewport.Width - 1);
        vy = MathHelper.Clamp(vy, 0, _viewport.Height - 1);
        return new Vector2(vx, vy);
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState currentKeyState = Keyboard.GetState();
        var mouseState = Mouse.GetState();
        var virtualMouse = GetVirtualMousePosition(mouseState);

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            Exit();

        // Main menu state
        if (_gameState == GameState.MainMenu)
        {
            var action = _mainMenuUI.Update(virtualMouse, mouseState, _previousMouseState);
            if (action == MenuAction.Play)
                _gameState = GameState.Playing;
            else if (action == MenuAction.Exit)
                Exit();

            _previousKeyboardState = currentKeyState;
            _previousMouseState = mouseState;
            base.Update(gameTime);
            return;
        }

        // Paused state
        if (_gameState == GameState.Paused)
        {
            if (currentKeyState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
                _gameState = GameState.Playing;
            else
            {
                var action = _pauseMenuUI.Update(virtualMouse, mouseState, _previousMouseState);
                if (action == PauseAction.Resume)
                    _gameState = GameState.Playing;
                else if (action == PauseAction.ExitToMenu)
                    _gameState = GameState.MainMenu;
            }

            _previousKeyboardState = currentKeyState;
            _previousMouseState = mouseState;
            base.Update(gameTime);
            return;
        }

        // --- Playing state ---

        // Tab toggles inventory
        if (currentKeyState.IsKeyDown(Keys.Tab) && !_previousKeyboardState.IsKeyDown(Keys.Tab))
            _inventoryOpen = !_inventoryOpen;

        // E key: open/close letter
        if (currentKeyState.IsKeyDown(Keys.E) && !_previousKeyboardState.IsKeyDown(Keys.E))
        {
            if (_letterUI.IsOpen)
            {
                _letterUI.IsOpen = false;
                _letterRead = true;
            }
            else if (_letterPlaced && _toolbarUI.SelectedSlot == 0)
            {
                _letterUI.Open();
            }
        }

        // Escape closes inventory/letter first, then pauses
        if (currentKeyState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
        {
            if (_letterUI.IsOpen)
            {
                _letterUI.IsOpen = false;
                _letterRead = true;
            }
            else if (_inventoryOpen)
                _inventoryOpen = false;
            else
                _gameState = GameState.Paused;
        }

        // Death fade-out
        if (_deathFadeTimer > 0)
        {
            _deathFadeTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_deathFadeTimer <= 0)
            {
                _playerHp = MaxPlayerHp;
                _playerReactionTimer = 0;
                _playerPosition = FindSafeSpawnPosition(
                    _tilemap.MapWidth / 2,
                    (int)(_tilemap.MapHeight * 0.75f)
                );
            }
            _player.Update(gameTime);
            _hair.Update(gameTime);
            _previousKeyboardState = currentKeyState;
            base.Update(gameTime);
            return;
        }

        Vector2 direction = Vector2.Zero;

        if (!_inventoryOpen && !_letterUI.IsOpen)
        {
            if (currentKeyState.IsKeyDown(Keys.W) || currentKeyState.IsKeyDown(Keys.Up))
                direction.Y -= 1;
            if (currentKeyState.IsKeyDown(Keys.S) || currentKeyState.IsKeyDown(Keys.Down))
                direction.Y += 1;
            if (currentKeyState.IsKeyDown(Keys.A) || currentKeyState.IsKeyDown(Keys.Left))
                direction.X -= 1;
            if (currentKeyState.IsKeyDown(Keys.D) || currentKeyState.IsKeyDown(Keys.Right))
                direction.X += 1;
        }

        bool isMoving = direction != Vector2.Zero;
        bool isSprinting = currentKeyState.IsKeyDown(Keys.LeftShift) || currentKeyState.IsKeyDown(Keys.RightShift);

        // Decrement reaction timer
        if (_playerReactionTimer > 0)
            _playerReactionTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (isMoving)
        {
            direction.Normalize();
            float speed = isSprinting ? RunSpeed : MovementSpeed;
            Vector2 previousPosition = _playerPosition;
            _playerPosition += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            float halfTile = _tilemap.ScaledTileSize / 2f;
            int worldW = _tilemap.MapWidth * _tilemap.ScaledTileSize;
            int worldH = _tilemap.MapHeight * _tilemap.ScaledTileSize;
            _playerPosition.X = MathHelper.Clamp(_playerPosition.X, halfTile, worldW - halfTile);
            _playerPosition.Y = MathHelper.Clamp(_playerPosition.Y, halfTile, worldH - halfTile);

            float r = _tilemap.ScaledTileSize * 0.3f;
            if (IsTileBlockedAt(_playerPosition.X - r, _playerPosition.Y) ||
                IsTileBlockedAt(_playerPosition.X + r, _playerPosition.Y) ||
                IsTileBlockedAt(_playerPosition.X, _playerPosition.Y - r) ||
                IsTileBlockedAt(_playerPosition.X, _playerPosition.Y + r))
                _playerPosition = previousPosition;

            if (_playerReactionTimer <= 0)
            {
                _player.Animation = isSprinting ? _runAnimation : _walkAnimation;
                _hair.Animation = isSprinting ? _hairRunAnimation : _hairWalkAnimation;
            }

            if (direction.X < 0)
            {
                _player.Effects = SpriteEffects.FlipHorizontally;
                _hair.Effects = SpriteEffects.FlipHorizontally;
            }
            else if (direction.X > 0)
            {
                _player.Effects = SpriteEffects.None;
                _hair.Effects = SpriteEffects.None;
            }
        }
        else
        {
            if (_playerReactionTimer <= 0)
            {
                _player.Animation = _idleAnimation;
                _hair.Animation = _hairIdleAnimation;
            }
        }

        _player.Update(gameTime);
        _hair.Update(gameTime);

        foreach (var animal in _animals)
            animal.Update(gameTime);

        foreach (var tree in _trees)
            tree.Update(gameTime);

        _timeOfDay = (_timeOfDay + (float)gameTime.ElapsedGameTime.TotalSeconds / DayDurationSeconds) % 1.0f;

        // Skeleton spawn at dusk
        bool isNight = _timeOfDay >= 0.80f || _timeOfDay < 0.28f;
        if (isNight && !_skeletonsSpawnedThisNight)
        {
            _skeletonsSpawnedThisNight = true;
            _skeletonsDyingTriggered = false;
            int count = _rng.Next(10, 16);
            for (int i = 0; i < count; i++)
            {
                var skel = new SkeletonNpc(_skelIdleTex, _skelWalkTex, _skelAttackTex,
                    _skelHurtTex, _skelDeathTex, _skelJumpTex, _alertTexture, _tilemap, _rng);
                skel.SpawnAtRandom(_playerPosition);
                _skeletons.Add(skel);
            }
        }

        // Goblin spawn at dusk
        if (isNight && !_goblinsSpawnedThisNight)
        {
            _goblinsSpawnedThisNight = true;
            _goblinsDyingTriggered = false;
            int gobCount = _rng.Next(18, 25);
            for (int i = 0; i < gobCount; i++)
            {
                var gob = new GoblinNpc(_gobIdleTex, _gobWalkTex, _gobAttackTex,
                    _gobHurtTex, _gobDeathTex, _gobJumpTex, _alertTexture, _tilemap, _rng);
                gob.SpawnAtRandom(_playerPosition);
                _goblins.Add(gob);
            }
        }

        // Trigger death at sunrise
        if (_timeOfDay >= 0.28f && _timeOfDay < 0.40f && !_skeletonsDyingTriggered && _skeletonsSpawnedThisNight)
        {
            _skeletonsDyingTriggered = true;
            foreach (var skel in _skeletons)
                skel.TriggerDeath();
        }
        if (_timeOfDay >= 0.28f && _timeOfDay < 0.40f && !_goblinsDyingTriggered && _goblinsSpawnedThisNight)
        {
            _goblinsDyingTriggered = true;
            foreach (var gob in _goblins)
                gob.TriggerDeath();
        }

        // Reset spawn flag during daytime
        if (_timeOfDay >= 0.40f && _timeOfDay < 0.80f)
        {
            _skeletonsSpawnedThisNight = false;
            _skeletons.RemoveAll(s => s.IsDead);
            _goblinsSpawnedThisNight = false;
            _goblins.RemoveAll(g => g.IsDead);
        }

        // Update skeletons and handle damage
        if (_playerDamageCooldown > 0)
            _playerDamageCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var skel in _skeletons)
        {
            int dmg = skel.Update(gameTime, _playerPosition);
            if (dmg > 0 && _playerDamageCooldown <= 0)
            {
                _playerHp = Math.Max(0, _playerHp - dmg);
                _playerDamageCooldown = 1.0f;
                if (_playerHp > 0)
                {
                    _playerReactionTimer = 0.8f;
                    _player.Animation = _hurtAnimation;
                    _hair.Animation = _hairHurtAnimation;
                }
            }
        }
        _skeletons.RemoveAll(s => s.IsDead);

        foreach (var gob in _goblins)
        {
            int dmg = gob.Update(gameTime, _playerPosition);
            if (dmg > 0 && _playerDamageCooldown <= 0)
            {
                _playerHp = Math.Max(0, _playerHp - dmg);
                _playerDamageCooldown = 1.0f;
                if (_playerHp > 0)
                {
                    _playerReactionTimer = 0.8f;
                    _player.Animation = _hurtAnimation;
                    _hair.Animation = _hairHurtAnimation;
                }
            }
        }
        _goblins.RemoveAll(g => g.IsDead);

        // Player death: start fade-out with death animation
        if (_playerHp <= 0 && _deathFadeTimer <= 0)
        {
            _deathFadeTimer = 2.0f;
            _playerReactionTimer = 2.0f;
            _player.Animation = _playerDeathAnimation;
            _hair.Animation = _hairDeathAnimation;
        }

        // Update collectibles (magnet + collection)
        foreach (var c in _collectibles)
            c.Update((float)gameTime.ElapsedGameTime.TotalSeconds, _playerPosition);

        int collected = _collectibles.Count(c => c.Collected);
        _miniGameUI.Collected = collected;
        if (collected >= _miniGameUI.Target && !_miniGameUI.Won)
            _miniGameUI.TriggerWin();
        _miniGameUI.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

        // Place letter in toolbar when all mushrooms collected
        if (_miniGameUI.Won && !_letterPlaced)
        {
            _inventory.SetSlot(0, 0, ItemType.Letter, 1);
            _letterPlaced = true;
        }

        // Indicator bob animation
        _indicatorBob += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Letter scroll
        _letterUI.Update((float)gameTime.ElapsedGameTime.TotalSeconds, currentKeyState, Mouse.GetState());

        _inSnowBiome = !_tilemap.IsGrassBiomeRow(
            (int)(_playerPosition.Y / _tilemap.ScaledTileSize));
        if (_inSnowBiome)
            _snowParticles.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

        // Number keys 1-7 select toolbar slot
        for (int i = 0; i < 7; i++)
        {
            Keys numKey = Keys.D1 + i;
            if (currentKeyState.IsKeyDown(numKey) && !_previousKeyboardState.IsKeyDown(numKey))
                _toolbarUI.SelectedSlot = i;
        }

        _previousKeyboardState = currentKeyState;
        _previousMouseState = mouseState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Pass 1: render to virtual-resolution render target
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        // Main menu screen
        if (_gameState == GameState.MainMenu)
        {
            var virtualMouse = GetVirtualMousePosition(Mouse.GetState());
            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            _mainMenuUI.Draw(_spriteBatch, virtualMouse);
            _spriteBatch.End();

            DrawCursorAndPresent();
            base.Draw(gameTime);
            return;
        }

        int worldPixelWidth = _tilemap.MapWidth * _tilemap.ScaledTileSize;
        int worldPixelHeight = _tilemap.MapHeight * _tilemap.ScaledTileSize;
        _camera.Follow(_playerPosition, worldPixelWidth, worldPixelHeight);

        var visibleArea = new Rectangle(
            (int)_camera.Position.X,
            (int)_camera.Position.Y,
            _viewport.Width,
            _viewport.Height
        );

        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.GetTransformMatrix()
        );

        _tilemap.Draw(_spriteBatch, visibleArea);
        if (_inSnowBiome)
            _snowParticles.Draw(_spriteBatch, _camera.Position);

        foreach (var crop in _crops)
            crop.DrawSoil(_spriteBatch);

        foreach (var crop in _crops)
            crop.DrawCrop(_spriteBatch);

        _drawList.Clear();

        _drawList.Add((_playerPosition.Y, sb =>
        {
            _player.Draw(sb, _playerPosition);
            _hair.Draw(sb, _playerPosition);
        }));

        foreach (var animal in _animals)
        {
            var a = animal;
            _drawList.Add((a.Y, sb => a.Draw(sb)));
        }

        foreach (var tree in _trees)
        {
            var t = tree;
            _drawList.Add((t.Y, sb => t.Draw(sb)));
        }

        foreach (var (sprite, pos) in _snowDecorations)
        {
            var s = sprite;
            var p = pos;
            _drawList.Add((p.Y, sb => s.Draw(sb, p)));
        }

        foreach (var skel in _skeletons)
        {
            var sk = skel;
            _drawList.Add((sk.Y, sb => sk.Draw(sb)));
        }

        foreach (var gob in _goblins)
        {
            var g = gob;
            _drawList.Add((g.Y, sb => g.Draw(sb)));
        }

        foreach (var c in _collectibles)
        {
            var coll = c;
            if (!coll.Collected)
                _drawList.Add((coll.Y, sb => coll.Draw(sb)));
        }

        _drawList.Sort((a, b) => a.y.CompareTo(b.y));

        foreach (var entry in _drawList)
            entry.draw(_spriteBatch);

        _spriteBatch.End();

        // Day/night tint overlay
        Color tint = GetDayNightTint(_timeOfDay);
        if (tint.A > 0)
        {
            _spriteBatch.Begin(blendState: BlendState.AlphaBlend);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, _viewport.Width, _viewport.Height), tint);
            _spriteBatch.End();
        }

        // Death screen overlay
        if (_deathFadeTimer > 0)
        {
            float alpha = MathHelper.Clamp(_deathFadeTimer / 2.0f, 0f, 1f);
            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, _viewport.Width, _viewport.Height),
                new Color(0, 0, 0, (int)(alpha * 200)));
            float iconScale = 4f;
            int iconW = (int)(_deathIconTexture.Width * iconScale);
            int iconH = (int)(_deathIconTexture.Height * iconScale);
            _spriteBatch.Draw(_deathIconTexture,
                new Rectangle(_viewport.Width / 2 - iconW / 2, _viewport.Height / 2 - iconH / 2, iconW, iconH),
                new Color(255, 255, 255, (int)(alpha * 255)));
            _spriteBatch.End();
        }

        // Draw hearts HP
        {
            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            int heartScale = 3;
            int heartW = _heartTexture.Width * heartScale;
            int heartH = _heartTexture.Height * heartScale;
            int heartPadding = 4;
            int heartStartX = 10;
            int heartStartY = 10;

            bool heartsVisible = _playerDamageCooldown <= 0 || (int)(_playerDamageCooldown * 10) % 2 == 0;

            for (int i = 0; i < MaxPlayerHp; i++)
            {
                var heartRect = new Rectangle(heartStartX + i * (heartW + heartPadding), heartStartY, heartW, heartH);
                if (i < _playerHp)
                {
                    // Full heart (blink when damaged)
                    if (heartsVisible)
                        _spriteBatch.Draw(_heartTexture, heartRect, Color.White);
                }
                else
                {
                    // Empty heart silhouette
                    _spriteBatch.Draw(_heartTexture, heartRect, new Color(50, 50, 50, 120));
                }
            }
            _spriteBatch.End();
        }

        // Toolbar hotbar (always visible)
        _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
        _toolbarUI.Draw(_spriteBatch);

        // Bouncing indicator above toolbar slot 0 when letter placed
        if (_letterPlaced && !_letterRead)
        {
            float bob = MathF.Sin(_indicatorBob * 4f) * 6f;
            float indicatorScale = 3f;
            int slotCenterX = _toolbarUI.GetSlotCenterX(0);
            int barTopY = _toolbarUI.GetBarTopY();
            int indW = (int)(_indicatorTexture.Width * indicatorScale);
            int indH = (int)(_indicatorTexture.Height * indicatorScale);
            int indX = slotCenterX - indW / 2;
            int indY = (int)(barTopY - indH - 4 + bob);
            _spriteBatch.Draw(_indicatorTexture, new Rectangle(indX, indY, indW, indH), Color.White);
        }
        _spriteBatch.End();

        // Mini-game HUD
        _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
        _miniGameUI.Draw(_spriteBatch);
        _spriteBatch.End();

        // Letter overlay
        if (_letterUI.IsOpen)
        {
            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            _letterUI.Draw(_spriteBatch);
            _spriteBatch.End();
        }

        // Inventory UI
        if (_inventoryOpen)
        {
            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, _viewport.Width, _viewport.Height),
                new Color(0, 0, 0, 120));
            _inventoryUI.Draw(_spriteBatch);
            _spriteBatch.End();
        }

        // Pause menu overlay
        if (_gameState == GameState.Paused)
        {
            var virtualMouse = GetVirtualMousePosition(Mouse.GetState());
            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            _pauseMenuUI.Draw(_spriteBatch, virtualMouse);
            _spriteBatch.End();
        }

        DrawCursorAndPresent();
        base.Draw(gameTime);
    }

    private void DrawCursorAndPresent()
    {
        // Custom cursor
        var virtualMouse = GetVirtualMousePosition(Mouse.GetState());
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_cursorTexture, virtualMouse, null,
            Color.White, 0f, Vector2.Zero, 1.875f, SpriteEffects.None, 0f);
        _spriteBatch.End();

        // Pass 2: draw render target scaled to window with letterboxing
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_renderTarget, _destinationRect, Color.White);
        _spriteBatch.End();
    }

    private void RefreshViewportFromWindow()
    {
        int logicalW = Window.ClientBounds.Width;
        int logicalH = Window.ClientBounds.Height;
        _viewport.Refresh(logicalW, logicalH);

        if (_viewport.SizeChanged)
        {
            _renderTarget?.Dispose();
            _renderTarget = new RenderTarget2D(GraphicsDevice, _viewport.Width, _viewport.Height);
            _viewport.SizeChanged = false;
        }

        // Blit rect in backbuffer pixel space
        int bbW = GraphicsDevice.Viewport.Width;
        int bbH = GraphicsDevice.Viewport.Height;
        float scale = Math.Min((float)bbW / _viewport.Width, (float)bbH / _viewport.Height);
        int sw = (int)(_viewport.Width * scale);
        int sh = (int)(_viewport.Height * scale);
        _destinationRect = new Rectangle((bbW - sw) / 2, (bbH - sh) / 2, sw, sh);
    }

    private Vector2 FindSafeSpawnPosition(int preferredCol, int preferredRow)
    {
        int scaledTile = _tilemap.ScaledTileSize;

        if (!_tilemap.IsTileBlocked(preferredCol, preferredRow))
            return new Vector2(
                preferredCol * scaledTile + scaledTile / 2f,
                preferredRow * scaledTile + scaledTile / 2f
            );

        for (int radius = 1; radius < Math.Max(_tilemap.MapWidth, _tilemap.MapHeight); radius++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (Math.Abs(dx) != radius && Math.Abs(dy) != radius) continue;
                    int col = preferredCol + dx;
                    int row = preferredRow + dy;
                    if (!_tilemap.IsTileBlocked(col, row))
                        return new Vector2(
                            col * scaledTile + scaledTile / 2f,
                            row * scaledTile + scaledTile / 2f
                        );
                }
            }
        }

        return new Vector2(scaledTile / 2f, scaledTile / 2f);
    }

    private bool IsTileBlockedAt(float x, float y)
    {
        int col = (int)(x / _tilemap.ScaledTileSize);
        int row = (int)(y / _tilemap.ScaledTileSize);
        return _tilemap.IsTileBlocked(col, row);
    }

    private static Color GetDayNightTint(float time)
    {
        // Keyframes: (time, R, G, B, alpha)
        (float t, int r, int g, int b, float a)[] keys =
        {
            (0.00f, 15, 10, 40, 0.65f),
            (0.20f, 15, 10, 40, 0.40f),
            (0.30f, 0, 0, 0, 0.00f),
            (0.70f, 0, 0, 0, 0.00f),
            (0.80f, 15, 10, 40, 0.30f),
            (0.90f, 15, 10, 40, 0.55f),
            (1.00f, 15, 10, 40, 0.65f),
        };

        // Find the two keyframes surrounding current time
        int i = 0;
        for (; i < keys.Length - 1; i++)
        {
            if (time < keys[i + 1].t)
                break;
        }

        var k0 = keys[i];
        var k1 = keys[Math.Min(i + 1, keys.Length - 1)];
        float span = k1.t - k0.t;
        float lerp = span > 0 ? (time - k0.t) / span : 0f;

        int cr = (int)MathHelper.Lerp(k0.r, k1.r, lerp);
        int cg = (int)MathHelper.Lerp(k0.g, k1.g, lerp);
        int cb = (int)MathHelper.Lerp(k0.b, k1.b, lerp);
        int ca = (int)(MathHelper.Lerp(k0.a, k1.a, lerp) * 255);

        return new Color(cr, cg, cb, ca);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pixelTexture?.Dispose();
            _renderTarget?.Dispose();
        }

        base.Dispose(disposing);
    }
}
