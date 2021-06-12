using System;
using tainicom.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Chronons;
using tainicom.Aether.Elementary.Leptons;
using tainicom.Aether.Elementary.Photons;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.Physics2D.Components;
using tainicom.Aether.Physics2D.Dynamics.Contacts;

namespace MGJ3.Components
{
    public partial class EnemyA:
        IPhoton, 
        ILepton, IChronon, IBoundingBox, IInitializable, IAetherSerialization
        , IPhysics2dBody
        , IHealth, IEnemies
    {
        protected virtual string ContentModel { get { return "Agents\\EnemyA0"; } }

        private AetherEngine _engine;
        const float w = 8f;
        const float h = 8f;

        float _phase = 0;
        float _amplitude = 25;

        public float Phase
        {
            get { return MathHelper.ToDegrees(_phase); }
            set { _phase = MathHelper.ToRadians(value); }
        }

        public float Amplitude
        {
            get { return _amplitude; }
            set { _amplitude = value; }
        }

        public Matrix Rotate = Matrix.Identity;

        public EnemyA()
        {
            InitParticleEmmiter();

            ((IHealth)this).Health = 4;
        }

        public void Initialize(AetherEngine engine)
        {
            _engine = engine;
            _photonImpl.Initialize(engine, this, ContentModel);
        }


        #region  Implement ILepton
        LeptonImpl _leptonImpl = new LeptonImpl();
        public Matrix LocalTransform
        {
            get { return _leptonImpl.LocalTransform; }
        }
        #if WINDOWS
        [System.ComponentModel.Category("ILepton")]
        [System.ComponentModel.TypeConverter(typeof(QuaternionEditAsYawPitchRollConverter))]
        #endif
        public Quaternion Rotation
        {
            get { return _leptonImpl.Rotation; }
            set 
            { 
                _leptonImpl.Rotation = value;
                _bodyImpl.Body.Rotation = Physics2dManager.XNAtoBOX2DRotation(_bodyImpl.Physics2dPlane, value);
            }
        }
        #if WINDOWS
        [System.ComponentModel.Category("ILepton")]
        [System.ComponentModel.TypeConverter(typeof(Vector3EditConverter))]
        #endif
        public Vector3 Scale
        {
            get { return _leptonImpl.Scale; }
            set { _leptonImpl.Scale = value; }
        }
        #if WINDOWS
        [System.ComponentModel.Category("ILepton")]
        [System.ComponentModel.TypeConverter(typeof(Vector3EditConverter))]
        #endif
        public Vector3 Position
        {
            get { return _leptonImpl.Position; }
            set 
            {
                _leptonImpl.Position = value;
                if (_bodyImpl.Physics2dPlane != null)
                    _bodyImpl.Body.Position = Physics2dManager.XNAtoBox2DWorldPosition(_bodyImpl.Physics2dPlane, value);
            }
        }
        #endregion


        #region Implement IPhoton
        PhotonModelImpl _photonImpl = new PhotonModelImpl();
        public void Accept(IGeometryVisitor geometryVisitor)
        {
            _photonImpl.Accept(geometryVisitor);
        }

        public IMaterial Material 
        {
            get { return _photonImpl.Material; }
            set { _photonImpl.Material = value; }
        }

        public ITexture[] Textures
        {
            get { return _photonImpl.Textures; }
            set {  }
        }
        #endregion
        

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(
                new Vector3(-w / 2f, 0, -w / 2f),
                new Vector3(+w / 2f, h, +w / 2f));
        }

        #region Implement IPhysics2dBody
        Physics2dBodyImpl _bodyImpl = new Physics2dBodyImpl();
        Fixture fixture;

        public void InitializeBody(Physics2dPlane physics2dPlane, Body body)
        {
            _bodyImpl.InitializeBody(physics2dPlane, body);
            _bodyImpl.Body.BodyType = BodyType.Dynamic;
            _bodyImpl.Body.IgnoreGravity = false;
            _bodyImpl.Body.SleepingAllowed = false;
            _bodyImpl.Body.FixedRotation = true;
            _bodyImpl.Body.LinearDamping = 2;
            _bodyImpl.Body.Position = Physics2dManager.XNAtoBox2DWorldPosition(_bodyImpl.Physics2dPlane, this.Position);
            fixture = _bodyImpl.Body.CreateCircle(w/2f, 1, new Vector2(0f, 0f));

            fixture.CollisionCategories = CollisionCategories.Enemy;
            fixture.CollidesWith = CollisionCategories.Player
                                 | CollisionCategories.PlayerBullet
                                 ;

            fixture.OnCollision += OnCollision;
        }
        
