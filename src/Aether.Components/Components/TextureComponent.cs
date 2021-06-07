using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Photons;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.MonoGame;

namespace tainicom.ProtonType.ContentLib.Components
{
    [AssetContentTypeAttribute("Microsoft.Xna.Framework.Content.Pipeline.Graphics.TextureContent")]
    public class TextureComponent: ComponentBase, ITexture, IAether, IInitializable
    {
        private Texture _texture;

        public Texture Texture  { get { return _texture; } }


        public TextureComponent()
        {
        }
        
        public void Initialize(Aether.Engine.AetherEngine engine)
        {
            var content = AetherContextMG.GetContent(engine);
#if EDITOR
            try { _texture = content.Load<Texture>(_assetFilename); }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException cle) { /* TODO: log warning */ }
#else
            _texture = content.Load<Texture>(_assetFilename);
#endif
        }


        public override void AssetChanged(AetherEngine engine)
        {
            var content = AetherContextMG.GetContent(engine);
            try { this._texture = content.Load<Texture>(_assetFilename); }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException cle) { /* TODO: log warning */ }
        }

        public override void Save(IAetherWriter writer)
        {
#if(WINDOWS)
            base.Save(writer);
#endif
        }

        public override void Load(IAetherReader reader)
        {
            base.Load(reader);
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
            return "TextureComponent:" + _assetFilename;
        }

    }
}
