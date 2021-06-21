using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary.Spatial;
using tainicom.Aether.Elementary.Serialization;

namespace MGJ3.Components
{
    class LeptonImpl: ILepton, IAetherSerialization
    {
        #region Implement ILepton Properties
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
        }
        #endregion
        
        
        #region Implement IAetherSerialization
        public void Save(IAetherWriter writer)
        {
            writer.WriteVector3("Position", _position);
            writer.WriteVector3("Scale", _scale);
            writer.WriteQuaternion("Rotation", _rotation);
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
