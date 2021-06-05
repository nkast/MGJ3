using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.PageManager;

namespace MGJ3.Pages
{
    internal abstract class BasicPage : PageBase
    {
        private BasicEffect _uiEffect;
        private float _virtualWidth;
        private float _virtualHeight;
        private Matrix _uiWorld = Matrix.Identity;
        private Matrix _uiView = Matrix.Identity;
        private Matrix _uiProj = Matrix.Identity;

        protected Effect UiEffect { get { return _uiEffect; } }

        public BasicPage(PageManager pageManager) : base(pageManager)
        {
            SetVirtualResolution(800, 480);
        }

        public override void Initialize()
        {
            base.Initialize();

            _uiEffect = new BasicEffect(this.Game.GraphicsDevice);
            _uiEffect.TextureEnabled = true;
            _uiEffect.VertexColorEnabled = true;

        }

        public override void UpdateLayout()
        {
            base.UpdateLayout();
            UpdateUIEffect();
        }

        protected void SetVirtualResolution(float width, float height)
        {
            _virtualWidth = width;
            _virtualHeight = height;
        }


        protected float GetViewWidth()
        {
            bool isLandscape = (Game.Window.CurrentOrientation == DisplayOrientation.LandscapeLeft)
                              || (Game.Window.CurrentOrientation == DisplayOrientation.LandscapeRight)
                              || (Game.Window.CurrentOrientation == DisplayOrientation.Default) // MonoGame bug?
                              ;
            float aspectRatio = Game.GraphicsDevice.Viewport.AspectRatio;
            float virtualWidth = (isLandscape) ? _virtualWidth : _virtualHeight;
            float virtualHeight = (isLandscape) ? _virtualHeight : _virtualWidth;
            bool fitWidth = ((virtualWidth / virtualHeight) >= aspectRatio);
            return (fitWidth) ? (virtualWidth) : (virtualHeight * aspectRatio);
        }

        protected float GetViewHeight()
        {
            bool isLandscape = (Game.Window.CurrentOrientation == DisplayOrientation.LandscapeLeft)
                            || (Game.Window.CurrentOrientation == DisplayOrientation.LandscapeRight)
                            || (Game.Window.CurrentOrientation == DisplayOrientation.Default); // MonoGame bug?
            float aspectRatio = Game.GraphicsDevice.Viewport.AspectRatio;
            float virtualWidth = (isLandscape) ? _virtualWidth : _virtualHeight;
            float virtualHeight = (isLandscape) ? _virtualHeight : _virtualWidth;
            bool fitWidth = ((virtualWidth / virtualHeight) >= aspectRatio);
            return (fitWidth) ? (virtualWidth / aspectRatio) : (virtualHeight);
        }


        protected void UpdateMenuEffectProjection()
        {
            // set _menuEffect same as base.UiEffect
            UpdateUIEffect();


            // set _uiEffect from camera for VR menus
#if CARDBOARD
            if (UiEffect == null)
                return;
            float viewWidth = GetViewWidth();
            float viewHeight = GetViewHeight();
            float uicameraBack = 32f;
            var c = 0.1f;
            var eyepos = -MGJ3Game.VrEye.View.Translation.X;
            //var eyepos = -0.03f;
            eyepos = eyepos / 10f;

            var uiEffect = this.UiEffect as IEffectMatrices;
            uiEffect.View = Matrix.CreateLookAt(
                                new Vector3(eyepos, 0.00f, 0.00f) + Vector3.Backward * uicameraBack, 
                                Vector3.Forward * 1f, 
                                Vector3.Up);

            uiEffect.Projection = Matrix.CreatePerspectiveOffCenter(
                0, viewWidth, viewHeight, 0, uicameraBack - c, uicameraBack + 1f + c);
            uiEffect.Projection = uiEffect.Projection * Matrix.CreateScale(1f, 1f, -1f);
#endif
        }


        private void UpdateUIEffect()
        {            
            if (_uiEffect == null)
                return;

            _uiWorld = Matrix.Identity;
            _uiView = Matrix.Identity;

            // set projection for SpriteBatch
            float viewWidth = GetViewWidth();
            float viewHeight = GetViewHeight();

            _uiProj = Matrix.CreateOrthographicOffCenter(0, viewWidth, viewHeight, 0, 0, -1);
#if XNA // if (NeedsHalfPixelOffset)
                prj.M41 += -0.5f * prj.M11;
                prj.M42 += -0.5f * prj.M22;
#endif

            _uiEffect.World = _uiWorld;
            _uiEffect.View = _uiView;
            _uiEffect.Projection = _uiProj;
        }
    }
}
