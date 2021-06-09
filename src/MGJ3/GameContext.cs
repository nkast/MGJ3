using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Devices;
using tainicom.Aether.Physics2D.Components;
using MGJ3.Components;
using MGJ3.Stages;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;

namespace MGJ3
{
    class GameContext
    {
        private Game _game;

        StageBackgroundStarfield _stageBackgroundStarfield;
        private Stage _stage;
        private TimeSpan Time;

        Physics2dPlane _physicsPlane0;


        public GameContext(Game game)
        {
            _game = game;
            _stageBackgroundStarfield = new StageBackgroundStarfield(_game);
            _stageBackgroundStarfield.Initialize();

            _stage = new Stage01(game);

            var engine = _stage.Engine;
            var phmgr = engine.Managers.GetManager<Physics2dManager>();
            var sm = phmgr.Root[0];
            _physicsPlane0 = (Physics2dPlane)sm;
            var stageBounds = (StageBounds)engine["StageBounds1"];

            stageBounds.Body.OnCollision += OnStageBoundsCollision;
            _physicsPlane0.World.ContactManager.ContactFilter += OnCollisionFilter;

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

            while(_projectilesToRemove.Count > 0)
            {
                var ibody = _projectilesToRemove.Dequeue();
                engine.UnregisterParticle(ibody);
                // TODO: create a bullet pool
            }
            


            var player1 = (Player)engine["Player1"];

            // player fire
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
                _physicsPlane0.Add(bullet);
                bullet.Position = player1.Position + new Vector3(2, -2, 0);
                bullet.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationZ(MathHelper.ToRadians(-90)));
            }

        }

        Queue<IPhysics2dBody> _projectilesToRemove = new Queue<IPhysics2dBody>();

        private bool OnStageBoundsCollision(Fixture sender, Fixture other, Contact contact)
        {
            if ((other.CollisionCategories & CollisionCategories.Projectiles) != 0)
            {
                var body = other.Body;
                IPhysics2dBody ibody = (IPhysics2dBody)body.Tag;
                _projectilesToRemove.Enqueue(ibody);
                return false;
            }

            return true;
        }


        bool OnCollisionFilter(Fixture fixtureA, Fixture fixtureB)
        {


            return true;
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