        public float Restitution
        {
            get { return _bodyImpl.Restitution; }
            set { _bodyImpl.Restitution = value; }
        }
        public float LinearDamping
        {
            get { return _bodyImpl.LinearDamping; }
            set { _bodyImpl.LinearDamping = value; }
        }
        public float AngularDamping
        {
            get { return _bodyImpl.AngularDamping; }
            set { _bodyImpl.AngularDamping = value; }
        }
        public float Friction
        {
            get { return _bodyImpl.Friction; }
            set { _bodyImpl.Friction = value; }
        }

        public Body Body
        {
            get { return _bodyImpl.Body; }
        } 
        #endregion

        #region Chronons implementation
        public void Tick(GameTime gameTime)
        {
            TickParticleEmmiter(gameTime);

            float accelForce = 32f; // meters/sec
            float t = (float)gameTime.TotalGameTime.TotalSeconds;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //stopping force
            //var lvelocity = _bodyImpl.Body.LinearVelocity;
            //_bodyImpl.Body. ApplyLinear Impulse(-lvelocity);

            _bodyImpl.Body.ApplyLinearImpulse(new Vector2(-accelForce,0));
                        
            _leptonImpl.Position = Physics2dManager.Box2DtoXNAWorldPosition(_bodyImpl.Physics2dPlane, Body.Position, _leptonImpl.Position);

            //System.Diagnostics.Debug.WriteLine(_bodyImpl.Body.LinearVelocity.Y);
            float rot = 35f * MathHelper.Clamp(_bodyImpl.Body.LinearVelocity.Y / (13f * 2f), -1f, 1f);
            _leptonImpl.Rotation = Quaternion.CreateFromYawPitchRoll(0,0,MathHelper.ToRadians(90))
                                 * Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(rot));
            
            var targetpos = Position;
            targetpos.Y = (float)Math.Cos(MathHelper.WrapAngle(_phase) + MathHelper.WrapAngle(MathHelper.Tau * t * 1f/6f)) * Amplitude;
            var diffpos = targetpos - Position;

            _bodyImpl.Body.ApplyLinearImpulse(new Vector2(0, diffpos.Y * _bodyImpl.Body.Mass));

            return;
        }
    
        #endregion

        // will be called whenever some other body collides with 'body'
        bool OnCollision(Fixture sender, Fixture other, Contact contact)
        {
            //if (fixtureB.IsSensor) return false;


            return true;
        }


        #region  Implement IDamage
        int IHealth.Health { get; set; }
        #endregion


        #region  Implement IEnemies
        void IEnemies.Kill()
        {
            var coin = new OneCoin();
            _engine.RegisterParticle(coin);
            //engine.SetParticleName(coin, "coin");
            coin.Initialize(_engine);
            coin.Position = this.Position;
            ((Physics2dPlane)Body.World.Tag).Add(coin);

            _engine.UnregisterParticle(this);
        }
        #endregion


        #region Implement IAetherSerialization
        public void Save(IAetherWriter writer)
        {
            _leptonImpl.Save(writer);
            _photonImpl.Save(writer);
            _bodyImpl.Save(writer);

            writer.WriteFloat("Amplitude", _amplitude);
            writer.WriteFloat("Phase", _phase);

            SaveParticleEmmiter(writer);
        }
        public void Load(IAetherReader reader)
        {
            IAether particle;
            _leptonImpl.Load(reader);
            _photonImpl.Load(reader);
            _bodyImpl.Load(reader);

            reader.ReadFloat("Amplitude", out _amplitude);
            reader.ReadFloat("Phase", out _phase);

            LoadParticleEmmiter(reader);
        }

        #endregion

    }
}
