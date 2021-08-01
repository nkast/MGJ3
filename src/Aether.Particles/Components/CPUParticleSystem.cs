#if WINDOWS
using System.ComponentModel;
using tainicom.Aether.Design.Converters;
#endif
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Core.Materials;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Temporal;
using tainicom.Aether.Elementary.Visual;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.MonoGame;
using tainicom.Aether.Particles.VertexTypes;

namespace tainicom.Aether.Particles
{
    public class CPUParticleSystem : ParticleSystemBase, ITemporal, IInitializable, IAetherSerialization
    {
        #region Fields

        // For loading the effect and particle texture.
        protected AetherEngine engine;
        protected ContentManager content;
        protected GraphicsDevice device;


        // Shortcuts for accessing frequently changed effect parameters.
        //EffectParameter viewParam;
        //EffectParameter projectionParam;
        //EffectParameter viewportScaleParam;
        //EffectParameter timeParam;

        // An array of particles, treated as a circular queue.
        ParticleVertex[] particles;

        // A vertex buffer holding our particles. This contains the same data as
        // the particles array, but copied across to where the GPU can access it.        
        VertexPositionColorTexture[] _vertices;


        // Index buffer turns sets of four vertices into particle quads (pairs of triangles).
        short[] _indices;


        // The particles array and vertex buffer are treated as a circular queue.
        // Initially, the entire contents of the array are free, because no particles
        // are in use. When a new particle is created, this is allocated from the
        // beginning of the array. If more than one particle is created, these will
        // always be stored in a consecutive block of array elements. Because all
        // particles last for the same amount of time, old particles will always be
        // removed in order from the start of this active particle region, so the
        // active and free regions will never be intermingled. Because the queue is
        // circular, there can be times when the active particle region wraps from the
        // end of the array back to the start. The queue uses modulo arithmetic to
        // handle these cases. For instance with a four entry queue we could have:
        //
        //      0
        //      1 - first active particle
        //      2 
        //      3 - first free particle
        //
        // In this case, particles 1 and 2 are active, while 3 and 4 are free.
        // Using modulo arithmetic we could also have:
        //
        //      0
        //      1 - first free particle
        //      2 
        //      3 - first active particle
        //
        // Here, 3 and 0 are active, while 1 and 2 are free.
        //
        // But wait! The full story is even more complex.
        //
        // When we create a new particle, we add them to our managed particles array.
        // We also need to copy this new data into the GPU vertex buffer, but we don't
        // want to do that straight away, because setting new data into a vertex buffer
        // can be an expensive operation. If we are going to be adding several particles
        // in a single frame, it is faster to initially just store them in our managed
        // array, and then later upload them all to the GPU in one single call. So our
        // queue also needs a region for storing new particles that have been added to
        // the managed array but not yet uploaded to the vertex buffer.
        //
        // Another issue occurs when old particles are retired. The CPU and GPU run
        // asynchronously, so the GPU will often still be busy drawing the previous
        // frame while the CPU is working on the next frame. This can cause a
        // synchronization problem if an old particle is retired, and then immediately
        // overwritten by a new one, because the CPU might try to change the contents
        // of the vertex buffer while the GPU is still busy drawing the old data from
        // it. Normally the graphics driver will take care of this by waiting until
        // the GPU has finished drawing inside the VertexBuffer.SetData call, but we
        // don't want to waste time waiting around every time we try to add a new
        // particle! To avoid this delay, we can specify the SetDataOptions.NoOverwrite
        // flag when we write to the vertex buffer. This basically means "I promise I
        // will never try to overwrite any data that the GPU might still be using, so
        // you can just go ahead and update the buffer straight away". To keep this
        // promise, we must avoid reusing vertices immediately after they are drawn.
        //
        // So in total, our queue contains four different regions:
        //
        // Vertices between firstActiveParticle and firstNewParticle are actively
        // being drawn, and exist in both the managed particles array and the GPU
        // vertex buffer.
        //
        // Vertices between firstNewParticle and firstFreeParticle are newly created,
        // and exist only in the managed particles array. These need to be uploaded
        // to the GPU at the start of the next draw call.
        //
        // Vertices between firstFreeParticle and firstRetiredParticle are free and
        // waiting to be allocated.
        //
        // Vertices between firstRetiredParticle and firstActiveParticle are no longer
        // being drawn, but were drawn recently enough that the GPU could still be
        // using them. These need to be kept around for a few more frames before they
        // can be reallocated.

