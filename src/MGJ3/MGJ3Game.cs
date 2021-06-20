using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using tainicom.Devices;
using tainicom.PageManager;
using MGJ3.Pages;
using MGJ3.Components;

namespace MGJ3
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MGJ3Game : Game
    {
        GraphicsDeviceManager graphics;

        PageManager pageManager;
        InputState inputState = new InputState();
        FpsComponent _fpsComponent;

#if CARDBOARD
        public static Microsoft.Xna.Framework.Input.Cardboard.EyeState VrEye { get; private set; }
#endif

        public MGJ3Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            graphics.PreparingDeviceSettings += (sender, e) =>
            {
                // unlock the 30 fps limit
                e.GraphicsDeviceInformation.PresentationParameters.PresentationInterval = PresentInterval.One;

                // use HiDef profile
                if (e.GraphicsDeviceInformation.Adapter.IsProfileSupported(GraphicsProfile.HiDef))
                    e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
            };

            Window.AllowUserResizing = true;
            graphics.HardwareModeSwitch = false;

#if ANDROID || CARDBOARD || !DEBUG
#if WINDOWS || WUP || DESKTOPGL
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
#endif
            graphics.IsFullScreen = true;
#endif
#if CARDBOARD
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
#endif

            // Test Cardboard view
            //graphics.PreferredBackBufferWidth = 400;
            //graphics.PreferredBackBufferHeight = 450;

            // Frame rate
            TargetElapsedTime = TimeSpan.FromTicks(166666);
            //TargetElapsedTime = TimeSpan.FromTicks( 83333); // run update at 120Hz, allow monitors with > 60Hz refresh rate.
            IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = true;

            IsMouseVisible = true;


            pageManager = new PageManager(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        bool isInitialized = false;
        protected override void Initialize()
        {
            if (isInitialized) return; // bug in Cardboard platform
            isInitialized = true;

            Components.Add(pageManager);

            _fpsComponent = new FpsComponent(this);
            _fpsComponent.DrawOrder = 1000;
            Components.Add(_fpsComponent);

            base.Initialize();

            IntroPage intro = new IntroPage(pageManager);
            pageManager.SideloadPage(intro);
            intro.Initialize();
            pageManager.AddPage(intro);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            inputState.Update(this.IsActive);

            // toggle FullScreen
#if (WINDOWS || WUP || DESKTOPGL)
            if (inputState.IsKeyReleased(Keys.F11))
            {
                if (!graphics.IsFullScreen)
                {
                    graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    graphics.IsFullScreen = true;
                    graphics.ApplyChanges();
                }
                else
                {
                    graphics.PreferredBackBufferWidth = 800;
                    graphics.PreferredBackBufferHeight = 480;
                    graphics.IsFullScreen = false;
                    graphics.ApplyChanges();
                }
            }
#endif

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            pageManager.PreDraw(gameTime);

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

#if CARDBOARD
            var vrstate = Microsoft.Xna.Framework.Input.Cardboard.Headset.GetState();
            // draw left eye
            VrEye = vrstate.LeftEye;
            GraphicsDevice.Viewport = VrEye.Viewport;
            base.Draw(gameTime);
            // draw right eye
            VrEye = vrstate.RightEye;
            GraphicsDevice.Viewport = VrEye.Viewport;
            base.Draw(gameTime);
#else
            base.Draw(gameTime);
#endif
        }
    }
}
