using System.Collections.Generic;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Visual;

namespace tainicom.Aether.Components
{
    partial class ModelBoneComponent :
        IVisualPlasma, IPlasma<IVisualNode>,
        IVisualNode
    {
        List<IVisualNode> _photons = new List<IVisualNode>();
        
        IEnumerator<IVisualNode> IVisualPlasma.VisibleParticles
        {
            get { return _photons.GetEnumerator(); }
        }
        
        IEnumerator<IVisualNode> IEnumerable<IVisualNode>.GetEnumerator()
        {
            return _photons.GetEnumerator();
        }

    }
}
