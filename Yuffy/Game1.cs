using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Yuffy.Graphics;

namespace Yuffy;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private const int VirtualWidth = 960;
    private const int VirtualHeight = 540;

    private RenderTarget2D _renderTarget;
    private Camera _camera;
    private Rectangle _destinationRect;

    private AnimatedSprite _player;
    private Animation _idleAnimation;
    private Animation _walkAnimation;

    private AnimatedSprite _hair;
    private Animation _hairIdleAnimation;
    private Animation _hairWalkAnimation;

    private Tilemap _tilemap;

    private List<AnimalNpc> _animals;

    private Vector2 _playerPosition;

    private const float MovementSpeed = 150f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderTarget = new RenderTarget2D(GraphicsDevice, VirtualWidth, VirtualHeight);
        _camera = new Camera(VirtualWidth, VirtualHeight);
        UpdateDestinationRect();

        Texture2D idleTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/IDLE/base_idle_strip9");
        Texture2D walkTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/WALKING/base_walk_strip8");

        _idleAnimation = Animation.CreateFromSpriteStrip(idleTexture, 9, TimeSpan.FromSeconds(1.0 / 8.0));
        _walkAnimation = Animation.CreateFromSpriteStrip(walkTexture, 8, TimeSpan.FromSeconds(1.0 / 10.0));

        Texture2D hairIdleTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/IDLE/longhair_idle_strip9");
        Texture2D hairWalkTexture = Content.Load<Texture2D>("images/tilesets/Characters/Human/WALKING/longhair_walk_strip8");

        _hairIdleAnimation = Animation.CreateFromSpriteStrip(hairIdleTexture, 9, TimeSpan.FromSeconds(1.0 / 8.0));
        _hairWalkAnimation = Animation.CreateFromSpriteStrip(hairWalkTexture, 8, TimeSpan.FromSeconds(1.0 / 10.0));

        _player = new AnimatedSprite(_idleAnimation);
        _player.Scale = new Vector2(3f, 3f);
        _player.CenterOrigin();

        _hair = new AnimatedSprite(_hairIdleAnimation);
        _hair.Scale = new Vector2(3f, 3f);
        _hair.CenterOrigin();

        Texture2D tilesetTexture = Content.Load<Texture2D>("images/tilesets/Tileset/spr_tileset_sunnysideworld_16px");
        int[,] mapData = Tilemap.CreateGrassWithPondMap(60, 40);
        _tilemap = new Tilemap(tilesetTexture, mapData);
        _tilemap.Scale = 3f;

        _playerPosition = new Vector2(
            _tilemap.MapWidth * _tilemap.ScaledTileSize / 2f,
            _tilemap.MapHeight * _tilemap.ScaledTileSize / 2f
        );

        Texture2D cowTexture = Content.Load<Texture2D>("images/tilesets/Elements/Animals/spr_deco_cow_strip4");
        Texture2D chickenTexture = Content.Load<Texture2D>("images/tilesets/Elements/Animals/spr_deco_chicken_01_strip4");

        Random animalRng = new Random();
        _animals = new List<AnimalNpc>();

        for (int i = 0; i < 6; i++)
        {
            var cow = new AnimalNpc(cowTexture, _tilemap, 30f, animalRng);
            cow.SpawnAtRandomGrass();
            _animals.Add(cow);
        }

        for (int i = 0; i < 10; i++)
        {
            var chicken = new AnimalNpc(chickenTexture, _tilemap, 60f, animalRng);
            chicken.SpawnAtRandomGrass();
            _animals.Add(chicken);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        KeyboardState keyboardState = Keyboard.GetState();
        Vector2 direction = Vector2.Zero;

        if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
            direction.Y -= 1;
        if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
            direction.Y += 1;
        if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
            direction.X -= 1;
        if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            direction.X += 1;

        bool isMoving = direction != Vector2.Zero;

        if (isMoving)
        {
            direction.Normalize();
            Vector2 previousPosition = _playerPosition;
            _playerPosition += direction * MovementSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            float halfTile = _tilemap.ScaledTileSize / 2f;
            int worldW = _tilemap.MapWidth * _tilemap.ScaledTileSize;
            int worldH = _tilemap.MapHeight * _tilemap.ScaledTileSize;
            _playerPosition.X = MathHelper.Clamp(_playerPosition.X, halfTile, worldW - halfTile);
            _playerPosition.Y = MathHelper.Clamp(_playerPosition.Y, halfTile, worldH - halfTile);

            float r = _tilemap.ScaledTileSize * 0.3f;
            if (IsWaterAt(_playerPosition.X - r, _playerPosition.Y) ||
                IsWaterAt(_playerPosition.X + r, _playerPosition.Y) ||
                IsWaterAt(_playerPosition.X, _playerPosition.Y - r) ||
                IsWaterAt(_playerPosition.X, _playerPosition.Y + r))
                _playerPosition = previousPosition;

            _player.Animation = _walkAnimation;
            _hair.Animation = _hairWalkAnimation;

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
            _player.Animation = _idleAnimation;
            _hair.Animation = _hairIdleAnimation;
        }

        _player.Update(gameTime);
        _hair.Update(gameTime);

        foreach (var animal in _animals)
            animal.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        int worldPixelWidth = _tilemap.MapWidth * _tilemap.ScaledTileSize;
        int worldPixelHeight = _tilemap.MapHeight * _tilemap.ScaledTileSize;
        _camera.Follow(_playerPosition, worldPixelWidth, worldPixelHeight);

        // Pass 1: render world to virtual-resolution render target
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        var visibleArea = new Rectangle(
            (int)_camera.Position.X,
            (int)_camera.Position.Y,
            VirtualWidth,
            VirtualHeight
        );

        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.GetTransformMatrix()
        );

        _tilemap.Draw(_spriteBatch, visibleArea);

        _animals.Sort((a, b) => a.Y.CompareTo(b.Y));

        bool playerDrawn = false;
        foreach (var animal in _animals)
        {
            if (!playerDrawn && _playerPosition.Y < animal.Y)
            {
                _player.Draw(_spriteBatch, _playerPosition);
                _hair.Draw(_spriteBatch, _playerPosition);
                playerDrawn = true;
            }
            animal.Draw(_spriteBatch);
        }
        if (!playerDrawn)
        {
            _player.Draw(_spriteBatch, _playerPosition);
            _hair.Draw(_spriteBatch, _playerPosition);
        }

        _spriteBatch.End();

        // Pass 2: draw render target scaled to window with letterboxing
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_renderTarget, _destinationRect, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        UpdateDestinationRect();
    }

    private void UpdateDestinationRect()
    {
        int windowWidth = GraphicsDevice.Viewport.Width;
        int windowHeight = GraphicsDevice.Viewport.Height;

        float scaleX = (float)windowWidth / VirtualWidth;
        float scaleY = (float)windowHeight / VirtualHeight;
        float scale = Math.Min(scaleX, scaleY);

        int scaledWidth = (int)(VirtualWidth * scale);
        int scaledHeight = (int)(VirtualHeight * scale);

        int offsetX = (windowWidth - scaledWidth) / 2;
        int offsetY = (windowHeight - scaledHeight) / 2;

        _destinationRect = new Rectangle(offsetX, offsetY, scaledWidth, scaledHeight);
    }

    private bool IsWaterAt(float x, float y)
    {
        int col = (int)(x / _tilemap.ScaledTileSize);
        int row = (int)(y / _tilemap.ScaledTileSize);
        return _tilemap.GetTileAt(col, row) == Tilemap.WaterTileId;
    }
}
