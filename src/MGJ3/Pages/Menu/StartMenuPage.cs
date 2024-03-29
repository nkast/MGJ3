﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Devices;
using tainicom.PageManager;
using tainicom.PageManager.Enums;
using tainicom.PageManager.Events;
using tainicom.Tweens;
using MGJ3.Pages.Menu.UI;
using MGJ3.Pages.GamePages;

namespace MGJ3.Pages.MenuPages
{
    class StartMenuPage : BasicPage
    {
        ContentManager content;
        Scheduler scheduler;
        Random rnd = new Random();

        Texture2D _txBackground;
        Texture2D _txTitle;
        UIButton _btnSettings;
        UIButton _btnPlay;
        UIButton _btnAbout;
        List<UIButton> _buttons = new List<UIButton>();
        int _selectionIndex = 1;



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
            _btnSettings = new UIButton(new Vector2(250, 240), 0.7f, content.Load<Texture2D>(@"Pages\UIButtons\UIButtonSettings"));
            _btnPlay = new UIButton(new Vector2(400, 240), 0.7f, content.Load<Texture2D>(@"Pages\UIButtons\UIButtonPlay"));
            _btnAbout = new UIButton(new Vector2(550, 240), 0.7f, content.Load<Texture2D>(@"Pages\UIButtons\UIButtonAbout"));
            _buttons.AddRange(new[] { _btnSettings , _btnPlay, _btnAbout });

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
                //if (input.GamePadState.IsConnected)
                {
                    if (input.IsButtonPressed(Buttons.DPadRight) ||
                        input.IsKeyReleased(Keys.Right) ||
                        input.IsKeyReleased(Keys.D)
                        )
                    {
                        _selectionIndex = (_selectionIndex+1)% _buttons.Count;
                    }

                    if (input.IsButtonPressed(Buttons.DPadLeft) ||
                        input.IsKeyReleased(Keys.Left) ||
                        input.IsKeyReleased(Keys.A)
                        )
                    {
                        _selectionIndex = (_selectionIndex-1 + _buttons.Count) % _buttons.Count;
                    }

                    if (input.IsButtonPressed(Buttons.A) ||
                        input.IsKeyReleased(Keys.Space) ||
                        input.IsKeyReleased(Keys.Enter)
                        )
                    {
                        switch(_selectionIndex)
                        {
                            case 0:
                                {
                                    var settingsMenuPage = new SettingsMenuPage(pageManager);
                                    settingsMenuPage.Initialize();
                                    pageManager.SideloadPage(settingsMenuPage);
                                    pageManager.ReplacePage(settingsMenuPage);
                                }
                                break;
                            case 1:
                                {
                                    var gamePage = new GameStartPage(pageManager);
                                    gamePage.Initialize();
                                    pageManager.SideloadPage(gamePage);
                                    pageManager.ReplacePage(gamePage);
                                }
                                break;
                            case 2:
                                {
                                    var aboutMenuPage = new AboutMenuPage(pageManager);
                                    aboutMenuPage.Initialize();
                                    pageManager.SideloadPage(aboutMenuPage);
                                    pageManager.ReplacePage(aboutMenuPage);
                                }
                                break;
                        }
                        content.Load<SoundEffect>("Pages/Menu/Select").Play();
                    }

                }

                if (input.IsButtonPressed(Buttons.B) ||
                    input.IsKeyReleased(Keys.Escape) ||
                    input.IsKeyReleased(Keys.Back)
                    )
                {
                    try
                    {
                        Game.Exit();
                    }
                    catch
                    {
#if ANDROID
                        Game.Services.GetService<MGJ3Activity>().MoveTaskToBack(true);
#endif
                    }
                }

            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


            for (int i = 0; i < _buttons.Count; i++)
            {
                var btn = _buttons[i];

                var targetScale = 1.0f;
                var targetDepth = 0.7f;
                var damping = 0.92f;

                if (i == _selectionIndex)
                {
                    targetScale = 1.15f;
                    targetScale += (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3.14f * 2f) * 0.05f;
                    targetDepth = 0.9f;
                }

                btn.Scale += (targetScale - btn.Scale) * (1f-damping);
                btn.Depth += (targetDepth - btn.Depth) * (1f-damping);
            }


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
            
            pageManager.SpriteBatch.Draw(_txBackground, Vector2.Zero, null, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.2f);

            pageManager.SpriteBatch.Draw(_txTitle, new Vector2(400,100), null, scolor, 0f, new Vector2(_txTitle.Width, _txTitle.Height) / 2f, 1f, SpriteEffects.None, 0.7f);

            for (int i=0; i<_buttons.Count;i++)
            {
                var btn = _buttons[i];
                var animOffset = (Vector2.UnitY * (1f - this.TransitionDelta) * (480f + i*128f));
                pageManager.SpriteBatch.Draw(btn.texture, btn.Position + animOffset, null, color, 0f, btn.Origin, btn.Scale, SpriteEffects.None, btn.Depth);

            }

            pageManager.SpriteBatch.End();
        }
    }
}