        int firstActiveParticle;
        int firstNewParticle;
        int firstFreeParticle;
        int firstRetiredParticle;


        // Store the current time, in seconds.
        float currentTime;


        // Count how many times Draw has been called. This is used to know
        // when it is safe to retire old particles back into the free list.
        int drawCounter;


        // Shared random number generator.
        static Random random = new Random();



        
        #endregion

        #region properties
        
        
        int maxParticles;
        // Maximum number of particles that can be displayed at one time.
        public int MaxParticles 
        {
            get { return maxParticles; }
            set 
            { 
                maxParticles = value;
                firstActiveParticle = 0;
                firstNewParticle = 0;
                firstFreeParticle = 0;
                firstRetiredParticle = 0;
                InitializeParticles();
            }
        }

        //settings
        // How long these particles will last.
        public TimeSpan Duration { get ; set; }

        // If greater than zero, some particles will last a shorter time than others.
        public float DurationRandomness { get; set; }

        // Direction and strength of the gravity effect. Note that this can point in any
        // direction, not just down! The fire effect points it upward to make the flames
        // rise, and the smoke plume points it sideways to simulate wind.
        public Vector3 Gravity { get; set; }

        // Controls how the particle velocity will change over their lifetime. If set
        // to 1, particles will keep going at the same speed as when they were created.
        // If set to 0, particles will come to a complete stop right before they die.
        // Values greater than 1 make the particles speed up over time.
        public float EndVelocity { get; set; }

        // Range of values controlling the particle color and alpha. Values for
        // individual particles are randomly chosen from somewhere between these limits.  
        #if WINDOWS
        [Category("ParticleSystem"), TypeConverter(typeof(Vector4EditAsColorConverter))]
        #endif
        public Vector4 ColorMin { get; set; }
        #if WINDOWS
        [Category("ParticleSystem"), TypeConverter(typeof(Vector4EditAsColorConverter))]
        #endif
        public Vector4 ColorMax { get; set; }


        // Range of values controlling how fast the particles rotate. Values for
        // individual particles are randomly chosen from somewhere between these
        // limits. If both these values are set to 0, the particle system will
        // automatically switch to an alternative shader technique that does not
        // support rotation, and thus requires significantly less GPU power. This
        // means if you don't need the rotation effect, you may get a performance
        // boost from leaving these values at 0.
        #if WINDOWS
        [Category("ParticleSystem")]
        #endif
        public float RotateSpeedMin { get; set; }
        #if WINDOWS
        [Category("ParticleSystem")]
        #endif
        public float RotateSpeedMax { get; set; }


        // Range of values controlling how big the particles are when first created.
        // Values for individual particles are randomly chosen from somewhere between
        // these limits.
        #if WINDOWS
        [Category("ParticleSystem")]
        #endif
        public float StartSizeMin { get; set; }
        #if WINDOWS
        [Category("ParticleSystem")]
        #endif
        public float StartSizeMax { get; set; }


        // Range of values controlling how big particles become at the end of their
        // life. Values for individual particles are randomly chosen from somewhere
        // between these limits.
        #if WINDOWS
        [Category("ParticleSystem")]
        #endif
        public float EndSizeMin { get; set; }
        #if WINDOWS
        [Category("ParticleSystem")]
        #endif
        public float EndSizeMax { get; set; }
        
        // Alpha blending settings.
        public BlendState BlendState { get; set; }


        #endregion
        
        public CPUParticleSystem() : base()
        {
            maxParticles = 256;

            Duration = TimeSpan.FromSeconds(5);
            DurationRandomness = 0;

            Gravity = Vector3.Zero;

            EndVelocity = 1;
            ColorMin = Color.White.ToVector4();
            ColorMax = Color.White.ToVector4();

            RotateSpeedMin = 0;
            RotateSpeedMax = 0;

            StartSizeMin = 100;
            StartSizeMax = 100;
            EndSizeMin = 100;
            EndSizeMax = 100;

            BlendState = BlendState.NonPremultiplied;
        }

        public virtual void Initialize(AetherEngine engine)
        {
            this.engine = engine;
            this.device = AetherContextMG.GetDevice(engine);
            this.content = AetherContextMG.GetContent(engine);
            
            InitializeParticles();
        }

