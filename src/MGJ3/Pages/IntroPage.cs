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

namespace MGJ3.Pages
{
    internal class IntroPage : BasicPage
    {
        ContentManager content;
        Scheduler scheduler;
        Random rnd = new Random();

        Texture2D _txTitle;

        public IntroPage(PageManager pageManager) : base(pageManager)
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
            _txTitle = content.Load<Texture2D>(@"Pages\Intro\IntroTitle");

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
                        // TODO: load next page
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
            Vector2 txSize2 = new Vector2(_txTitle.Width, _txTitle.Height);
            Vector2 origin = Vector2.Zero;            
            Vector2 txPos2 = new Vector2(0, (screenSize.Y - txSize2.Y) / 2f ); //left-center            


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
            pageManager.SpriteBatch.Draw(_txTitle, txPos2, null, color, 0f, origin, 1f, SpriteEffects.None, 0.9f);
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
