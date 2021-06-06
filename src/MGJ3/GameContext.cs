using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MGJ3.Components;
using tainicom.Devices;
using Microsoft.Xna.Framework.Graphics;
using MGJ3.Stages;

namespace MGJ3
{
    class GameContext
    {
        private Game _game;

        StageBackgroundStarfield _stageBackgroundStarfield;
        private Stage _stage;
        private TimeSpan Time;

        public GameContext(Game game)
        {
            this._game = game;
            _stageBackgroundStarfield = new StageBackgroundStarfield(_game);
            _stageBackgroundStarfield.Initialize();

             this._stage = new Stage01(game);
        }

        internal void HandleInput(InputState input)
        {
            
        }

        internal void Update(GameTime gameTime)
        {
            _stageBackgroundStarfield.Update(gameTime);

            Time += gameTime.ElapsedGameTime;
            float time = (float)Time.TotalSeconds;

            var engine = _stage.Engine;

            //update aether
            engine.Tick(gameTime);
        }

        internal void SetUIEffect(Effect uiEffect)
        {
            _stageBackgroundStarfield.UiEffect = uiEffect;
        }

        internal void Draw(GameTime gameTime)
        {
            _stageBackgroundStarfield.Draw(gameTime);

            _stage.UpdateCamera();
            _stage.Engine.Render(gameTime);
        }

    }
}
