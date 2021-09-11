using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Controllers;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Core;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Spatial;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.Elementary.Temporal;
using tainicom.Aether.Core.Spatial;

namespace tainicom.Aether.Physics2D.Components
{
    public class Physics2dPlane : BasePlasma<IPhysics2dNode>, IPhysics2dNode, ITemporal, IInitializable, IAetherSerialization,
        ISpatial, ILocalTransform, IPosition,
        IWorldTransform, IWorldTransformUpdateable,
        IAether
    {
        #region Implement ISpatial
        SpatialBase _spatialImpl = new SpatialBase();

        public Vector3 Scale
        {
            get { return _spatialImpl.Scale; }
            set { _spatialImpl.Scale = value; UpdateTransform(); }
        }

        public Quaternion Rotation
        {
            get { return _spatialImpl.Rotation; }
            set { _spatialImpl.Rotation = value; UpdateTransform(); }
        }

        public Vector3 Position
        {
            get { return _spatialImpl.Position; }
            set { _spatialImpl.Position = value; UpdateTransform(); }
        }
        
        public Matrix LocalTransform { get { return _spatialImpl.LocalTransform; } }
        
        protected void UpdateTransform()
        {
            InvWorldTransform = Matrix.Invert(this.WorldTransform);
        }

        #endregion

        
        #region Implement IWorldTransform
        public Matrix WorldTransform { get { return _spatialImpl.WorldTransform; } }
        public Matrix InvWorldTransform { get; private set; } // cache Inverse World Transform 
        
        public void UpdateWorldTransform(IWorldTransform parentWorldTransform)
        {
            _spatialImpl.UpdateWorldTransform(parentWorldTransform);
            InvWorldTransform = Matrix.Invert(this.WorldTransform);
        }

        #endregion

        public World World { get; private set; }
        public VelocityLimitController VelocityLimitController { get; private set; }



        float _angularVelocityLimit = 0.0f;
        float _linearVelocityLimit = 0.00f; // 0 = disabled

        float _gravityAcceleration = 9.8f;
        float _tableInclineAngle = 5.1f;

        bool _suppressStep = false;


        public float MaxAngularVelocity
        {
            get { return _angularVelocityLimit; }
            set 
            {
                _angularVelocityLimit = value;
                VelocityLimitController.MaxAngularVelocity = _angularVelocityLimit;
                VelocityLimitController.LimitAngularVelocity = (_angularVelocityLimit != 0);
            }
        }
        public float MaxLinearVelocity
        {
            get { return _linearVelocityLimit; }
            set 
            {
                _linearVelocityLimit = value;
                VelocityLimitController.MaxLinearVelocity = _linearVelocityLimit;
                VelocityLimitController.LimitLinearVelocity = (_linearVelocityLimit != 0);
            }
        }

        public float GravityAcceleration
        {
            get { return _gravityAcceleration; }
            set 
            {
                _gravityAcceleration = value;
                World.Gravity = GetGravity();
            }
        }
        public float TableInclineAngle
        {
            get { return _tableInclineAngle; }
            set 
            { 
                _tableInclineAngle = value;
                World.Gravity = GetGravity();
            }
        }
        
        protected List<IMaximumStep> maxStepParticles;

        private AetherEngine _engine;

        public Physics2dPlane()
        {
            UpdateTransform();

            //create Physics2d World
            World = new World(Vector2.Zero);
            World.Tag = this; // Allow converting World -> Physics2dPlane.

            maxStepParticles = new List<IMaximumStep>();

            VelocityLimitController = new VelocityLimitController();
        }


        //helper functions
        public Vector2 GetGravity()
        {
            float incline = MathHelper.ToRadians(TableInclineAngle);
            float inclineProjection = (float)Math.Sin(incline);
            float gravity = GravityAcceleration * inclineProjection;
            return Physics2dManager.XNAtoBox2DWorldPosition(this, new Vector3(0, 0, gravity));
        }

        
        #region Implement Aether.Elementary.IInitializable
        public void Initialize(AetherEngine engine)
        {
            _engine = engine;
            
            //setup farseer
            Vector2 gravity = GetGravity();
            gravity = new Vector2(0, -GravityAcceleration);
            World.Gravity = gravity;

            MaxAngularVelocity = _angularVelocityLimit;
            MaxLinearVelocity = _linearVelocityLimit;
            World.Add(VelocityLimitController);
        }
        #endregion
        
        protected override void InsertItem(int index, IPhysics2dNode item)
        {
            base.InsertItem(index, item);

            var body = World.CreateBody();
            IPhysics2dBody physics2dBody = item as IPhysics2dBody;
            body.Tag = physics2dBody;
            physics2dBody.InitializeBody(this, body);
            if (body.IsBullet)
                VelocityLimitController.AddBody(body);
            
            if (item is IMaximumStep)
                maxStepParticles.Add((IMaximumStep)item);
        }

        protected override void RemoveItem(int index)
        {
            IAether item = this[index];
            base.RemoveItem(index);
            IPhysics2dBody physics2dBody = item as IPhysics2dBody;
            if (physics2dBody.Body.IsBullet)
                VelocityLimitController.RemoveBody(physics2dBody.Body);
            if (World.IsLocked)
            {
                World.RemoveAsync(physics2dBody.Body);
                System.Diagnostics.Debug.WriteLine("Warning: call to deprecated World.RemoveAsync(Body)");
            }
            else
                World.Remove(physics2dBody.Body);

            if (item is IMaximumStep)
                maxStepParticles.Remove((IMaximumStep)item);
        }

        void ITickable.Tick(GameTime gameTime)
        {
            Step(gameTime.ElapsedGameTime);
        }

        internal void SuppressStep()
        {
            _suppressStep = true;
        }

        internal void Step(TimeSpan elapsedTime)
        {
            if(_suppressStep)
            {
                _suppressStep = false;
                return;
            }

            if (maxStepParticles.Count > 0)
            {
                float elapsedSeconds = (float)elapsedTime.TotalSeconds;
                while (elapsedSeconds >= 0.001f)
                {
                    // calculate max step interval.
                    float moveSeconds = elapsedSeconds;
                    foreach (IMaximumStep particle in maxStepParticles)
                    {
                        float maxMoveSeconds = particle.MaximumStep;
                        if (maxMoveSeconds > 0)
                            moveSeconds = Math.Min(moveSeconds, maxMoveSeconds);
                    }

                    World.Step(moveSeconds);
                    elapsedSeconds -= moveSeconds;
                }

                /* replacing float with TimeSpan cause flippers to behave erratic.
                TimeSpan lim = TimeSpan.FromSeconds(0.001f);
                while (elapsedTime >= lim)
                {
                    // calculate max step interval.
                    TimeSpan moveTime = elapsedTime;
                    foreach (IMaximumStep particle in maxStepParticles)
                    {
                        TimeSpan maxMoveTime = particle.MaximumStep;
                        if (maxMoveTime > TimeSpan.Zero)
                            moveTime = (moveTime < maxMoveTime ? moveTime : maxMoveTime);
                    }

                    World.Step((float)moveTime.TotalSeconds);
                    elapsedTime -= moveTime;
                }
                */
            }
            else
            {
                World.Step((float)elapsedTime.TotalSeconds);
            }
            return;
        }
        
        public static Physics2dPlane TryGetParentPlane(Body body)
        {
            if (body == null) return null;
            return Physics2dPlane.FromWorld(body.World);
        }

        public static Physics2dPlane FromWorld(World world)
        {
            if (world == null) return null;            
            return world.Tag as Physics2dPlane;
        }


        public override void Save(IAetherWriter writer)
        {
#if(WINDOWS)
            writer.WriteInt32("Version", 1);

            writer.WriteFloat("GravityAcceleration", _gravityAcceleration);
            writer.WriteFloat("TableInclineAngle", _tableInclineAngle);
            writer.WriteFloat("AngularVelocityLimit", _angularVelocityLimit);
            writer.WriteFloat("LinearVelocityLimit", _linearVelocityLimit);

            base.Save(writer);

            _spatialImpl.Save(writer);
#endif
        }

        public override void Load(IAetherReader reader)
        {
            int version;
            reader.ReadInt32("Version", out version);

            switch (version)
            {
                case 1:
                    reader.ReadFloat("GravityAcceleration", out _gravityAcceleration);
                    reader.ReadFloat("TableInclineAngle", out _tableInclineAngle);
                    reader.ReadFloat("AngularVelocityLimit", out _angularVelocityLimit);
                    reader.ReadFloat("LinearVelocityLimit", out _linearVelocityLimit);

                    base.Load(reader);

                    _spatialImpl.Load(reader);
                    UpdateTransform();
                  break;
                default:
                  throw new InvalidOperationException("unknown version " + version);
            }
        }

    }
}
