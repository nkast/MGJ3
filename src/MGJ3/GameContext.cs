using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MGJ3.Components;
using tainicom.Devices;
using Microsoft.Xna.Framework.Graphics;

namespace MGJ3
{
    class GameContext
    {
        private Game _game;

        StageBackgroundStarfield _stageBackgroundStarfield;

        public GameContext(Game game)
        {
            this._game = game;
            _stageBackgroundStarfield = new StageBackgroundStarfield(_game);
            _stageBackgroundStarfield.Initialize();
        }

        internal void HandleInput(InputState input)
        {
            
        }

        internal void Update(GameTime gameTime)
        {
            _stageBackgroundStarfield.Update(gameTime);
        }
        internal void SetUIEffect(Effect uiEffect)
        {
            _stageBackgroundStarfield.UiEffect = uiEffect;
        }

        internal void Draw(GameTime gameTime)
        {
            _stageBackgroundStarfield.Draw(gameTime);
        }

    }
}