        private void InitializeParticles()
        {
            //dispose previous buffers
            if (_vertices != null) _vertices = null;
            if (_indices != null) _indices = null;
            
            // Allocate the particle array, and fill in the corner fields (which never change).
            particles = new ParticleVertex[MaxParticles * 4];

            for (int i = 0; i < MaxParticles; i++)
            {
                particles[i * 4 + 0].Corner = new Vector2(-1, -1);
                particles[i * 4 + 1].Corner = new Vector2(1, -1);
                particles[i * 4 + 2].Corner = new Vector2(1, 1);
                particles[i * 4 + 3].Corner = new Vector2(-1, 1);
            }

            // Create a dynamic vertex buffer.
            _vertices = new VertexPositionColorTexture[MaxParticles * 4];

            // Create and populate the index buffer.
            short[] indices = new short[MaxParticles * 6];

            for (int i = 0; i < MaxParticles; i++)
            {
                indices[i * 6 + 0] = (short)(i * 4 + 0);
                indices[i * 6 + 1] = (short)(i * 4 + 1);
                indices[i * 6 + 2] = (short)(i * 4 + 2);

                indices[i * 6 + 3] = (short)(i * 4 + 0);
                indices[i * 6 + 4] = (short)(i * 4 + 2);
                indices[i * 6 + 5] = (short)(i * 4 + 3);
            }

            _indices = indices;
        }
        
        public void Tick(GameTime gameTime)
        {
            currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            RetireActiveParticles();
            FreeRetiredParticles();

            // If we let our timer go on increasing for ever, it would eventually
            // run out of floating point precision, at which point the particles
            // would render incorrectly. An easy way to prevent this is to notice
            // that the time value doesn't matter when no particles are being drawn,
            // so we can reset it back to zero any time the active queue is empty.

            if (firstActiveParticle == firstFreeParticle)
                currentTime = 0;

            if (firstRetiredParticle == firstActiveParticle)
                drawCounter = 0;
        }
        
        /// <summary>
        /// Helper for checking when active particles have reached the end of
        /// their life. It moves old particles from the active area of the queue
        /// to the retired section.
        /// </summary>
        void RetireActiveParticles()
        {
            float particleDuration = (float)Duration.TotalSeconds;

            while (firstActiveParticle != firstNewParticle)
            {
                // Is this particle old enough to retire?
                // We multiply the active particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                float particleAge = currentTime - particles[firstActiveParticle * 4].Time;

                if (particleAge < particleDuration)
                    break;

                // Remember the time at which we retired this particle.
                particles[firstActiveParticle * 4].Time = drawCounter;

                // Move the particle from the active to the retired queue.
                firstActiveParticle++;

                if (firstActiveParticle >= MaxParticles)
                    firstActiveParticle = 0;
            }
        }


        /// <summary>
        /// Helper for checking when retired particles have been kept around long
        /// enough that we can be sure the GPU is no longer using them. It moves
        /// old particles from the retired area of the queue to the free section.
        /// </summary>
        void FreeRetiredParticles()
        {
            while (firstRetiredParticle != firstActiveParticle)
            {
                // Has this particle been unused long enough that
                // the GPU is sure to be finished with it?
                // We multiply the retired particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                int age = drawCounter - (int)particles[firstRetiredParticle * 4].Time;

                // The GPU is never supposed to get more than 2 frames behind the CPU.
                // We add 1 to that, just to be safe in case of buggy drivers that
                // might bend the rules and let the GPU get further behind.
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                firstRetiredParticle++;

                if (firstRetiredParticle >= MaxParticles)
                    firstRetiredParticle = 0;
            }
        }

