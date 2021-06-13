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
using MGJ3.Components;

namespace MGJ3.Pages.GamePages
{
    internal class GamePlayPage : BasicPage
    {
        ContentManager content;
        Random rnd = new Random();

        GameContext _gameContext;

        public GamePlayPage(PageManager pageManager, GameContext gameContext) : base(pageManager)
        {
            _gameContext = gameContext;

        }

        public override void Initialize()
        {
            base.Initialize();
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

            if (TransitionState != EnumTransitionState.TransitionOut && _gameContext.PlayerState == PlayerState.Lost)
            {
                var gamePage = new GameOverPage(pageManager, _gameContext);
                gamePage.Initialize();
                pageManager.SideloadPage(gamePage);
                pageManager.ReplacePage(gamePage);
            }


            if (TransitionState == EnumTransitionState.Active && _gameContext.PlayerState == PlayerState.Win)
            {
                if (_gameContext.IncRound())
                {
                    var gamePage = new GameStartPage(pageManager, _gameContext);
                    gamePage.Initialize();
                    pageManager.SideloadPage(gamePage);
                    pageManager.ReplacePage(gamePage);
                    //_gameContext.PlayerState = PlayerState.Normal;
                }
                else
                {
                    var gamePage = new GameFinishPage(pageManager, _gameContext);
                    gamePage.Initialize();
                    pageManager.SideloadPage(gamePage);
                    pageManager.ReplacePage(gamePage);
                }
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
