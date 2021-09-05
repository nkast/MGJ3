using System;
using tainicom.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Spatial;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.Physics2D.Components;
using tainicom.Aether.Core.Spatial;

namespace MGJ3.Components
{
    public partial class StageBounds :
        ISpatial, IBoundingBox, IInitializable, IAetherSerialization
        , IPhysics2dBody
    {
        public float Width 
        {
            get { return w; }
            set { w = value; }
        }
        public float Height
        {
            get { return h; }
            set { h = value; }
        }

        private float w = 129f;
        private float h = 80f;

        public void Initialize(AetherEngine engine)
        {

        }


        #region  Implement ISpatial
        SpatialBase _spatialImpl = new SpatialBase();
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


        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(
                new Vector3(-w / 2f, 0, -w / 2f),
                new Vector3(+w / 2f, h, +w / 2f));
        }

        #region Implement IPhysics2dBody
        Physics2dBodyImpl _bodyImpl = new Physics2dBodyImpl();
        Fixture[] fixture = new Fixture[4];

        public void InitializeBody(Physics2dPlane physics2dPlane, Body body)
        {
            _bodyImpl.InitializeBody(physics2dPlane, body);
            _bodyImpl.Body.BodyType = BodyType.Static;
            _bodyImpl.Body.IgnoreGravity = true;
            _bodyImpl.Body.SleepingAllowed = false;
            _bodyImpl.Body.FixedRotation = true;
            _bodyImpl.Body.LinearDamping = 2;
            _bodyImpl.Body.Position = Physics2dManager.XNAtoBox2DWorldPosition(_bodyImpl.Physics2dPlane, this.Position);
            fixture[0] = _bodyImpl.Body.CreateEdge(new Vector2(-w/2f,-h/2f), new Vector2(-w/2f, h/2f)); // left
            fixture[1] = _bodyImpl.Body.CreateEdge(new Vector2( w/2f,-h/2f), new Vector2( w/2f, h/2f)); // right
            fixture[2] = _bodyImpl.Body.CreateEdge(new Vector2(-w/2f,-h/2f), new Vector2( w/2f,-h/2f)); // bottom
            fixture[3] = _bodyImpl.Body.CreateEdge(new Vector2(-w/2f, h/2f), new Vector2( w/2f, h/2f)); // top

            for (int i = 0; i < 4; i++)
            {
                fixture[i].CollisionCategories = CollisionCategories.StageBounds;
                fixture[i].CollidesWith = CollisionCategories.Player
                                        | CollisionCategories.Projectiles
                                        | CollisionCategories.Bonuses
                                        ;
            }

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

        #region Implement IAetherSerialization
        public void Save(IAetherWriter writer)
        {
            _spatialImpl.Save(writer);
            _bodyImpl.Save(writer);
        }
        public void Load(IAetherReader reader)
        {
            IAether particle;
            _spatialImpl.Load(reader);
            _bodyImpl.Load(reader);
        }
        #endregion
        
    }
}
