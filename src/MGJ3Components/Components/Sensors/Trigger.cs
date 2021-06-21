using System;
using tainicom.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Spatial;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.Physics2D.Components;
//using tainicom.Aether.Physics2D.Factories;
//using tainicom.Aether.Physics2D.Dynamics;

namespace MGJ3.Components
{
    public class Trigger: ILepton, IBoundingBox, IInitializable, IAetherSerialization
        ,IPhysics2dBody
    {
        const float r = 1f/2;

        public void Initialize(AetherEngine engine)
        {
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
        
        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(
                new Vector3(-r , 0, -r),
                new Vector3(+r , r*2, +r ));
        }

        #region Implement IPhysics2dBody
        Physics2dBodyImpl _bodyImpl = new Physics2dBodyImpl();
        Fixture fixture;
        public void InitializeBody(Physics2dPlane physics2dPlane, Body body)
        {
            _bodyImpl.InitializeBody(physics2dPlane, body);
            _bodyImpl.Body.Position = Physics2dManager.XNAtoBox2DWorldPosition(_bodyImpl.Physics2dPlane, this.Position);
            _bodyImpl.Body.BodyType = BodyType.Static;
            fixture = _bodyImpl.Body.CreateCircle(r, 1f, new Vector2(0, r));
            fixture.IsSensor = true;
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
            _leptonImpl.Save(writer);
            _bodyImpl.Save(writer);
        }
        public void Load(IAetherReader reader)
        {
            _leptonImpl.Load(reader);
            _bodyImpl.Load(reader);
        }
        #endregion




        public bool Activated
        {
            get { return false; }
        }

        public bool InitialActiveState { get; set; }

        public string SoundNameActivate { get; set; }

        public float VibratePower { get; set; }

        public void Activate()
        {
        }

        public void Deactivate()
        {
        }

        public void SetActivateState(bool activate)
        {
        }

        public void ResetStage()
        {
            SetActivateState(false);
        }

        public void ResetNewBall()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> ActivatedChanged;

        public event EventHandler<EventArgs> HitScore;

        public int Score
        {
            get { return 0; }
        }

        public int Bonus
        {
            get { return 0; }
        }

        public int IncMultiplier
        {
            get { return 0; }
        }
    }
}
