using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Physics2D.Components;
using nkast.Aether.Physics2D.Dynamics;

namespace MGJ3.Components
{
    internal class Physics2dBodyImpl : IAetherSerialization //, IPhysics2dBody
    {
        protected Body _body;

        public float _friction;
        float _restitution;
        float _linearDamping;



        public Physics2dBodyImpl()
        {
        }

        #region Implement IPhysics2dBody

        public void InitializeBody(Physics2dPlane physics2dPlane, Body body)
        {
            this.Physics2dPlane = physics2dPlane;
            _body = body;
        }

        public Physics2dPlane Physics2dPlane { get; private set; }
        
        public Body Body
        {
            get { return _body; }
        }

        public float AngularDamping
        {
            get { return _body.AngularDamping; }
            set { _body.AngularDamping = value; }
        }

        public float Restitution
        {
            get { return _restitution; }
            set
            {
                _restitution = value;
                if (_body != null) 
                    foreach (Fixture fixture in _body.FixtureList)
                        fixture.Restitution = _restitution;
            }
        }

        public float LinearDamping
        {
            get { return _body.LinearDamping; }
            set { _body.LinearDamping = value; }
        }

        public float Friction
        {
            get { return _friction; }
            set
            {
                _friction = value;
                if (_body != null) 
                    foreach (Fixture fixture in _body.FixtureList)
                        fixture.Friction = _friction;
            }
        }
        #endregion


        #region Implement IAetherSerialization
        public void Save(IAetherWriter writer)
        {
            //writer.WriteVector3("Position", _position);
            //writer.WriteVector3("Scale", _scale);
            //writer.WriteQuaternion("Rotation", _rotation);
        }
        public void Load(IAetherReader reader)
        {
            //reader.ReadVector3("Position", out _position);
            //reader.ReadVector3("Scale", out _scale);
            //reader.ReadQuaternion("Rotation", out _rotation);
            //UpdateLocalTransform();
        }
        #endregion
    }
}
