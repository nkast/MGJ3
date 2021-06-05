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

namespace MGJ3.Pages.GamePages
{
    internal class GameStartPage : BasicPage
    {
        ContentManager content;
        Scheduler scheduler;
        Random rnd = new Random();

        public GameStartPage(PageManager pageManager) : base(pageManager)
        {
            
        }

        public override void Initialize()
        {
            base.Initialize();

            scheduler = new Scheduler();
            scheduler.AddEventSpan("delay",  0.05f); // 0- delay
            scheduler.AddEventSpan("fadein", 0.4f); // 1- fadein
            scheduler.AddEventSpan("show",   3.0f);   // 2- show

        }

        public override bool SideloadContent()
        {
            content = new ContentManager(pageManager.Game.Services, "Content");

            return base.SideloadContent();
        }

        protected override void UnloadContent()
        {
            content.Dispose();
        }

        public override void HandleInput(InputState input)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            scheduler.Update(gameTime);

            switch (scheduler.SpanId)
            {
                case 2:
                    break;
                case 3:
                    if (TransitionState == EnumTransitionState.Active)
                    {
                        var gameOverPage = new GameOverPage(pageManager);
                        gameOverPage.Initialize();
                        pageManager.SideloadPage(gameOverPage);
                        pageManager.ReplacePage(gameOverPage);
                    }
                    break;
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
            switch (scheduler.SpanId)
            {
                case 0:
                    fade = 0f;
                    break;
                case 1:
                    fade = scheduler.Delta;
                    break;
                case 2:
                    break;
                case 3:
                    fade = this.TransitionDelta;
                    break;
            }

            Color color = new Color(fade, fade, fade, fade);
          
            pageManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.DepthRead , RasterizerState.CullNone, this.UiEffect);
            
            pageManager.SpriteBatch.End();
        }

        public override void GetTransitionInfo(IPage pageB, out TimeSpan durationA, out EnumTransitionSync syncA)
        {
            durationA = TimeSpan.FromSeconds(0.4f);
            syncA = EnumTransitionSync.Exclusive;
            return;
        }

        public override void OnRemoved(EventArgs e)
        {
            base.OnRemoved(e);
            Dispose();
        }
    }
}
