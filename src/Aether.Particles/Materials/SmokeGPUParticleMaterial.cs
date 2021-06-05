using Microsoft.Xna.Framework;

namespace tainicom.Aether.Particles
{
    public class SmokeGPUParticleMaterial : GPUParticleMaterial 
    {
        public SmokeGPUParticleMaterial() : base()
        {
            Gravity = new Vector3(.025f, -.006f, 0); // Create a wind effect by tilting the gravity vector sideways.
            EndVelocity = 0.75f;
            ColorMin = Color.White.ToVector4();
            ColorMax = Color.FromNonPremultiplied(255, 255, 255, 200).ToVector4();
            RotateSpeedMin = -1;
            RotateSpeedMax = 1;
            StartSizeMin = .015f;
            StartSizeMax = .04f;
            EndSizeMin = .09f;
            EndSizeMax = .18f;
        }        
    }
}
