using System.Collections.Generic;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Visual;

namespace tainicom.Aether.Components
{
    public partial class ModelComponent :
        IVisualPlasma, IPlasma<IVisualNode>,
        IVisualNode
    {
        List<IVisualNode> _visualNodes = new List<IVisualNode>();

        IEnumerator<IVisualNode> IVisualPlasma.VisibleParticles
        {
            get { return _visualNodes.GetEnumerator(); }
        }

        IEnumerator<IVisualNode> IEnumerable<IVisualNode>.GetEnumerator()
        {
            return _visualNodes.GetEnumerator();
        }
    }
}
