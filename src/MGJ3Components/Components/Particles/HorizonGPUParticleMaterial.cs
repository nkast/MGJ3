using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Particles;

namespace MGJ3.Components.Particles
{
    public class HorizonGPUParticleMaterial : GPUParticleMaterial
    {
        public HorizonGPUParticleMaterial() : base()
        {
            Duration = TimeSpan.FromSeconds(1.5f);
            DurationRandomness = 1;

            Gravity = new Vector3(0, 0.006f, 0); // Set gravity upside down, so the flames will 'fall' upward.

            ColorMin = new Color(255, 255, 255, 255).ToVector4();
            ColorMax = new Color(255, 255, 255, 255).ToVector4();

            StartSizeMin = 10f;
            StartSizeMax = 20f;
            EndSizeMin = 10f;
            EndSizeMax = 20f;

            // Use additive blending.
            BlendState = BlendState.Additive;

            //MinHorizontalVelocity = 0;
            //MaxHorizontalVelocity = 15;
            //MinVerticalVelocity = -10;
            //MaxVerticalVelocity = 10;
        }
    }
}
