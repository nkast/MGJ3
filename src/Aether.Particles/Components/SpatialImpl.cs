using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Spatial;
using tainicom.Aether.Elementary.Serialization;

namespace tainicom.Aether.Particles
{
    class SpatialImpl : ISpatial, IWorldTransform, IWorldTransformUpdateable, ILocalTransform, IPosition, ISpatialNode, IAetherSerialization, IAether
    {
        #region Implement IWorldTransform, IWorldTransformUpdateable
        Matrix _parentWorldTransform = Matrix.Identity;
        Matrix _worldTransform = Matrix.Identity;

        public void UpdateWorldTransform(IWorldTransform parentWorldTransform)
        {
            _parentWorldTransform = parentWorldTransform.WorldTransform;
            _worldTransform = _localTransform * _parentWorldTransform;
        }

        public Matrix WorldTransform 
        { 
            get { return _worldTransform; }
        }

        #endregion

        #region Implement ISpatial Properties
        protected Matrix _localTransform = Matrix.Identity;

        Vector3 _position; 
        Vector3 _scale = Vector3.One; 
        Quaternion _rotation = Quaternion.Identity;

        public Matrix LocalTransform { get { return _localTransform; } }

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; UpdateLocalTransform(); }
        }

        public Quaternion Rotation
        {
            get { return _rotation; }
            set { _rotation = value; UpdateLocalTransform(); }
        }

        public Vector3 Scale
        {
            get { return _scale; }
            set { _scale = value; UpdateLocalTransform(); }
        }

        protected void UpdateLocalTransform()
        {
            _localTransform = Matrix.CreateScale(_scale)
                            * Matrix.CreateFromQuaternion(_rotation) 
                            * Matrix.CreateTranslation(_position);

            _worldTransform = _localTransform * _parentWorldTransform;
        }
        #endregion
        
        
        #region Implement IAetherSerialization
        
        public void Save(IAetherWriter writer)
        {
#if(WINDOWS)
            writer.WriteVector3("Position", _position);
            writer.WriteVector3("Scale", _scale);
            writer.WriteQuaternion("Rotation", _rotation);
#endif
        }        

        public void Load(IAetherReader reader)
        {
            reader.ReadVector3("Position", out _position);
            reader.ReadVector3("Scale", out _scale);
            reader.ReadQuaternion("Rotation", out _rotation);
            UpdateLocalTransform();
        }
        #endregion
                
    }
}