        public override void Accept(IGeometryVisitor geometryVisitor)
        {   
            // If there are any particles waiting in the newly added queue,
            // we'd better upload them to the GPU ready for drawing.
            if (firstNewParticle != firstFreeParticle)
            {
                AddNewParticlesToVertexBuffer();

                // Move the particles we just uploaded from the new to the active queue.
                firstNewParticle = firstFreeParticle;
            }

            var view = ((IShaderMatrices)geometryVisitor).View;
            var projection = ((IShaderMatrices)geometryVisitor).Projection;

            // If there are any active particles, draw them now!
            if (firstActiveParticle != firstFreeParticle)
            {
                BasicMaterial material = Material as BasicMaterial;
                if (material == null) return;

                // Set the values of parameters 
                material.BlendState = this.BlendState;
                
                if (firstActiveParticle < firstFreeParticle)
                {
                    UpdateParticles(engine.TotalTime, view, projection, firstActiveParticle, (firstFreeParticle - firstActiveParticle));

                    // If the active particles are all in one consecutive range,
                    // we can draw them all in a single call.
                    geometryVisitor.SetVertices( this,
                        _vertices, firstActiveParticle * 4, (firstFreeParticle - firstActiveParticle) * 4,
                        _indices,  firstActiveParticle * 6, (firstFreeParticle - firstActiveParticle) * 2,
                        VertexPositionColorTexture.VertexDeclaration);
                }
                else
                {
                    UpdateParticles(engine.TotalTime, view, projection, firstActiveParticle, (MaxParticles - firstActiveParticle));

                    // If the active particle range wraps past the end of the queue
                    // back to the start, we must call two draws.                    
                    geometryVisitor.SetVertices(this,
                       _vertices, firstActiveParticle * 4, (MaxParticles - firstActiveParticle) * 4,
                       _indices,  firstActiveParticle * 6, (MaxParticles - firstActiveParticle) * 2,
                       VertexPositionColorTexture.VertexDeclaration);

                    if (firstFreeParticle > 0)
                    {
                        UpdateParticles(engine.TotalTime, view, projection, 0, firstFreeParticle);

                        geometryVisitor.SetVertices(this,
                            _vertices, 0, firstFreeParticle * 4,
                            _indices,  0, firstFreeParticle * 2,
                            VertexPositionColorTexture.VertexDeclaration);
                    }
                }
            }

            drawCounter++;
            return;
        }

        private void UpdateParticles(float CurrentTime, Matrix view, Matrix projection, int first, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var input = particles[first + i];
                
                // Compute the age of the particle.
                float age = CurrentTime - input.Time;

                Vector4 Random = input.Random.ToVector4();
                // Apply a random factor to make different particles age at different rates.
                age *= 1 + Random.X * DurationRandomness;
                
                // Normalize the age into the range zero to one.
                float normalizedAge = MathHelper.Clamp((float)(age / Duration.TotalSeconds), 0, 1);

                var position = ComputeParticlePosition(view, projection, input.Position, input.Velocity,
                                              age, normalizedAge);

                float size = ComputeParticleSize(projection, Random.Y, normalizedAge);

                Matrix rotation = ComputeParticleRotation(Random.W, age);

                var viewportScale = new Vector4(0.5f / this.device.Viewport.AspectRatio, -0.5f,1,1);
                
                //output.Position.xy += mul(input.Corner, rotation) * size * ViewportScale;
                position = Vector4.Transform(position, rotation) * size * viewportScale;

                var v = (first + i) * 4;
                _vertices[v + 0] = new VertexPositionColorTexture(
                    
                    );
                _vertices[v + 1] = new VertexPositionColorTexture(

                    );
                _vertices[v + 2] = new VertexPositionColorTexture(

                    );
                _vertices[v + 3] = new VertexPositionColorTexture(

                    );
            }


                //material.Duration = Duration;
                //material.DurationRandomness = DurationRandomness;
                //material.Gravity = Gravity;
                //material.EndVelocity = EndVelocity;
                //material.MinColor = MinColor;
                //material.MaxColor = MaxColor;
                //material.RotateSpeed = new Vector2(MinRotateSpeed, MaxRotateSpeed);
                //material.StartSize = new Vector2(MinStartSize, MaxStartSize);
                //material.EndSize = new Vector2(MinEndSize, MaxEndSize);

            // Set an effect parameter describing the current time. All the vertex
            // shader particle animation is keyed off this value.
            //material.Time = currentTime;
        }

        // Vertex shader helper for computing the rotation of a particle.
        Matrix ComputeParticleRotation(float randomValue, float age)
        {
            var RotateSpeed = new Vector2(RotateSpeedMin, RotateSpeedMax);

            // Apply a random factor to make each particle rotate at a different speed.
            float rotateSpeed = MathHelper.Lerp(RotateSpeed.X, RotateSpeed.Y, randomValue);

            float rotation = rotateSpeed * age;

            // Compute a 2x2 rotation matrix.
            //float c = (float)Math.Cos(rotation);
            //float s = (float)Math.Sin(rotation);
            //return float2x2(c, -s, s, c);
            return Matrix.CreateRotationZ(rotation);
        }

