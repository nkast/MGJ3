﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input.XR;
using Microsoft.Xna.Framework.XR;
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

        internal XRDevice _xrDevice;
        HandsState _ovrHandsState;

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
                if (e.GraphicsDeviceInformation.GraphicsProfile < GraphicsProfile.HiDef && e.GraphicsDeviceInformation.Adapter.IsProfileSupported(GraphicsProfile.HiDef))
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

#if OVR
            // 90Hz Frame rate for oculus
            TargetElapsedTime = TimeSpan.FromTicks(111111);
            IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = false;

            // we don't care is the main window is Focuses or not
            // because we render on the Oculus surface.
            InactiveSleepTime = TimeSpan.FromSeconds(0);

            // OVR requirees at least DX feature level 11.0
            graphics.GraphicsProfile = GraphicsProfile.FL11_0;

#endif

            // create XR device
            _xrDevice = new XRDevice("MGJ3-VR", this);
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
#if (WINDOWS || OVR || WUP || DESKTOPGL)
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

#if OVR || CARDBOARD
            if (_xrDevice.DeviceState == XRDeviceState.Disabled)
            {
                try
                {
                    // Initialize XR
                    int ovrCreateResult = _xrDevice.BeginSessionAsync();
                }
                catch (Exception ovre)
                {
                    System.Diagnostics.Debug.WriteLine(ovre.Message);
                }
            }
#endif

            if (_xrDevice.DeviceState == XRDeviceState.Enabled)
            {
                 _ovrHandsState = _xrDevice.GetHandsState();
            }

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

            if (_xrDevice.DeviceState == XRDeviceState.Enabled)
            {
                var ovrResult = _xrDevice.BeginFrame();
                if (ovrResult >= 0)
                {
                    HeadsetState ovrHeadsetState = _xrDevice.GetHeadsetState();

                    // draw each eye on a rendertarget
                    foreach (XREye eye in _xrDevice.GetEyes())
                    {
                        RenderTarget2D ovrrt = _xrDevice.GetEyeRenderTarget(eye);
                        GraphicsDevice.SetRenderTarget(ovrrt);
                        GraphicsDevice.Clear(Color.Black);

                        Matrix ovrProj = _xrDevice.CreateProjection(eye, 0.001f, 10f);
                        Matrix ovrView = ovrHeadsetState.GetEyeView(eye);
                        // TODO: set cameras and SpriteBatch matrix with eye's view/proj 

                        // draw any drawable components
                        base.Draw(gameTime);

                        // Resolve eye rendertarget
                        GraphicsDevice.SetRenderTarget(null);
                        // submit eye rendertarget
                        _xrDevice.CommitRenderTarget(eye, ovrrt);
                    }

                    // submit frame
                    int result = _xrDevice.EndFrame();

                    // draw on backbaffer
                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(Color.Black);

                    // preview rendertargets
                    var pp = GraphicsDevice.PresentationParameters;
                    int height = pp.BackBufferHeight;
                    float aspectRatio = (float)_xrDevice.GetEyeRenderTarget(XREye.Left).Width / _xrDevice.GetEyeRenderTarget(XREye.Left).Height;

                    int width = Math.Min(pp.BackBufferWidth, (int)(height * aspectRatio));
                    pageManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                    pageManager.SpriteBatch.Draw(_xrDevice.GetEyeRenderTarget(XREye.Left), new Rectangle(0, 0, width, height), Color.White);
                    pageManager.SpriteBatch.Draw(_xrDevice.GetEyeRenderTarget(XREye.Right), new Rectangle(width, 0, width, height), Color.White);
                    pageManager.SpriteBatch.End();

                    return;
                }
            }

            base.Draw(gameTime);
        }
    }
}
