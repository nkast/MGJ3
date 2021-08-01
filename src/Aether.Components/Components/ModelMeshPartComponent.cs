using System;
using System.Collections.Generic;
using System.Text;
using tainicom.Aether.Elementary.Visual;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Elementary.Spatial;
using Microsoft.Xna.Framework;
using tainicom.Aether.Core.Materials;
using tainicom.Aether.Core.Visual;

namespace tainicom.Aether.Components
{
    public class ModelMeshPartComponent : IVisual, ILocalTransform, IWorldTransform, IWorldTransformUpdateable,
        ISpatialNode
    {
        private ModelComponent _modelComponent;
        private int _meshIndex;
        private int _meshPartIndex;

        ModelMeshPart MeshPart { get { return _modelComponent.Model.Meshes[_meshIndex].MeshParts[_meshPartIndex]; } }
        
        public IMaterial Material { get; set; } 
        public ITexture[] Textures { get; private set; }

        
        public ModelMeshPartComponent(ModelComponent modelComponent, int meshIndex, int meshPartIndex)
        {
            this._modelComponent = modelComponent;
            this._meshIndex = meshIndex;
            this._meshPartIndex = meshPartIndex;

            this.Textures = new ITexture[4];

            var effect = MeshPart.Effect;
            this.Material = CreateMaterial(effect);

            int txCount = 0;
            foreach (var param in effect.Parameters)
            {
                if (param.ParameterType == EffectParameterType.Texture2D)
                {
                    var texture = param.GetValueTexture2D();
                    if (texture == null)
                        Textures[txCount] = null;
                    else
                        Textures[txCount] = new AetherTexture(texture);

                    txCount++;
                    if (txCount >= Textures.Length) break;
                }
            }
        }

        private IMaterial CreateMaterial(Effect effect)
        {
            var engine = _modelComponent.Engine;

            string materialName = "ModelMaterial" + effect.GetHashCode();

            //if (engine.ContainsName(materialName))
            //    return (IMaterial)engine[materialName];
            
            ModelMaterial newMaterial = new ModelMaterial(effect);
            //engine.RegisterParticle(newMaterial);
            //engine.SetParticleName(newMaterial, materialName);
            //engine.MaterialsMgr.Root.Add(newMaterial);
            newMaterial.Initialize(engine);
            return newMaterial;
        }


        public void Accept(IGeometryVisitor geometryVisitor)
        {
            geometryVisitor.SetVertices(this,
                MeshPart.VertexBuffer, MeshPart.VertexOffset, 0, MeshPart.NumVertices,
                MeshPart.IndexBuffer, MeshPart.StartIndex, MeshPart.PrimitiveCount);
            return;
        }


        #region  Implement IWorldTransform
        Matrix _parentWorldTransform = Matrix.Identity;
        Matrix _worldTransform = Matrix.Identity;

        public void UpdateWorldTransform(IWorldTransform parentWorldTransform)
        { 
            _parentWorldTransform = parentWorldTransform.WorldTransform;
            _worldTransform = _parentWorldTransform;
        }

        public Matrix WorldTransform { get { return this._worldTransform; } }
        #endregion


        public Matrix LocalTransform { get { return Matrix.Identity; } }
                        
        protected void UpdateWorld()
        {
            _worldTransform = _parentWorldTransform;
        }
    }
}
