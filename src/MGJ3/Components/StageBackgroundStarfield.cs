using System;
using System.Collections.Generic;
using System.Text;using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGJ3.Components
{
    class StageBackgroundStarfield : DrawableGameComponent
    {
        SpriteBatch _sb;

        const int _virtualWidth = 800;
        const int _virtualHeight = 480;
        Color _baseColor = Color.White * 0.7f;

        readonly Random _rnd = new Random(0);
        Texture2D _txStar;
        const int _starCount = 512;
        float[] _posx = new float[_starCount];
        float[] _posy = new float[_starCount];        
        float[] _dep = new float[_starCount];


        public float Velocity { get; set; }

        public Effect UiEffect { get; set; }

        public StageBackgroundStarfield(Game game) : base(game)
        {
            Velocity = 1000f;
            for (int i=0;i<_starCount;i++)
            {
                _posx[i] = _rnd.Next(_virtualWidth);
                _posy[i] = _rnd.Next(_virtualHeight);
                _dep[i] = (float)_rnd.NextDouble();
            }
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);

            _txStar = new Texture2D(GraphicsDevice, 3, 1);
            _txStar.SetData(new[] { Color.Transparent, Color.White, Color.Transparent });
        }

        public override void Update(GameTime gameTime)
        {
            float dis = (float)(Velocity * gameTime.ElapsedGameTime.TotalSeconds);
            for (int i = 0; i < _starCount; i++)
            {
                float vel = dis * _dep[i];
                _posx[i] = _posx[i] - vel;
                if (_posx[i] <= -_virtualWidth * 0.5f)
                    _posx[i] +=  _virtualWidth * 1.5f;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _sb.Begin(SpriteSortMode.Deferred, effect: UiEffect);

            for (int i = 0; i < _starCount; i++)
            {
                var invDep = (1f - _dep[i]);
                int w = (int)(1f + _dep[i] * Velocity * 0.1f);
                var color = _baseColor * invDep;
                _sb.Draw(_txStar, new Rectangle((int)_posx[i], (int)_posy[i], w, 1), color);
            }            
            _sb.End();
        }
    }
}
