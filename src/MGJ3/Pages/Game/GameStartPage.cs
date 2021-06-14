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
    internal class GameStartPage : BasicPage
    {
        ContentManager content;
        Random rnd = new Random();

        GameContext _gameContext;
        SpriteFont _font;

        public GameStartPage(PageManager pageManager) : base(pageManager)
        {

        }

        public GameStartPage(PageManager pageManager, GameContext gameContext) : base(pageManager)
        {
            _gameContext = gameContext;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (_gameContext == null)
            {
                _gameContext = new GameContext(Game);
            }
            else
            {
                var pp1 = _gameContext.Stage.Player1.Position;
                _gameContext.LoadStage(Game);
                _gameContext.Stage.Player1.Position = pp1;
            }
        }

        public override bool SideloadContent()
        {
            content = new ContentManager(pageManager.Game.Services, "Content");

            _font = content.Load<SpriteFont>(@"Pages\Menu\About\AboutFont");

            return base.SideloadContent();
        }

        public override void GetTransitionInfo(IPage pageB, out TimeSpan durationA, out EnumTransitionSync syncA)
        {
            durationA = TimeSpan.FromSeconds(0.6f);
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


            //_gameContext.HandleInput(input);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
          
            //_gameContext.Update(gameTime);

            // move player to starting position
            var player = _gameContext.Stage.Player1;
            var trgt = new Vector2(-120,0);
            var diff = trgt - player.Body.Position;
            player.Body.LinearVelocity = Vector2.Zero;
            player.Body.ApplyLinearImpulse(player.Body.Mass * TransitionDelta * diff);


            if (TransitionState == EnumTransitionState.Active)
            {
                var gamePage = new GamePlayPage(pageManager, _gameContext);
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
            //_gameContext.Draw(gameTime, this.content , pageManager.SpriteBatch, fade);

            var txt = "ROUND " + _gameContext.Round;
            pageManager.SpriteBatch.DrawString(_font, txt, screenSize/2f - _font.MeasureString(txt), Color.White * fade);

            pageManager.SpriteBatch.End();


        }


        public override void OnRemoved(EventArgs e)
        {
            base.OnRemoved(e);
            Dispose();
        }
    }
}
