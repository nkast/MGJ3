#if WINDOWS
using tainicom.Aether.Design.Converters;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Leptons;
using tainicom.Aether.Elementary.Photons;
using tainicom.Aether.Elementary.Serialization;


namespace tainicom.Aether.Particles
{
    public abstract class ParticleSystemBase :
        ILocalTransform, IPosition,
        IWorldTransform, IWorldTransformUpdateable,
        IPhoton, IAetherSerialization
    {
        public ParticleSystemBase()
        {

        }

        #region  Implement ILepton
        LeptonImpl _leptonImpl = new LeptonImpl();
        public Matrix LocalTransform { get { return _leptonImpl.LocalTransform; } }
        #if WINDOWS
        [Category("ILepton")]
        [TypeConverter(typeof(QuaternionEditAsYawPitchRollConverter))]
        #endif
        public Quaternion Rotation
        {
            get { return _leptonImpl.Rotation; }
            set { _leptonImpl.Rotation = value; }
        }
        #if WINDOWS
        [Category("ILepton")]
        [TypeConverter(typeof(Vector3EditConverter))]
        #endif
        public Vector3 Scale
        {
            get { return _leptonImpl.Scale; }
            set { _leptonImpl.Scale = value; }
        }
        #if WINDOWS
        [Category("ILepton")]
        [TypeConverter(typeof(Vector3EditConverter))]
        #endif
        public Vector3 Position
        {
            get { return _leptonImpl.Position; }
            set { _leptonImpl.Position = value; }
        }
        #endregion
        

        #region  Implement IWorldTransform, IWorldTransformUpdateable
        public void UpdateWorldTransform(IWorldTransform parentWorldTransform)
        {
            _leptonImpl.UpdateWorldTransform(parentWorldTransform);
        }

        public Matrix WorldTransform 
        { 
            get { return _leptonImpl.WorldTransform; } 
        }
        #endregion


        #region Implement IPhoton   
        protected ITexture[] _textures = new ITexture[] { null };
        abstract public void Accept(IGeometryVisitor geometryVisitor);

        public IMaterial Material { get; set; }
        public ITexture[] Textures
        {
            get { return _textures; }
            set { }
        }
        #endregion

        #region Implement IAetherSerialization

        public virtual void Save(IAetherWriter writer)
        {
        #if(WINDOWS)
            writer.WriteInt32("Version", 1);

            _leptonImpl.Save(writer);
            writer.WriteParticle("Material", Material);
            writer.WriteParticles("Textures", _textures);
        #endif
        }

        public virtual void Load(IAetherReader reader)
        {
            int version;
            reader.ReadInt32("Version", out version);

            switch (version)
            {
                case 1:
                    _leptonImpl.Load(reader);
                    IAether ae;
                    reader.ReadParticle("Material", out ae); this.Material = (IMaterial)ae;
                    var textures = new List<ITexture>();
                    reader.ReadParticles("Textures", textures); _textures = textures.ToArray();
                    break;
                default:
                    throw new InvalidOperationException("unknown version " + version);
            }
        }
        #endregion

        abstract public float NextRandom();

        abstract public void AddParticle(Vector3 position, Vector3 velocity);
    }
}
