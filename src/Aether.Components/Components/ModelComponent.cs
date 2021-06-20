using System;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.MonoGame;

namespace tainicom.Aether.Components
{
    [AssetContentTypeAttribute("Microsoft.Xna.Framework.Content.Pipeline.Processors.ModelContent")]
    public partial class ModelComponent : ComponentBase, IAether, IInitializable, IEnumerable
    {
        private Model _model;
        internal Model Model { get { return _model; } }
        public AetherEngine Engine { get; private set; }


        public ModelComponent()
        {
        }
        
        public void Initialize(Aether.Engine.AetherEngine engine)
        {
            this.Engine = engine;

            var content = AetherContextMG.GetContent(engine);
#if EDITOR
            try { _model = content.Load<Model>(_assetFilename); }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException cle) { /* TODO: log warning */ }
#else
            _model = content.Load<Model>(_assetFilename);
#endif
            UpdateLeptonChilden();
        }


        public override void AssetChanged(AetherEngine engine)
        {
            var content = AetherContextMG.GetContent(engine);
            try { _model = content.Load<Model>(_assetFilename); }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException cle) { /* TODO: log warning */ }

            UpdateLeptonChilden();
        }

        public override void Save(IAetherWriter writer)
        {
#if(WINDOWS)
            writer.WriteInt32("Version", 1);

            base.Save(writer);
            this._leptonImpl.Save(writer);
#endif
        }

        public override void Load(IAetherReader reader)
        {
            int version;
            reader.ReadInt32("Version", out version);

            base.Load(reader);
            this._leptonImpl.Load(reader);
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                //Dispose unmanaged resources here
            }
            //Dispose managed resources here

            base.Dispose(disposing);
        }

        public override string ToString()
        {
            return "ModelComponent:" + _assetFilename;
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

    }
}
