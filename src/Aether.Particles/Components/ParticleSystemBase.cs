#if WINDOWS
using tainicom.Aether.Design.Converters;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Spatial;
using tainicom.Aether.Elementary.Visual;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Core.Spatial;

namespace tainicom.Aether.Particles
{
    public abstract class ParticleSystemBase : 
        ISpatial, 
        IWorldTransform, IWorldTransformUpdateable,
        IVisual, IAetherSerialization
    {
        public ParticleSystemBase()
        {

        }

        #region  Implement ISpatial
        SpatialBase _spatialImpl = new SpatialBase();
        public Matrix LocalTransform { get { return _spatialImpl.LocalTransform; } }
        #if WINDOWS
        [Category("ISpatial")]
        [TypeConverter(typeof(QuaternionEditAsYawPitchRollConverter))]
        #endif
        public Quaternion Rotation
        {
            get { return _spatialImpl.Rotation; }
            set { _spatialImpl.Rotation = value; }
        }
        #if WINDOWS
        [Category("ISpatial")]
        [TypeConverter(typeof(Vector3EditConverter))]
        #endif
        public Vector3 Scale
        {
            get { return _spatialImpl.Scale; }
            set { _spatialImpl.Scale = value; }
        }
        #if WINDOWS
        [Category("ISpatial")]
        [TypeConverter(typeof(Vector3EditConverter))]
        #endif
        public Vector3 Position
        {
            get { return _spatialImpl.Position; }
            set { _spatialImpl.Position = value; }
        }
        #endregion
        

        #region  Implement IWorldTransform, IWorldTransformUpdateable
        public void UpdateWorldTransform(IWorldTransform parentWorldTransform)
        {
            _spatialImpl.UpdateWorldTransform(parentWorldTransform);
        }

        public Matrix WorldTransform 
        { 
            get { return _spatialImpl.WorldTransform; } 
        }
        #endregion


        #region Implement IVisual   
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

            _spatialImpl.Save(writer);
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
                    _spatialImpl.Load(reader);
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
