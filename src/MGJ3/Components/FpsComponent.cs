using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MGJ3.Components
{
    class FpsComponent : DrawableGameComponent
    {
        SpriteBatch _sb;
        SpriteFont _debugFont;
        

        readonly Stopwatch sw = Stopwatch.StartNew();
        int fps = 0;
        int fc = 0;
        string _fpsTxt = "??";


        public FpsComponent(Game game) : base(game)
        {
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);
            _debugFont = Game.Content.Load<SpriteFont>("DebugFont");

        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            fc++;
            var elapsed = sw.Elapsed;
            if (elapsed > TimeSpan.FromSeconds(1))
            {
                fps = (int)Math.Round(fc / elapsed.TotalSeconds);
                sw.Restart();
                fc = 0;

                _fpsTxt = fps.ToString();
                //this.Game.Window.Title = fps.ToString() + "fps";
            }

            _sb.Begin(SpriteSortMode.Deferred);
            {
                var color = (fps < 60) ? Color.DarkRed : Color.CornflowerBlue;
                _sb.DrawString(_debugFont, _fpsTxt, new Vector2(10,6), color);
            }
            _sb.End();
        }
    }
}
