using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Devices;
using tainicom.PageManager;
using tainicom.PageManager.Enums;
using tainicom.PageManager.Events;
using tainicom.Tweens;
using MGJ3.Pages.MenuPages;

namespace MGJ3.Pages.GamePages
{
    internal class GameStartPage : BasicPage
    {
        ContentManager content;
        Random rnd = new Random();

        GameContext _gameContext;

        public GameStartPage(PageManager pageManager) : base(pageManager)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            _gameContext = new GameContext(Game);

        }

        public override bool SideloadContent()
        {
            content = new ContentManager(pageManager.Game.Services, "Content");

            return base.SideloadContent();
        }

        public override void GetTransitionInfo(IPage pageB, out TimeSpan durationA, out EnumTransitionSync syncA)
        {
            durationA = TimeSpan.FromSeconds(0.4f);
            syncA = EnumTransitionSync.Exclusive;
            return;
        }
        protected override void UnloadContent()
        {
            content.Dispose();
        }

        public override void HandleInput(InputState input)
        {
            if (TransitionState == EnumTransitionState.Active)
            {

                if (input.IsButtonPressed(Buttons.B) ||
                    input.IsButtonPressed(Buttons.Back) ||
                    input.IsKeyReleased(Keys.Escape) ||
                    input.IsKeyReleased(Keys.Back)
                    )
                {
                    var startMenuPage = new StartMenuPage(pageManager);
                    startMenuPage.Initialize();
                    pageManager.SideloadPage(startMenuPage);
                    pageManager.ReplacePage(startMenuPage);
                }
            }


            _gameContext.HandleInput(input);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (TransitionState == EnumTransitionState.TransitionOut)
            {
                double invDelta = (1.0 - this.TransitionDelta);
                double dt = gameTime.ElapsedGameTime.TotalSeconds * invDelta;
                var gt = new GameTime(gameTime.TotalGameTime, TimeSpan.FromSeconds(dt));
                _gameContext.Update(gt);
            }
            else
            {
                _gameContext.Update(gameTime);
            }

            if (TransitionState != EnumTransitionState.TransitionOut && _gameContext.PlayerState == Components.PlayerState.Lost)
            {
                var gamePage = new GameOverPage(pageManager, _gameContext);
                gamePage.Initialize();
                pageManager.SideloadPage(gamePage);
                pageManager.ReplacePage(gamePage);
            }

        }

        public override void Draw(GameTime gameTime)
        {
            float viewWidth = this.GetViewWidth();
            float viewHeight = this.GetViewHeight();
            UpdateMenuEffectProjection();

#if CARDBOARD
            viewHeight = viewWidth * 0.8f;
#endif

            Vector2 screenSize = new Vector2(viewWidth, viewHeight);
            Vector2 origin = Vector2.Zero;


            float fade = 1;
            fade = this.TransitionDelta;

            _gameContext.SetUIEffect(this.UiEffect);
            
            pageManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.DepthRead , RasterizerState.CullNone, this.UiEffect);
            _gameContext.Draw(gameTime, this.content , pageManager.SpriteBatch, fade);
            pageManager.SpriteBatch.End();


        }


        public override void OnRemoved(EventArgs e)
        {
            base.OnRemoved(e);
            Dispose();
        }
    }
}
