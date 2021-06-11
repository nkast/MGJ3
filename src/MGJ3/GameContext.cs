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
        Queue<IPhysics2dBody> _projectilesToRemove = new Queue<IPhysics2dBody>();
        Queue<IPhysics2dBody> _bonusesToRemove = new Queue<IPhysics2dBody>();
        Queue<IPhysics2dBody> _bodiesToRemove = new Queue<IPhysics2dBody>();


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

            while (_bonusesToRemove.Count > 0)
            {
                var ibody = _bonusesToRemove.Dequeue();
                engine.UnregisterParticle(ibody);
            }

            while (_bodiesToRemove.Count > 0)
            {
                var ibody = _bodiesToRemove.Dequeue();
                if (ibody is IEnemies)
                    ((IEnemies)ibody).Kill();
                else
                    engine.UnregisterParticle(ibody);
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

        private bool OnStageBoundsCollision(Fixture sender, Fixture other, Contact contact)
        {
            if ((other.CollisionCategories & CollisionCategories.Projectiles) != 0)
            {
                var ibody = (IPhysics2dBody)other.Body.Tag;
                _projectilesToRemove.Enqueue(ibody);
                return false;
            }
            if ((other.CollisionCategories & CollisionCategories.Bonuses) != 0)
            {
                var ibody = (IPhysics2dBody)other.Body.Tag;
                _bonusesToRemove.Enqueue(ibody);
                return false;
            }

            return true;
        }


        bool OnCollisionFilter(Fixture fixtureA, Fixture fixtureB)
        {
            bool colllide = true;

            var ibodyA = (IPhysics2dBody)fixtureA.Body.Tag;
            var ibodyB = (IPhysics2dBody)fixtureB.Body.Tag;

            ApplyDamage(ibodyA, ibodyB);
            ApplyDamage(ibodyB, ibodyA);

            // handle Projectiles
            if ((fixtureA.CollisionCategories & CollisionCategories.Projectiles) != 0)
            {
                if (!_projectilesToRemove.Contains(ibodyA))
                    _projectilesToRemove.Enqueue(ibodyA);
                colllide = false; // disable collision
            }
            if ((fixtureB.CollisionCategories & CollisionCategories.Projectiles) != 0)
            {
                if (!_projectilesToRemove.Contains(ibodyB))
                    _projectilesToRemove.Enqueue(ibodyB);
                colllide = false; // disable collision
            }

            // handle Bonuses
            if ((fixtureA.CollisionCategories & CollisionCategories.Bonuses) != 0)
            {
                if (!_bonusesToRemove.Contains(ibodyA))
                    _bonusesToRemove.Enqueue(ibodyA);
                colllide = false; // disable collision
            }
            if ((fixtureB.CollisionCategories & CollisionCategories.Bonuses) != 0)
            {
                  if (!_bonusesToRemove.Contains(ibodyB))
                    _bonusesToRemove.Enqueue(ibodyB);
                colllide = false; // disable collision
            }

            var ihealthA = ibodyA as IHealth;
            if (ihealthA != null)
            {
                if (ihealthA.Health <= 0)
                {
                    if (!_bodiesToRemove.Contains(ibodyA))
                        _bodiesToRemove.Enqueue(ibodyA);
                    colllide = false; // disable collision
                }
            }

            var ihealthB = ibodyB as IHealth;
            if (ihealthB != null)
            {
                if (ihealthB.Health <= 0)
                {
                    if (!_bodiesToRemove.Contains(ibodyB))
                        _bodiesToRemove.Enqueue(ibodyB);
                    colllide = false; // disable collision
                }
            }

            return colllide;
        }

        private bool ApplyDamage(IPhysics2dBody ibodyA, IPhysics2dBody ibodyB)
        {
            var idamage = ibodyA as IDamage;
            var ihealth = ibodyB as IHealth;

            if (idamage != null && ihealth != null)
            {
                ihealth.Health -= idamage.Damage;
                return true;
            }
            else
            {
                return false;
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
