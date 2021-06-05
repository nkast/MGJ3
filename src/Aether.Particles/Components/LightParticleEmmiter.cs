using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
#if WINDOWS
using tainicom.Aether.Design.Converters;
#endif
using tainicom.Aether.Elementary.Photons;
using tainicom.Aether.Elementary.Serialization;

namespace tainicom.Aether.Particles
{
    public class LightParticleEmmiter : ParticleEmmiter, ILightSource 
    {

        #region Implement ILightSource
		#if WINDOWS
        [Category("ILightSource"), TypeConverter(typeof(Vector3EditAsColorConverter))]
        #endif
        public Vector3 LightSourceColor { get; set; }
        #if WINDOWS
        [Category("ILightSource")]
        #endif
        public float Intensity { get; set; }
        #if WINDOWS
        [Category("ILightSource")]
        #endif
        public float MaximumRadius { get; set; }
        public Vector3 PremultiplyColor { get { return this.LightSourceColor * this.Intensity; } }
        #endregion

        public LightParticleEmmiter(): base()
        {
            LightSourceColor = Vector3.One;
            Intensity = 1;
            MaximumRadius = float.MaxValue;
        }


        #region Implement IAetherSerialization
        public override void Save(IAetherWriter writer)
        {
#if(WINDOWS)
            writer.WriteInt32("Version", 1);

            base.Save(writer);
            writer.WriteVector3("LightSourceColor", LightSourceColor);
            writer.WriteFloat("Intensity", Intensity);
            writer.WriteFloat("MaximumRadius", MaximumRadius);
#endif
        }

        public override void Load(IAetherReader reader)
        {
            int version;
            reader.ReadInt32("Version", out version);

            switch (version)
            {
                case 1:
                    base.Load(reader);
                    Vector3 v3; float f;
                    reader.ReadVector3("LightSourceColor", out v3); LightSourceColor = v3;
                    reader.ReadFloat("Intensity", out f); Intensity = f;
                    reader.ReadFloat("MaximumRadius", out f); MaximumRadius = f;
                    break;
                default:
                    throw new InvalidOperationException("unknown version " + version);
            }           
        }
        #endregion

    }
}
