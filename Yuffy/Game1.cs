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
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

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

        _playerPosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2f,
            _graphics.PreferredBackBufferHeight / 2f
        );

        Texture2D tilesetTexture = Content.Load<Texture2D>("images/tilesets/Tileset/spr_tileset_sunnysideworld_16px");
        int[,] mapData = Tilemap.CreateGrassWithPondMap(20, 12);
        _tilemap = new Tilemap(tilesetTexture, mapData);
        _tilemap.Scale = 3f;

        Texture2D cowTexture = Content.Load<Texture2D>("images/tilesets/Elements/Animals/spr_deco_cow_strip4");
        Texture2D chickenTexture = Content.Load<Texture2D>("images/tilesets/Elements/Animals/spr_deco_chicken_01_strip4");

        Random animalRng = new Random();
        _animals = new List<AnimalNpc>();

        for (int i = 0; i < 2; i++)
        {
            var cow = new AnimalNpc(cowTexture, _tilemap, 30f, animalRng);
            cow.SpawnAtRandomGrass();
            _animals.Add(cow);
        }

        for (int i = 0; i < 3; i++)
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
            _playerPosition += direction * MovementSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
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
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _tilemap.Draw(_spriteBatch);

        foreach (var animal in _animals)
            animal.Draw(_spriteBatch);

        _player.Draw(_spriteBatch, _playerPosition);
        _hair.Draw(_spriteBatch, _playerPosition);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
