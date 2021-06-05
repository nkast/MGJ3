using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Particles
{
    public class FireGPUParticleMaterial : GPUParticleMaterial
    {
        public FireGPUParticleMaterial() : base()
        {
            Duration = TimeSpan.FromSeconds(2);
            DurationRandomness = 1;

            Gravity = new Vector3(0, 0.006f, 0); // Set gravity upside down, so the flames will 'fall' upward.

            ColorMin = new Color(255, 255, 80, 255).ToVector4();
            ColorMax = new Color(255, 255, 80, 64).ToVector4();

            StartSizeMin = .007f;
            StartSizeMax = .008f;
            EndSizeMin = .001f;
            EndSizeMax = .002f;

            // Use additive blending.
            BlendState = BlendState.Additive;

            //MinHorizontalVelocity = 0;
            //MaxHorizontalVelocity = 15;
            //MinVerticalVelocity = -10;
            //MaxVerticalVelocity = 10;
        }
    }
}
