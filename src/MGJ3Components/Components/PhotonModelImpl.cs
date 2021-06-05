using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Core.Materials;
using tainicom.Aether.Core.Photons;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Leptons;
using tainicom.Aether.Elementary.Photons;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.MonoGame;

namespace MGJ3.Components
{
    class PhotonModelImpl : IPhoton, IBoundingBox, IAetherSerialization
    {
        IPhoton parent;
        Model _model;
        ModelMeshPart _meshPart;
        
        public PhotonModelImpl()
        {
            
        }

        internal void Initialize(AetherEngine engine, IPhoton parent, string assetName)
        { 
            this.parent = parent;
            _model = AetherContextMG.GetContent(engine).Load<Model>(assetName);
            _meshPart = _model.Meshes[0].MeshParts[0];
            Texture tx = ((BasicEffect)_meshPart.Effect).Texture;
            ITexture itx = (tx==null)?null:new AetherTexture(tx);
            this.Textures = new ITexture[] { itx };

            bool textureEnabled = (this.Textures[0] != null);
            bool vertexColorEnabled = false;

            Material = CreateDefaultMaterial(engine, textureEnabled, vertexColorEnabled);
        }
        
        #region Implement IPhoton
        public void Accept(IGeometryVisitor geometryVisitor)
        {   
            foreach(var mesh in _model.Meshes)
            {
                ModelMeshPart meshPart = mesh.MeshParts[0];
                geometryVisitor.SetVertices(parent,
                    meshPart.VertexBuffer, meshPart.VertexOffset, 0, meshPart.NumVertices,
                    meshPart.IndexBuffer, meshPart.StartIndex, meshPart.PrimitiveCount);
            }
            return;
        }

        public IMaterial Material { get; set; }
        public ITexture[] Textures { get; private set; }
        #endregion

        private IMaterial CreateDefaultMaterial(AetherEngine engine, bool textureEnabled = true, bool vertexColorEnabled = false)
        {
            string materialName = "DefaultMaterial1";
            if (textureEnabled) materialName += "Tx";
            if (vertexColorEnabled) materialName += "Vx";

            if(!engine.ContainsName(materialName))
            {
                BasicMaterial basicMaterial = new BasicMaterial();
                engine.RegisterParticle(basicMaterial);
                engine.SetParticleName(basicMaterial, materialName);
                engine.MaterialsMgr.Root.Add(basicMaterial);
                basicMaterial.Initialize(engine);
                basicMaterial.RasterizerState = new RasterizerState() { CullMode = CullMode.None };
                basicMaterial.VertexColorEnabled = vertexColorEnabled;
                basicMaterial.LightingEnabled = true;
                basicMaterial.TextureEnabled = textureEnabled;
                basicMaterial.DirectionalLight0.Direction = (Vector3.Down + Vector3.Forward + Vector3.Right) * 0.707f;
                basicMaterial.AmbientLightColor = new Vector3(0.5f);
            }
            IMaterial material = (IMaterial)engine[materialName];
            return material;
        }
        
        public BoundingBox GetBoundingBox()
        {
            BoundingBox boundingBox = new BoundingBox();

            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (ModelMeshPart modelMeshPart in mesh.MeshParts)
                {
                    Vector3[] points = GetVerticesPoints(modelMeshPart);
                    BoundingBox bb = BoundingBox.CreateFromPoints(points);
                    BoundingBox.CreateMerged(ref boundingBox, ref bb, out boundingBox);
                }
            }
            
            return boundingBox;
        }

        public Vector3[] GetVerticesPoints(ModelMeshPart modelMeshPart)
        {            
            Vector3[] points = new Vector3[modelMeshPart.NumVertices];
            int startIndex = 0; // where to place data inside target array 'points'
            int elementCount = modelMeshPart.NumVertices;
            int vertexStride = modelMeshPart.VertexBuffer.VertexDeclaration.VertexStride;
            int offsetInBytes = modelMeshPart.VertexOffset * vertexStride;

            modelMeshPart.VertexBuffer.GetData<Vector3>(
                offsetInBytes,
                points, startIndex, elementCount,
                vertexStride);
            return points;
        }

        #region Implement IAetherSerialization
        public void Save(IAetherWriter writer)
        {
            writer.WriteParticle("Material", Material);
        }
        public void Load(IAetherReader reader)
        {
            IAether ae;
            reader.ReadParticle("Material", out ae); Material = (IMaterial)ae;
        }
        #endregion
        
    }
}
