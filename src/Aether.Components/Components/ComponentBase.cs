using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;

namespace tainicom.ProtonType.ContentLib.Components
{
    public partial class ComponentBase : IAetherSerialization
    {
        protected string _assetFilename;

        internal void SetAsset(string contentFilename)
        {
            this._assetFilename = contentFilename;
        }

        public virtual void AssetChanged(AetherEngine engine)
        {
        }

        public virtual void Save(IAetherWriter writer)
        {
#if(WINDOWS)
            writer.WriteString("AssetFilename", _assetFilename);
#endif
        }

        public virtual void Load(IAetherReader reader)
        {
            reader.ReadString("AssetFilename", out _assetFilename);
        }

    }
}
