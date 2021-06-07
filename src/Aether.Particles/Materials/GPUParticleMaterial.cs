using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using tainicom.Aether.Core.Materials;
using tainicom.Aether.Elementary.Serialization;
#if WINDOWS
using System.ComponentModel;
using tainicom.Aether.Design.Converters;
#endif

namespace tainicom.Aether.Particles
{

    // Material for drawing particles. This computes the particle
    // animation entirely in the vertex shader: no per-particle CPU work required!
    public class GPUParticleMaterial: MaterialBase 
    {

        #region Effect Parameters

        // Shortcuts for accessing frequently changed effect parameters.
        EffectParameter worldViewProjParam;
        EffectParameter projection_22Param;
        EffectParameter viewportScaleParam;
        EffectParameter timeParam;

        EffectParameter durationParam;
        EffectParameter durationRandomnessParam;
        EffectParameter gravityParam;
        EffectParameter endVelocityParam;
        EffectParameter ColorMinParam;
        EffectParameter ColorMaxParam;
        EffectParameter rotateSpeedParam;
        EffectParameter startSizeParam;
        EffectParameter endSizeParam;

        TimeSpan _duration;
        float _durationRandomness;
        Vector3 _gavity;
        float _endVelocity;
        Vector4 _colorMin;
        Vector4 _colorMax;
        Vector2 _rotateSpeed;
        Vector2 _startSize;
        Vector2 _endSize;
        bool isParticleParametersDirty = true;
        #endregion

        #region Fields
        
        #endregion

        #region Public Properties

        // Settings
        // How long these particles will last.
        #if WINDOWS
        [Category("ParticleMaterial")]
        #endif
        public TimeSpan Duration
        {
            get { return _duration; }
            set { _duration = value; isParticleParametersDirty = true; }
        }

        // If greater than zero, some particles will last a shorter time than others.
        #if WINDOWS
        [Category("ParticleMaterial")]
        #endif
        public float DurationRandomness
        {
            get { return _durationRandomness; }
            set { _durationRandomness = value; isParticleParametersDirty = true; }
        }

        // Direction and strength of the gravity effect. Note that this can point in any
        // direction, not just down! The fire effect points it upward to make the flames
        // rise, and the smoke plume points it sideways to simulate wind.
        #if WINDOWS
        [Category("ParticleMaterial")]
        #endif
        public Vector3 Gravity
        {
            get { return _gavity; }
            set { _gavity = value; isParticleParametersDirty = true; }
        }

        // Controls how the particle velocity will change over their lifetime. If set
        // to 1, particles will keep going at the same speed as when they were created.
        // If set to 0, particles will come to a complete stop right before they die.
        // Values greater than 1 make the particles speed up over time.
        #if WINDOWS
        [Category("ParticleMaterial")]
        #endif
        public float EndVelocity
        {
            get { return _endVelocity; }
            set { _endVelocity = value; isParticleParametersDirty = true; }
        }

        // Range of values controlling the particle color and alpha. Values for
        // individual particles are randomly chosen from somewhere between these limits. 
        #if WINDOWS
        [Category("ParticleMaterial"), TypeConverter(typeof(Vector4EditAsColorConverter))]
        #endif 
        public Vector4 ColorMin
        {
            get { return _colorMin; }
            set { _colorMin = value; isParticleParametersDirty = true; }
        }
        #if WINDOWS
        [Category("ParticleMaterial"), TypeConverter(typeof(Vector4EditAsColorConverter))]
        #endif
        public Vector4 ColorMax
        {
            get { return _colorMax; }
            set { _colorMax = value; isParticleParametersDirty = true; }
        }
        
        // Range of values controlling how fast the particles rotate. Values for
        // individual particles are randomly chosen from somewhere between these
        // limits. If both these values are set to 0, the particle system will
        // automatically switch to an alternative shader technique that does not
        // support rotation, and thus requires significantly less GPU power. This
        // means if you don't need the rotation effect, you may get a performance
        // boost from leaving these values at 0.
        #if WINDOWS
        [Category("ParticleMaterial")]
        #endif
        public float RotateSpeedMin
        {
            get { return _rotateSpeed.X; }
            set { _rotateSpeed.X = value; isParticleParametersDirty = true; }
        }
        #if WINDOWS
        [Category("ParticleMaterial")]
        #endif
        public float RotateSpeedMax
        {
            get { return _rotateSpeed.Y; }
            set { _rotateSpeed.Y = value; isParticleParametersDirty = true; }
        }