        private Vector4 ComputeParticlePosition(Matrix view, Matrix projection, Vector3 position, Vector3 velocity,
                               float age, float normalizedAge)
        {
            float startVelocity = velocity.Length();

            // Work out how fast the particle should be moving at the end of its life,
            // by applying a constant scaling factor to its starting velocity.
            float endVelocity = startVelocity * EndVelocity;

            // Our particles have constant acceleration, so given a starting velocity
            // S and ending velocity E, at time T their velocity should be S + (E-S)*T.
            // The particle position is the sum of this velocity over the range 0 to T.
            // To compute the position directly, we must integrate the velocity
            // equation. Integrating S + (E-S)*T for T produces S*T + (E-S)*T*T/2.

            float velocityIntegral = startVelocity * normalizedAge +
                                     (endVelocity - startVelocity) * normalizedAge *
                                                                     normalizedAge / 2;

            position += Vector3.Normalize(velocity) * velocityIntegral * (float)Duration.TotalSeconds;

            // Apply the gravitational force.
            position += Gravity * age * normalizedAge;

            // Apply the camera view and projection transforms.
            //return mul(mul(float4(position, 1), View), Projection);
            var position4 = new Vector4(position, 1);
            position4 = Vector4.Transform(position4, view);
            position4 = Vector4.Transform(position4, projection);
            return position4;
        }

        private float ComputeParticleSize(Matrix projection, float randomValue, float normalizedAge)
        {
            var StartSize = new Vector2(StartSizeMin, StartSizeMax);
            var EndSize = new Vector2(EndSizeMin, EndSizeMax);

            // Apply a random factor to make each particle a slightly different size.
            float startSize = MathHelper.Lerp(StartSize.X, StartSize.Y, randomValue);
            float endSize = MathHelper.Lerp(EndSize.X, EndSize.Y, randomValue);

            // Compute the actual size based on the age of the particle.
            float size = MathHelper.Lerp(startSize, endSize, normalizedAge);

            // Project the size into screen coordinates.
            return size*projection.M11;
        }

        /// <summary>
        /// Helper for uploading new particles from our managed
        /// array to the GPU vertex buffer.
        /// </summary>
        private void AddNewParticlesToVertexBuffer()
        {
            int stride = ParticleVertex.VertexDeclaration.VertexStride;

            //if (firstNewParticle < firstFreeParticle)
            //{
            //    // If the new particles are all in one consecutive range,
            //    // we can upload them all in a single call.
            //    vertexBuffer.SetData(firstNewParticle * stride * 4, particles,
            //                         firstNewParticle * 4,
            //                         (firstFreeParticle - firstNewParticle) * 4,
            //                         stride, SetDataOptions.NoOverwrite);
            //}
            //else
            //{
            //    // If the new particle range wraps past the end of the queue
            //    // back to the start, we must split them over two upload calls.
            //    vertexBuffer.SetData(firstNewParticle * stride * 4, particles,
            //                         firstNewParticle * 4,
            //                         (MaxParticles - firstNewParticle) * 4,
            //                         stride, SetDataOptions.NoOverwrite);

            //    if (firstFreeParticle > 0)
            //    {
            //        vertexBuffer.SetData(0, particles,
            //                             0, firstFreeParticle * 4,
            //                             stride, SetDataOptions.NoOverwrite);
            //    }
            //}

            // Move the particles we just uploaded from the new to the active queue.
            firstNewParticle = firstFreeParticle;
        }

        public override void AddParticle(Vector3 position, Vector3 velocity)
        {
            // system might fail if velocity length is zero.
            if (velocity == Vector3.Zero)
                velocity.Y = 0.000001f;

            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = firstFreeParticle + 1;

            if (nextFreeParticle >= MaxParticles)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == firstRetiredParticle)
                return;
            
