using System;
using tainicom.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Chronons;
using tainicom.Aether.Elementary.Leptons;
using tainicom.Aether.Elementary.Photons;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.Physics2D.Components;

namespace MGJ3.Components
{
    public partial class Player: 
        IPhoton, 
        ILepton, IChronon, IBoundingBox, IInitializable, IAetherSerialization
        ,IPhysics2dBody
    {
        protected virtual string ContentModel { get { return "Agents\\Player"; } }

        const float w = 8f;
        const float h = 8f;

        public bool IsFiring;
        public TimeSpan BulletPeriod { get { return TimeSpan.FromSeconds(1f / 4f); } }

        public Trigger StartingPosition { get; set; }

        public Matrix Rotate = Matrix.Identity;

        public void Initialize(AetherEngine engine)
        {
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
            _bodyImpl.Body.LinearDamping = 8;
            _bodyImpl.Body.Position = Physics2dManager.XNAtoBox2DWorldPosition(_bodyImpl.Physics2dPlane, this.Position);
            fixture = _bodyImpl.Body.CreateCircle(w/2f, 1, new Vector2(0f, 0f));

            fixture.OnCollision += OnCollision;

            fixture.CollisionCategories = CollisionCategories.Player;
            fixture.CollidesWith = CollisionCategories.Player // co-op?
                                    | CollisionCategories.StageBounds
                                    | CollisionCategories.Enemies
                                    | CollisionCategories.Bonuses
                                    ;
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

            float accelForce = 4000f; // meters/sec
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState kstate = Keyboard.GetState();
            Vector2 input = Vector2.Zero;
            if (kstate.IsKeyDown(Keys.F)) input.X -= 1f;
            if (kstate.IsKeyDown(Keys.H)) input.X += 1f;
            if (kstate.IsKeyDown(Keys.T)) input.Y += 1f;
            if (kstate.IsKeyDown(Keys.G)) input.Y -= 1f;

            if (kstate.IsKeyDown(Keys.Left)) input.X -= 1f;
            if (kstate.IsKeyDown(Keys.Right)) input.X += 1f;
            if (kstate.IsKeyDown(Keys.Up)) input.Y += 1f;
            if (kstate.IsKeyDown(Keys.Down)) input.Y -= 1f;

            var gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.IsConnected)
            {
                Vector2 inputl = gamePadState.ThumbSticks.Left;
                inputl = Vector2.TransformNormal(inputl, Rotate); //rotate input
                if (inputl != Vector2.Zero)
                {
                    _bodyImpl.Body.ApplyLinearImpulse(inputl * accelForce*2);
                }
            }

            //stopping force
            //var lvelocity = _bodyImpl.Body.LinearVelocity;
            //_bodyImpl.Body. ApplyLinear Impulse(-lvelocity);

            if (input != Vector2.Zero)
            {
                input.Normalize();
                input = Vector2.TransformNormal(input, Rotate); //rotate input
                _bodyImpl.Body.ApplyLinearImpulse(input * accelForce);
            }


            IsFiring = false;
            if (kstate.IsKeyDown(Keys.Space)) IsFiring = true;
            if (gamePadState.IsConnected)
                IsFiring = IsFiring | gamePadState.IsButtonDown(Buttons.A);



            _leptonImpl.Position = Physics2dManager.Box2DtoXNAWorldPosition(_bodyImpl.Physics2dPlane, Body.Position, _leptonImpl.Position);

            //System.Diagnostics.Debug.WriteLine(_bodyImpl.Body.LinearVelocity.Y);
            float rot = -35f * MathHelper.Clamp(_bodyImpl.Body.LinearVelocity.Y/(132f*2f), -1f, 1f);
            _leptonImpl.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationZ(MathHelper.ToRadians(-90)))
                                 * Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(rot));
                                 

            return;
        }

        #endregion

        // will be called whenever some other body collides with 'body'
        bool OnCollision(Fixture fixtureA, Fixture fixtureB, tainicom.Aether.Physics2D.Dynamics.Contacts.Contact contact)
        {
            if (fixtureB.IsSensor) return false;


            return true;
        }


        #region Implement IAetherSerialization
        public void Save(IAetherWriter writer)
        {
            _leptonImpl.Save(writer);
            _photonImpl.Save(writer);
            _bodyImpl.Save(writer);
            writer.WriteParticle("StartingPosition", StartingPosition);

            SaveParticleEmmiter(writer);
        }
        public void Load(IAetherReader reader)
        {
            IAether particle;
            _leptonImpl.Load(reader);
            _photonImpl.Load(reader);
            _bodyImpl.Load(reader);
            reader.ReadParticle("StartingPosition", out particle); StartingPosition = (Trigger)particle;

            LoadParticleEmmiter(reader);
        }
        #endregion
        
    }
}