        // Range of values controlling how big the particles are when first created.
        // Values for individual particles are randomly chosen from somewhere between
        // these limits.
        #if WINDOWS
        [Category("ParticleMaterial")]
        #endif
        public float StartSizeMin
        {
            get { return _startSize.X; }
            set { _startSize.X = value; isParticleParametersDirty = true; }
        }
        #if WINDOWS
        [Category("ParticleMaterial")]
        #endif
        public float StartSizeMax
        {
            get { return _startSize.Y; }
            set { _startSize.Y = value; isParticleParametersDirty = true; }
        }

        // Range of values controlling how big particles become at the end of their
        // life. Values for individual particles are randomly chosen from somewhere
        // between these limits.
        #if WINDOWS
        [Category("ParticleMaterial")]
        #endif
        public float EndSizeMin
        {
            get { return _endSize.X; }
            set { _endSize.X = value; isParticleParametersDirty = true; }
        }
        #if WINDOWS
        [Category("ParticleMaterial")]
        #endif
        public float EndSizeMax
        {
            get { return _endSize.Y; }
            set { _endSize.Y = value; isParticleParametersDirty = true; }
        }

        #endregion

        internal protected float Time
        {
            get { return timeParam.GetValueSingle(); }
            set { timeParam.SetValue(value); }
        }

        string GetResourceName()
        {
#if XNA
            return "tainicom.Aether.Particles.Effects.ParticleEffect.WinReach.bin";
#else
            switch(PlatformInfo.GraphicsBackend)
            {
                case GraphicsBackend.DirectX:
                    return "tainicom.Aether.Particles.Effects.ParticleEffect.dx11.mgfxo";
                case GraphicsBackend.OpenGL:
                    return "tainicom.Aether.Particles.Effects.ParticleEffect.ogl.mgfxo";
                default:
                    throw new InvalidOperationException();
            }
#endif
        }

        internal static byte[] LoadEffectResource(string name)
        {
            using (var stream = LoadEffectResourceStream(name))
            {
                var bytecode = new byte[stream.Length];
                stream.Read(bytecode, 0, (int)stream.Length);
                return bytecode;
            }
        }

        internal static Stream LoadEffectResourceStream(string name)
        {
            Stream stream = GetAssembly(typeof(GPUParticleMaterial)).GetManifestResourceStream(name);
            return stream;
        }
        
        private static Assembly GetAssembly(Type type)
        {
            #if NETFX_CORE || ANDROID || __IOS__ // || W8_1 || W10
            return  type.GetTypeInfo().Assembly;
            #else
            return type.Assembly;
            #endif
        }

        internal Effect ParticleEffect { get { return _effect; } } 
        
        public GPUParticleMaterial() : base()
        {
            BlendState = BlendState.NonPremultiplied;
            DepthStencilState = DepthStencilState.DepthRead;
            
            Duration = TimeSpan.FromSeconds(2);
            DurationRandomness = 0;

            Gravity = Vector3.Zero;

            ColorMin = new Color(255, 255, 255, 255).ToVector4();
            ColorMax = new Color(255, 255, 255, 0).ToVector4();

            StartSizeMin = .015f;
            StartSizeMax = .020f;
            EndSizeMin = .15f;
            EndSizeMax = .20f;
        }
        
        protected override void CreateEffect()
        {
            if (_effect != null) return; //allready initialized?
            byte[] effectCode = LoadEffectResource(GetResourceName());
            _effect = new Effect(this.GraphicsDevice, effectCode);

            CacheEffectParameters();
        }
        
