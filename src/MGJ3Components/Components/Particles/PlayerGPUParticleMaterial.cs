using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Particles;

namespace MGJ3.Components.Particles
{
    public class PlayerGPUParticleMaterial : GPUParticleMaterial
    {
        public PlayerGPUParticleMaterial() : base()
        {
            Duration = TimeSpan.FromSeconds(0.5f);
            DurationRandomness = 1;

            Gravity = new Vector3(0, 0, 0);

            ColorMin = new Color(255, 255, 255, 255).ToVector4();
            ColorMax = new Color(8, 8, 8, 8).ToVector4();

            StartSizeMin = 20f;
            StartSizeMax = 20f;
            EndSizeMin = 40f;
            EndSizeMax = 35f;

            // Use additive blending.
            BlendState = BlendState.Additive;

            //MinHorizontalVelocity = 0;
            //MaxHorizontalVelocity = 15;
            //MinVerticalVelocity = -10;
            //MaxVerticalVelocity = 10;
        }
    }
}
