using System;
using tainicom.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Temporal;
using tainicom.Aether.Elementary.Spatial;
using tainicom.Aether.Elementary.Visual;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.Physics2D.Components;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using System.Globalization;

namespace MGJ3.Components
{
    public partial class PowerUp:
        IVisual, 
        ISpatial, ITemporal, IBoundingBox, IInitializable, IAetherSerialization
        , IPhysics2dBody
        , IBonus
    {
        protected virtual string ContentModel
        {
            get { return "Bonuses\\PowerUp"; }
        }

        const float w = 6f;
        const float h = 6f;

        public float q = MathHelper.PiOver2;

        public PowerUp()
        {
        }

        public void Initialize(AetherEngine engine)
        {
            _visualImpl.Initialize(engine, this, ContentModel);
        }


        #region  Implement ISpatial
        SpatialImpl _spatialImpl = new SpatialImpl();
        public Matrix LocalTransform
        {
            get { return _spatialImpl.LocalTransform; }
        }
        #if WINDOWS
        [System.ComponentModel.Category("ISpatial")]
        [System.ComponentModel.TypeConverter(typeof(QuaternionEditAsYawPitchRollConverter))]
        #endif
        public Quaternion Rotation
        {
            get { return _spatialImpl.Rotation; }
            set 
            { 
                _spatialImpl.Rotation = value;
                _bodyImpl.Body.Rotation = Physics2dManager.XNAtoBOX2DRotation(_bodyImpl.Physics2dPlane, value);
            }
        }
        #if WINDOWS
        [System.ComponentModel.Category("ISpatial")]
        [System.ComponentModel.TypeConverter(typeof(Vector3EditConverter))]
        #endif
        public Vector3 Scale
        {
            get { return _spatialImpl.Scale; }
            set { _spatialImpl.Scale = value; }
        }
        #if WINDOWS
        [System.ComponentModel.Category("ISpatial")]
        [System.ComponentModel.TypeConverter(typeof(Vector3EditConverter))]
        #endif
        public Vector3 Position
        {
            get { return _spatialImpl.Position; }
            set 
            {
                _spatialImpl.Position = value;
                if (_bodyImpl.Physics2dPlane != null)
                    _bodyImpl.Body.Position = Physics2dManager.XNAtoBox2DWorldPosition(_bodyImpl.Physics2dPlane, value);
            }
        }
        #endregion


        #region Implement IVisual
        VisualModelImpl _visualImpl = new VisualModelImpl();
        public void Accept(IGeometryVisitor geometryVisitor)
        {
            _visualImpl.Accept(geometryVisitor);
        }

        public IMaterial Material 
        {
            get { return _visualImpl.Material; }
            set { _visualImpl.Material = value; }
        }

        public ITexture[] Textures
        {
            get { return _visualImpl.Textures; }
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
            _bodyImpl.Body.IgnoreGravity = true;
            _bodyImpl.Body.SleepingAllowed = false;
            _bodyImpl.Body.FixedRotation = true;
            _bodyImpl.Body.LinearDamping = 2;
            _bodyImpl.Body.Position = Physics2dManager.XNAtoBox2DWorldPosition(_bodyImpl.Physics2dPlane, this.Position);
            fixture = _bodyImpl.Body.CreateCircle(w/2f, 1, new Vector2(0f, 0f));

            //fixture.IsSensor = true;
            fixture.CollisionCategories = CollisionCategories.Bonuses;
            fixture.CollidesWith = CollisionCategories.StageBounds
                                 | CollisionCategories.Player
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


        #region ITemporal implementation
        public void Tick(GameTime gameTime)
        {
            float accelForce = 128f; // meters/sec
            float t = (float)gameTime.TotalGameTime.TotalSeconds;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (q < 0.3f)
                Body.ApplyLinearImpulse(Body.Mass * dt * new Vector2(accelForce, 0));
            
            _spatialImpl.Position = Physics2dManager.Box2DtoXNAWorldPosition(_bodyImpl.Physics2dPlane, Body.Position, _spatialImpl.Position);

            q -= q * dt;
            _spatialImpl.Rotation = Quaternion.CreateFromYawPitchRoll(0, -q * q * q, 0);

            return;
        }
    
        #endregion


        // will be called whenever some other body collides with 'body'
        bool OnCollision(Fixture sender, Fixture other, Contact contact)
        {
            //if (fixtureB.IsSensor) return false;


            return true;
        }


        #region Implement IBonus
        int IBonus.Score { get { return 0; } }
        int IBonus.Power { get { return 1; } }
        int IBonus.Lives { get { return 0; } }
        #endregion


        #region Implement IAetherSerialization
        public void Save(IAetherWriter writer)
        {
            _spatialImpl.Save(writer);
            _visualImpl.Save(writer);
            _bodyImpl.Save(writer);
        }
        public void Load(IAetherReader reader)
        {
            IAether particle;
            _spatialImpl.Load(reader);
            _visualImpl.Load(reader);
            _bodyImpl.Load(reader);
        }
        #endregion
        
    }
}
