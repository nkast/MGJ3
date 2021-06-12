using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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

        Queue<IPhysics2dBody> _projectilesToRemove = new Queue<IPhysics2dBody>();
        Queue<IPhysics2dBody> _bonusesToRemove = new Queue<IPhysics2dBody>();
        Queue<IPhysics2dBody> _bodiesToRemove = new Queue<IPhysics2dBody>();

        // player info
        public int RemainingLives = 2;
        public int Score = 0;
        public static int HiScore = 4;
        string _scoreTxt;
        string _hiScoreTxt;


        public GameContext(Game game)
        {
            _game = game;
            _stageBackgroundStarfield = new StageBackgroundStarfield(_game);
            _stageBackgroundStarfield.Initialize();

            _stage = new Stage01(game);

            _stage.StageBounds.Body.OnCollision += OnStageBoundsCollision;
            _stage.PhysicsPlane0.World.ContactManager.ContactFilter += OnCollisionFilter;

            _scoreTxt = String.Format("{0:D5}", Score);
            _hiScoreTxt = String.Format("{0:D8}", HiScore);

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
            _stage.Engine.Tick(gameTime);

            while (_projectilesToRemove.Count > 0)
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


            // player fire
            if (_stage.Player1.IsFiring && _bulletTime > _stage.Player1.BulletPeriod)
            {
                _bulletTime = TimeSpan.Zero;
                _stage.Player1.Fire();
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
                {
                    _bonusesToRemove.Enqueue(ibodyA);
                    if (ibodyA is IBonus && ibodyB is Player)
                        ApplyBonus((IBonus)ibodyA);

                }
                colllide = false; // disable collision
            }
            if ((fixtureB.CollisionCategories & CollisionCategories.Bonuses) != 0)
            {
                if (!_bonusesToRemove.Contains(ibodyB))
                {
                    _bonusesToRemove.Enqueue(ibodyB);
                    if (ibodyB is IBonus && ibodyA is Player)
                        ApplyBonus((IBonus)ibodyB);
                }
                colllide = false; // disable collision
            }

            if (colllide == true && (ibodyA is Player || ibodyB is Player) )
            {
                if (RemainingLives > 0)
                {
                    RemainingLives--;
                }
                else
                {
                    // TODO: game over
                }
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

        private void ApplyBonus(IBonus bonus)
        {
            Score += bonus.Score;
            HiScore = Math.Max(HiScore, Score);


            _scoreTxt = String.Format("{0:D5}", Score);
            _hiScoreTxt = String.Format("{0:D8}", HiScore);
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

        internal void Draw(GameTime gameTime, ContentManager content, SpriteBatch spriteBatch, float fade)
        {
            _stageBackgroundStarfield.Draw(gameTime);

            _stage.UpdateCamera();
            _stage.Engine.Render(gameTime);

            DrawScore(gameTime, content, spriteBatch, fade);
        }

        private void DrawScore(GameTime gameTime, ContentManager content, SpriteBatch spriteBatch, float fade)
        {
            SpriteFont font = content.Load<SpriteFont>("Pages/Game/ScoreFont");
            
            spriteBatch.DrawString(font, _scoreTxt, new Vector2(40,6), Color.White * fade);

            spriteBatch.DrawString(font, "HI SCORE", new Vector2(400-20, 6), Color.White * fade);
            spriteBatch.DrawString(font, _hiScoreTxt, new Vector2(400-20, 24), Color.White * fade);



            spriteBatch.DrawString(font, "Remaining Lives: " + RemainingLives, new Vector2(40, 480-6-24), Color.White * fade);



        }
    }
}
