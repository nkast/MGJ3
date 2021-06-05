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

namespace MGJ3.Pages.MenuPages
{
    class StartMenuPage : BasicPage
    {
        ContentManager content;
        Scheduler scheduler;
        Random rnd = new Random();

        Texture2D _txBackground;
        Texture2D _txTitle;



        public StartMenuPage(PageManager pageManager)
            : base(pageManager)
        {
            TransitionStateChanged += new EventHandler<TransitionStateChangedEventArgs>(StartMenuPage_TransitionStateChanged);
        }

        public override void Initialize()
        {
            base.Initialize();

            scheduler = new Scheduler();
        }

        public override bool SideloadContent()
        {
            content = new ContentManager(pageManager.Game.Services, "Content");

            _txBackground = content.Load<Texture2D>(@"Pages\Menu\Background");
            _txTitle = content.Load<Texture2D>(@"Pages\Menu\MenuTitle");

            return base.SideloadContent();
        }

        public override void GetTransitionInfo(IPage pageB, out TimeSpan durationA, out EnumTransitionSync syncA)
        {
            durationA = TimeSpan.FromSeconds(0.4f);
            syncA = EnumTransitionSync.Exclusive;
            return;
        }

        void StartMenuPage_TransitionStateChanged(object sender, TransitionStateChangedEventArgs e)
        {
        }

        public override void HandleInput(InputState input)
        {
            if (TransitionState == EnumTransitionState.Active)
            {

            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);



        }

        float spark = 1f;
        public override void Draw(GameTime gameTime)
        {
            float viewWidth = this.GetViewWidth();
            float viewHeight = this.GetViewHeight();
            UpdateMenuEffectProjection();

            Vector2 screenSize = new Vector2(viewWidth, viewHeight);
            float fade = this.TransitionDelta;
            Color color = new Color(fade, fade, fade, fade);

            float nextspark = (float)(rnd.NextDouble() / 2.0 + 1.0 / 2.0);
            spark = spark + (nextspark - spark) * 0.5f;
            Color scolor = Color.LightYellow * fade * spark;
            
            pageManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, this.UiEffect);
            
            pageManager.SpriteBatch.Draw(_txBackground, Vector2.Zero, null, scolor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.2f);

            pageManager.SpriteBatch.Draw(_txTitle, new Vector2(400,100), null, color, 0f, new Vector2(_txTitle.Width, _txTitle.Height) / 2f, 1f, SpriteEffects.None, 0.7f);


            pageManager.SpriteBatch.End();
        }
    }
}
