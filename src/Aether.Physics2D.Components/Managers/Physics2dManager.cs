using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using tainicom.Aether.Core.Managers;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Data;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;

namespace tainicom.Aether.Physics2D.Components
{
    public class Physics2dManager : BaseManager<IPhysics2dNode>
    {
        public IPlasmaList<IPhysics2dNode> Root { get; protected set; }

        public static float DefaultGravityAcceleration { get { return 9.8f; } }

        public Physics2dPlane DefaultPlane { get; set; }

        #region
        public Physics2dPlane FindPlane(IPhysics2dNode particle)
        {
            for(int i=0;i<Root.Count;i++)
            {
                Physics2dPlane plane = (Physics2dPlane)Root[i];
                if (plane.Contains(particle))
                    return plane;
            }
            return (null);
        }
        #endregion

        public Physics2dManager(): base("FarseerMgr")
        {
        }

        public override void Initialize(AetherEngine engine)
        {
            base.Initialize(engine);
            Root = new Physics2dPlanePlasma();
        }

        public override void Tick(GameTime gameTime)
        {   
            return;
        }

        public void SuppressStep()
        {
            for(int i= 0; i< this.Root.Count; i++)
            {
                var item = this.Root[i];

                var plane = (Physics2dPlane)item;
                plane.SuppressStep();
            }
        }
        
        protected override void OnRegisterParticle(UniqueID uid, IAether particle)
        {
            if (particle is Physics2dPlane)
                Root.Add((Physics2dPlane)particle);
            if (particle is IPhysics2dBody && DefaultPlane != null)
            {
                _engine.AddChild<IPhysics2dNode>(DefaultPlane, (IPhysics2dNode)particle);
            }
        }

        protected override void OnUnregisterParticle(UniqueID uid, IAether particle)
        {
            if (particle is Physics2dPlane)
                Root.Remove((Physics2dPlane)particle);
            if (particle is IPhysics2dBody)
            {
                for (int i = 0; i < Root.Count; i++)
                {
                    Physics2dPlane plane = (Physics2dPlane)Root[i];
                    if (plane.Contains((IPhysics2dNode)particle))
                        _engine.RemoveChild<IPhysics2dNode>(plane, (IPhysics2dNode)particle);
                }
            }
        }

        public override void Save(IAetherWriter writer)
        {
#if (WINDOWS)
            base.Save(writer);

            //write root
            if (Root is IAetherSerialization)
                writer.Write("Root", (IAetherSerialization)Root);
#endif
        }

        public override void Load(IAetherReader reader)
        {
            base.Load(reader);

            //read root
            if (Root is IAetherSerialization)
                reader.Read("Root", (IAetherSerialization)Root);

            // Test if all paricles are registered
            foreach (var pair in this._engine)
            {
                if (pair.Value is IPhysics2dNode)
                {
                    if (this.ContainsKey(pair.Key)) continue;
                    System.Diagnostics.Debug.Assert(this.ContainsKey(pair.Key));
                    //this.Add(pair); // Add missing particles
                }
            }

            return;
        }
        
        #region farseer to XNA and vice-versa conversions
        public static float XNAtoBOX2DRotation(Physics2dPlane physics2dPlane, float rotation) { return rotation; }
        public static float BOX2DtoXNARotation(Physics2dPlane physics2dPlane, float rotation) { return rotation; }
        
        public static float XNAtoBOX2DRotation(Physics2dPlane physics2dPlane, Quaternion value)
        {
            Matrix planeWorld;
            SpatialManager.GetWorldTransform(physics2dPlane, out planeWorld);
            Vector3 planeNormal = planeWorld.Forward;
            
            //Decomposing quaternion to twist/swing
            value.Normalize();
            var v = new Vector3(value.X, value.Y, value.Z);
            float dot;
            Vector3.Dot(ref v, ref planeNormal, out dot);
            Vector3 proj;
            Vector3.Multiply(ref planeNormal, dot, out proj);
            Quaternion qTwist = new Quaternion(proj, value.W);
            qTwist.Normalize();
            //Quaternion qSwing = value * Quaternion.Conjugate(qTwist);
            
            var tAngle = (float)Math.Acos(qTwist.W)*2;
            if (dot > 0) tAngle = - tAngle;
            
            return tAngle;
        }
        
        public static Vector3 Box2DtoXNAWorldPosition(Physics2dPlane physics2dPlane, Vector2 position2, Vector3 worldPosition)
        {
            Matrix fpWorldTransform = SpatialManager.GetWorldTransform(physics2dPlane);
            Matrix fpInvWorldTransform = physics2dPlane.InvWorldTransform;

            Vector3 fpPos = Vector3.Transform(worldPosition, fpInvWorldTransform);
            fpPos.X = position2.X;
            fpPos.Y = position2.Y;
            Vector3 newWorldPosition = Vector3.Transform(fpPos, fpWorldTransform);
            return newWorldPosition;
        }

        public static Vector2 XNAtoBox2DWorldPosition(Physics2dPlane physics2dPlane, Vector3 worldPosition)
        {
            Matrix fpInvWorldTransform = physics2dPlane.InvWorldTransform;

            Vector3 fplpos = Vector3.Transform(worldPosition, fpInvWorldTransform);
            Vector2 newfpPosition = new Vector2(fplpos.X, fplpos.Y);
            return newfpPosition;
        }
        
        #endregion

    }
}
