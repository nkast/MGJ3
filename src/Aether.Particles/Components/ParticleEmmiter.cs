#if WINDOWS
using tainicom.Aether.Design.Converters;
#endif
using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using tainicom.Aether.Core.Managers;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Temporal;
using tainicom.Aether.Elementary.Spatial;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Maths;
using tainicom.Aether.Core.Spatial;

namespace tainicom.Aether.Particles
{
    public class ParticleEmmiter : 
        ISpatial, IWorldTransform, IWorldTransformUpdateable, ILocalTransform, IPosition, ISpatialNode,
        ITemporal, IAetherSerialization
    {
        float timeBetweenParticles;
        float velocitySensitivity;

        Vector3 previousPosition;
        float timeLeftOver;

        #if WINDOWS
        [Category("Emmiter")]
        #endif
        public ParticleSystemBase ParticleSystem { get; set; }
        
        #if WINDOWS
        [Category("Emmiter")]
        #endif
        public int ParticlesPerSecond 
        { 
            get { return (int)Math.Round(1f / timeBetweenParticles); }
            set { timeBetweenParticles = 1f / value; }
        }
        // Controls how much particles are influenced by the velocity of the object
        // which created them. You can see this in action with the explosion effect,
        // where the flames continue to move in the same direction as the source
        // projectile. The projectile trail particles, on the other hand, set this
        // value very low so they are less affected by the velocity of the projectile.
        
        #if WINDOWS
        [Category("Emmiter")]
        #endif
        public float VelocitySensitivity
        {
            get { return velocitySensitivity; }
            set { velocitySensitivity = MathHelper.Clamp(value, 0f, 1f); }
        } 
        
        // Range of values controlling how much X and Z axis velocity to give each
        // particle. Values for individual particles are randomly chosen from somewhere
        // between these limits.
        #if WINDOWS
        [Category("Emmiter")]
        #endif
        public float HorizontalVelocityMax { get; set; }
        #if WINDOWS
        [Category("Emmiter")]
        #endif
        public float HorizontalVelocityMin { get; set; }
        
        // Range of values controlling how much Y axis velocity to give each particle.
        // Values for individual particles are randomly chosen from somewhere between
        // these limits.
        #if WINDOWS
        [Category("Emmiter")]
        #endif
        public float VerticalVelocityMax { get; set; }
        #if WINDOWS
        [Category("Emmiter")]
        #endif
        public float VerticalVelocityMin { get; set; }



        public ParticleEmmiter()
        {
            ParticlesPerSecond = 10;

            VelocitySensitivity = 0f;
            
            HorizontalVelocityMax = 0;
            HorizontalVelocityMin = 0;
            VerticalVelocityMax = 0;
            VerticalVelocityMin = 0;


            //local settings
            HorizontalVelocityMax = 0.020f;
            HorizontalVelocityMin = 0.005f;
            VerticalVelocityMax   = 0.060f;
            VerticalVelocityMin   = 0.040f;
        }

        #region Implement ITemporal
        public virtual void Tick(GameTime gameTime)
        {
            if (ParticleSystem == null) return;
            if (timeBetweenParticles <= 0f) return;

            // Work out how much time has passed since the previous update.
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var worldTransform = SpatialManager.GetWorldTransform(this);
            Vector3 nextPosition = worldTransform.Translation;

            Matrix invWorldTransform = Matrix.Invert(ParticleSystem.WorldTransform);

            if (elapsedTime > 0)
            {
                // Work out how fast we are moving.
                Vector3 velocity = (nextPosition - previousPosition) / elapsedTime;

                // If we had any time left over that we didn't use during the
                // previous update, add that to the current elapsed time.
                float timeToSpend = timeLeftOver + elapsedTime;

                // Counter for looping over the time interval.
                float currentTime = -timeLeftOver;

                // Create particles as long as we have a big enough time interval.                
                while (timeToSpend > timeBetweenParticles)
                {
                    currentTime += timeBetweenParticles;
                    timeToSpend -= timeBetweenParticles;

                    // Work out the optimal position for this particle. This will produce
                    // evenly spaced particles regardless of the object speed, particle
                    // creation frequency, or game update rate.
                    float mu = currentTime / elapsedTime;
                    Vector3 position = Vector3.Lerp(previousPosition, nextPosition, mu);

                    velocity *= velocitySensitivity;

                    // Add in some random amount of vertical velocity.
                    velocity.Y += MathHelper.Lerp(VerticalVelocityMin,
                                                  VerticalVelocityMax,
                                                  ParticleSystem.NextRandom());

                    // Add in some random amount of horizontal velocity.
                    float horizontalVelocity = MathHelper.Lerp(HorizontalVelocityMin,
                                                               HorizontalVelocityMax,
                                                               ParticleSystem.NextRandom());

                    double horizontalAngle = ParticleSystem.NextRandom() * MathHelper.TwoPi;

                    double sin = Math.Sin(horizontalAngle);
                    double cos = Math.Cos(horizontalAngle);
                    if (TauD.QUARTER < horizontalAngle && horizontalAngle < (TauD.HALF + TauD.QUARTER))
                        cos = -cos;
                    velocity.X += horizontalVelocity * (float)cos;
                    velocity.Z += horizontalVelocity * (float)sin;


                    // convert world position to local
                    Vector3.Transform(ref position, ref invWorldTransform, out position);
                    // Create the particle.
                    ParticleSystem.AddParticle(position, velocity);
                }

                // Store any time we didn't use, so it can be part of the next update.
                timeLeftOver = timeToSpend;
            }

            previousPosition = nextPosition;
        }

        #endregion



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

        public Matrix WorldTransform { get { return _spatialImpl.WorldTransform; } }
        #endregion


        #region Implement IAetherSerialization
        public virtual void Save(IAetherWriter writer)
        {
#if(WINDOWS)
            writer.WriteInt32("Version", 1);

            _spatialImpl.Save(writer);
            writer.WriteParticle("ParticleSystem", ParticleSystem);
            writer.WriteFloat("TimeBetweenParticles", timeBetweenParticles);
            writer.WriteFloat("VelocitySensitivity",    velocitySensitivity);
            writer.WriteFloat("HorizontalVelocityMin", HorizontalVelocityMin);
            writer.WriteFloat("HorizontalVelocityMax", HorizontalVelocityMax);
            writer.WriteFloat("VerticalVelocityMin",    VerticalVelocityMin);
            writer.WriteFloat("VerticalVelocityMax",    VerticalVelocityMax);
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
                    IAether ae; float f;
                    reader.ReadParticle("ParticleSystem", out ae); ParticleSystem = ae as ParticleSystemBase;
                    reader.ReadFloat("TimeBetweenParticles", out timeBetweenParticles);
                    reader.ReadFloat("VelocitySensitivity",     out velocitySensitivity);
                    reader.ReadFloat("HorizontalVelocityMin",   out f); HorizontalVelocityMin = f;
                    reader.ReadFloat("HorizontalVelocityMax",   out f); HorizontalVelocityMax =f;
                    reader.ReadFloat("VerticalVelocityMin",     out f); VerticalVelocityMin =f;
                    reader.ReadFloat("VerticalVelocityMax",     out f); VerticalVelocityMax = f;
                    break;
                default:
                    throw new InvalidOperationException("unknown version " + version);
            }
        }
        #endregion


    }
}