        private void CacheEffectParameters()
        {
            EffectParameterCollection parameters = _effect.Parameters;
            
            // Look up shortcuts for parameters that change every frame.
            worldViewProjParam  = parameters["WorldViewProj"];
            projection_22Param  = parameters["Projection_22"];
            viewportScaleParam  = parameters["ViewportScale"];
            timeParam           = parameters["CurrentTime"];
            
            durationParam       = parameters["Duration"];
            durationRandomnessParam = parameters["DurationRandomness"];
            gravityParam        = parameters["Gravity"];
            endVelocityParam    = parameters["EndVelocity"];
            ColorMinParam       = parameters["ColorMin"];
            ColorMaxParam       = parameters["ColorMax"];

            rotateSpeedParam    = parameters["RotateSpeed"];
            startSizeParam      = parameters["StartSize"];
            endSizeParam        = parameters["EndSize"];
        }

        public override void Apply()
        {
            var worldViewProj = World * View * Projection;
            worldViewProjParam.SetValue(worldViewProj);
            projection_22Param.SetValue(Projection.M22);

            // Set an effect parameter describing the viewport size. This is
            // needed to convert particle sizes into screen space point sizes.
            viewportScaleParam.SetValue(new Vector2(0.5f / GraphicsDevice.Viewport.AspectRatio, -0.5f));

            // Apply particle settings
            if (isParticleParametersDirty)
            {
                durationParam.SetValue((float)_duration.TotalSeconds);
                durationRandomnessParam.SetValue(_durationRandomness);
                gravityParam.SetValue(_gavity);
                endVelocityParam.SetValue(_endVelocity);
                ColorMinParam.SetValue(_colorMin);
                ColorMaxParam.SetValue(_colorMax);
                rotateSpeedParam.SetValue(_rotateSpeed);
                startSizeParam.SetValue(_startSize);
                endSizeParam.SetValue(_endSize);
                isParticleParametersDirty = false;
            }

            base.Apply();
        }


        
        #region Aether.Elementary.Serialization.IAetherSerialization Members

        public override void Save(IAetherWriter writer)
        {
#if (WINDOWS)
            writer.WriteInt32("Version", 1);

            base.Save(writer);
            writer.WriteTimeSpan("Duration", Duration);
            writer.WriteFloat("DurationRandomness", DurationRandomness);
            writer.WriteVector3("Gravity", Gravity);
            writer.WriteFloat("EndVelocity", EndVelocity);
            writer.WriteVector4("ColorMin", ColorMin);
            writer.WriteVector4("ColorMax", ColorMax);
            writer.WriteFloat("RotateSpeedMin", RotateSpeedMin);
            writer.WriteFloat("RotateSpeedMax", RotateSpeedMax);
            writer.WriteFloat("StartSizeMin", StartSizeMin);
            writer.WriteFloat("StartSizeMax", StartSizeMax);
            writer.WriteFloat("EndSizeMin", EndSizeMin);
            writer.WriteFloat("EndSizeMax", EndSizeMax);
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
                    float f; TimeSpan ts; Vector3 v3; Vector4 v4;
                    reader.ReadTimeSpan("Duration", out ts); Duration = ts;
                    reader.ReadFloat("DurationRandomness", out f); DurationRandomness = f;
                    reader.ReadVector3("Gravity", out v3); Gravity = v3;
                    reader.ReadFloat("EndVelocity", out f); EndVelocity = f;
                    reader.ReadVector4("ColorMin", out v4); ColorMin = v4;
                    reader.ReadVector4("ColorMax", out v4); ColorMax = v4;
                    reader.ReadFloat("RotateSpeedMin", out f); RotateSpeedMin = f;
                    reader.ReadFloat("RotateSpeedMax", out f); RotateSpeedMax = f;
                    reader.ReadFloat("StartSizeMin", out f); StartSizeMin = f;
                    reader.ReadFloat("StartSizeMax", out f); StartSizeMax = f;
                    reader.ReadFloat("EndSizeMin", out f); EndSizeMin = f;
                    reader.ReadFloat("EndSizeMax", out f); EndSizeMax = f;
                    break;
                default:
                    throw new InvalidOperationException("unknown version "+ version);
            }
        }
        #endregion
                
    }
}
