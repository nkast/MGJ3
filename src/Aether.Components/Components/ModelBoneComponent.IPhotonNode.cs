using System.Collections.Generic;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Photons;

namespace tainicom.Aether.Components
{
    partial class ModelBoneComponent :
        IPhotonPlasma, IPlasma<IPhotonNode>,
        IPhotonNode
    {
        List<IPhotonNode> _photons = new List<IPhotonNode>();
        
        IEnumerator<IPhotonNode> IPhotonPlasma.VisibleParticles
        {
            get { return _photons.GetEnumerator(); }
        }
        
        IEnumerator<IPhotonNode> IEnumerable<IPhotonNode>.GetEnumerator()
        {
            return _photons.GetEnumerator();
        }

    }
}
