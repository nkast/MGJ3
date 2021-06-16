using System;
using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary.Leptons;
using tainicom.Aether.Elementary.Serialization;

namespace tainicom.ProtonType.ContentLib.Components
{
    class LeptonImpl: ILepton, IWorldTransform, IWorldTransformUpdateable, IAetherSerialization
    {
        #region Implement ILepton Properties

        Vector3 _position;
        Vector3 _scale = Vector3.One;
        Quaternion _rotation = Quaternion.Identity;
        Matrix _localTransform = Matrix.Identity;

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

        #endregion


        #region Implement IWorldTransform & IWorldTransformUpdateable
        Matrix _parentWorldTransform = Matrix.Identity;
        Matrix _worldTransform = Matrix.Identity;
        public void UpdateWorldTransform(IWorldTransform parentWorldTransform)
        {
            _parentWorldTransform = parentWorldTransform.WorldTransform;
            UpdateWorldTransform();
        }
        public Matrix WorldTransform { get { return _worldTransform; } }
        #endregion


        void UpdateLocalTransform()
        {
            _localTransform = Matrix.CreateScale(_scale)
                            * Matrix.CreateFromQuaternion(_rotation) 
                            * Matrix.CreateTranslation(_position);

            UpdateWorldTransform();
        }

        private void UpdateWorldTransform()
        {
            _worldTransform = _localTransform * _parentWorldTransform;
        }

        #region Implement IAetherSerialization
        public void Save(IAetherWriter writer)
        {
            writer.WriteInt32("Version", 1);

            writer.WriteVector3("Position", _position);
            writer.WriteVector3("Scale", _scale);
            writer.WriteQuaternion("Rotation", _rotation);
        }
        public void Load(IAetherReader reader)
        {
            int version;
            reader.ReadInt32("Version", out version);

            reader.ReadVector3("Position", out _position);
            reader.ReadVector3("Scale", out _scale);
            reader.ReadQuaternion("Rotation", out _rotation);
            UpdateLocalTransform();
        }
        #endregion

    }
}
