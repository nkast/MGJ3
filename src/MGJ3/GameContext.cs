﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Devices;
using tainicom.Aether.Physics2D.Components;
using MGJ3.Components;
using MGJ3.Stages;
using tainicom.Aether.Physics2D.Dynamics;

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

        TimeSpan _bulletTime;
        internal void Update(GameTime gameTime)
        {
            _stageBackgroundStarfield.Update(gameTime);

            Time += gameTime.ElapsedGameTime;
            float time = (float)Time.TotalSeconds;

            _bulletTime += gameTime.ElapsedGameTime;

            var engine = _stage.Engine;

            //update aether
            engine.Tick(gameTime);

            var player1 = (Player)engine["Player1"];

            if (player1.IsFiring && _bulletTime > player1.BulletPeriod)
            {
                _bulletTime = TimeSpan.Zero;

                var bullet = new PlayerBullet();
                engine.RegisterParticle(bullet);
                //engine.SetParticleName(bullet, "bullet");
                bullet.Initialize(engine);                
                var phmgr = engine.Managers.GetManager<Physics2dManager>();
                var sm = phmgr.Root[0];
                var pl= (Physics2dPlane)sm;
                pl.Add(bullet);
                bullet.Position = player1.Position + new Vector3(2,2,0);
                bullet.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationZ(MathHelper.ToRadians(-90)));

                bullet = new PlayerBullet();
                engine.RegisterParticle(bullet);
                //engine.SetParticleName(bullet, "bullet");
                bullet.Initialize(engine);
                phmgr = engine.Managers.GetManager<Physics2dManager>();
                sm = phmgr.Root[0];
                pl = (Physics2dPlane)sm;
                pl.Add(bullet);
                bullet.Position = player1.Position + new Vector3(2, -2, 0);
                bullet.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationZ(MathHelper.ToRadians(-90)));
            }

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
