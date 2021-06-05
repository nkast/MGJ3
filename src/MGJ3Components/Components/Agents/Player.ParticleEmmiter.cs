using System;
using tainicom.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Maths;
using tainicom.Aether.Particles;
//using tainicom.Aether.Factories;
//using tainicom.Aether.Physics2D.Dynamics;

namespace MGJ3.Components
{
    public partial class Player
    {
        
        float timeBetweenParticles;
        float velocitySensitivity;

        Vector3 previousPosition;
        float timeLeftOver;

        #if WINDOWS
        [System.ComponentModel.Category("Emmiter")]
        #endif
        public ParticleSystemBase ParticleSystem { get; set; }
        
        #if WINDOWS
        [System.ComponentModel.Category("Emmiter")]
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
        [System.ComponentModel.Category("Emmiter")]
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
        [System.ComponentModel.Category("Emmiter")]
        #endif
        public float HorizontalVelocityMax { get; set; }
        #if WINDOWS
        [System.ComponentModel.Category("Emmiter")]
        #endif
        public float HorizontalVelocityMin { get; set; }
        
        // Range of values controlling how much Y axis velocity to give each particle.
        // Values for individual particles are randomly chosen from somewhere between
        // these limits.
        #if WINDOWS
        [System.ComponentModel.Category("Emmiter")]
        #endif
        public float VerticalVelocityMax { get; set; }
        #if WINDOWS
        [System.ComponentModel.Category("Emmiter")]
        #endif
        public float VerticalVelocityMin { get; set; }


        public Player()
        {
            ParticlesPerSecond = 120;

            VelocitySensitivity = 1f;

            HorizontalVelocityMax = 0;
            HorizontalVelocityMin = 0;
            VerticalVelocityMax = 0;
            VerticalVelocityMin = 0;

            
            //local settings
            HorizontalVelocityMax = 0.5f;
            HorizontalVelocityMin = 0.2f;
            VerticalVelocityMax = 0.6f;
            VerticalVelocityMin = 0.4f;
        }

        public void TickParticleEmmiter(GameTime gameTime)
        {
            if (ParticleSystem == null) return;
            if (timeBetweenParticles <= 0f) return;

            // Work out how much time has passed since the previous update.
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime > 0)
            {
                // Work out how fast we are moving.
                //Vector3 velocity = (Position - previousPosition) / elapsedTime;
                Vector3 velocity = Vector3.Zero;

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
                    Vector3 position = Vector3.Lerp(previousPosition, Position, mu);
                    //Vector3 position = Position;

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


                    // Create the particle.
                    ParticleSystem.AddParticle(position, velocity);
                }

                // Store any time we didn't use, so it can be part of the next update.
                timeLeftOver = timeToSpend;
            }

            previousPosition = Position;
        }


        
        #region Implement IAetherSerialization
        public void SaveParticleEmmiter(IAetherWriter writer)
        {
            writer.WriteParticle("ParticleSystem", ParticleSystem);
            writer.WriteFloat("TimeBetweenParticles", timeBetweenParticles);
            writer.WriteFloat("VelocitySensitivity",    velocitySensitivity);
            writer.WriteFloat("MinHorizontalVelocity",  HorizontalVelocityMin);
            writer.WriteFloat("MaxHorizontalVelocity",  HorizontalVelocityMax);
            writer.WriteFloat("VerticalVelocityMin",    VerticalVelocityMin);
            writer.WriteFloat("VerticalVelocityMax",    VerticalVelocityMax);
        }

        public void LoadParticleEmmiter(IAetherReader reader)
        {
            IAether ae; float f;
            reader.ReadParticle("ParticleSystem", out ae); ParticleSystem = (ParticleSystemBase)ae;
            reader.ReadFloat("TimeBetweenParticles", out timeBetweenParticles);                        
            reader.ReadFloat("VelocitySensitivity",     out velocitySensitivity);
            reader.ReadFloat("MinHorizontalVelocity",   out f); HorizontalVelocityMin = f;
            reader.ReadFloat("MaxHorizontalVelocity",   out f); HorizontalVelocityMax =f;
            reader.ReadFloat("VerticalVelocityMin",     out f); VerticalVelocityMin =f;
            reader.ReadFloat("VerticalVelocityMax",     out f); VerticalVelocityMax = f;
        }
        #endregion


    }
}