            // Choose four random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            Color randomValues = new Color((byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255));

            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                particles[firstFreeParticle * 4 + i].Position = position;
                particles[firstFreeParticle * 4 + i].Velocity = velocity;
                particles[firstFreeParticle * 4 + i].Random = randomValues;
                particles[firstFreeParticle * 4 + i].Time = currentTime;
            }

            firstFreeParticle = nextFreeParticle;
        }
        
        public override float NextRandom()
        {
            return (float)random.NextDouble();
        }
        
        #region Implement IAetherSerialization

        public override void Save(IAetherWriter writer)
        {
#if(WINDOWS)
            writer.WriteInt32("Version", 1);

            base.Save(writer);
            writer.WriteInt32("MaxParticles", maxParticles);
            writer.WriteTimeSpan("Duration", Duration);
            writer.WriteFloat("DurationRandomness",    DurationRandomness);
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
            WriteBlendState(writer, BlendState);
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
                    reader.ReadInt32("MaxParticles", out maxParticles);
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
                    this.BlendState = ReadBlendState(reader);
                    break;
                default:
                    throw new InvalidOperationException("unknown version "+ version);
            }

            return;
        }
        
        #if(WINDOWS)
        private void WriteBlendState(IAetherWriter writer, BlendState blendState)
        {
            writer.WriteInt32("Version", 1);

            writer.WriteString("BlendStateName", blendState.Name);
            writer.WriteInt64("AlphaBlendFunction", (int)blendState.AlphaBlendFunction);
            writer.WriteInt64("AlphaDestinationBlend", (int)blendState.AlphaDestinationBlend);
            writer.WriteInt64("AlphaSourceBlend", (int)blendState.AlphaSourceBlend);
            writer.WriteVector4("BlendFactor", blendState.BlendFactor.ToVector4());
            writer.WriteInt64("ColorBlendFunction", (int)blendState.ColorBlendFunction);
            writer.WriteInt64("ColorDestinationBlend", (int)blendState.ColorDestinationBlend);
            writer.WriteInt64("ColorSourceBlend", (int)blendState.ColorSourceBlend);
            writer.WriteInt64("ColorWriteChannels", (int)blendState.ColorWriteChannels);
            writer.WriteInt64("ColorWriteChannels1", (int)blendState.ColorWriteChannels1);
            writer.WriteInt64("ColorWriteChannels2", (int)blendState.ColorWriteChannels2);
            writer.WriteInt64("ColorWriteChannels3", (int)blendState.ColorWriteChannels3);
            writer.WriteInt64("MultiSampleMask", (int)blendState.MultiSampleMask);
        }
        #endif

        private BlendState ReadBlendState(IAetherReader reader)
        {
            int version;
            reader.ReadInt32("Version", out version);

            switch (version)
            {
                case 1:
                    BlendState blendState = new BlendState();
                    string str; Int64 i64; Vector4 v4;
                    reader.ReadString("BlendStateName", out str); blendState.Name = str;
                    reader.ReadInt64("AlphaBlendFunction", out i64); blendState.AlphaBlendFunction = (BlendFunction)i64;
                    reader.ReadInt64("AlphaDestinationBlend", out i64); blendState.AlphaDestinationBlend = (Blend)i64;
                    reader.ReadInt64("AlphaSourceBlend", out i64); blendState.AlphaSourceBlend = (Blend)i64;
                    reader.ReadVector4("BlendFactor", out v4); blendState.BlendFactor = new Color(v4);
                    reader.ReadInt64("ColorBlendFunction", out i64); blendState.ColorBlendFunction = (BlendFunction)i64;
                    reader.ReadInt64("ColorDestinationBlend", out i64); blendState.ColorDestinationBlend = (Blend)i64;
                    reader.ReadInt64("ColorSourceBlend", out i64); blendState.ColorSourceBlend = (Blend)i64;
                    reader.ReadInt64("ColorWriteChannels", out i64); blendState.ColorWriteChannels = (ColorWriteChannels)i64;
                    reader.ReadInt64("ColorWriteChannels1", out i64); blendState.ColorWriteChannels1 = (ColorWriteChannels)i64;
                    reader.ReadInt64("ColorWriteChannels2", out i64); blendState.ColorWriteChannels2 = (ColorWriteChannels)i64;
                    reader.ReadInt64("ColorWriteChannels3", out i64); blendState.ColorWriteChannels3 = (ColorWriteChannels)i64;
                    reader.ReadInt64("MultiSampleMask", out i64); blendState.MultiSampleMask = (int)i64;
                    return blendState;
                default:
                    throw new InvalidOperationException("unknown version " + version);
            }
        }

        #endregion
    }
}
