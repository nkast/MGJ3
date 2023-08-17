using tainicom.Aether.Engine;
using tainicom.Aether.Physics2D.Dynamics;

namespace tainicom.Aether.Physics2D.Components
{
    public interface IPhysics2dBody : IPhysics2dNode
    {
        Body Body { get; }
        
        float AngularDamping { get; set; }
        float Friction { get; set; }
        float Restitution { get; set; }
        float LinearDamping { get; set; }

        void InitializeBody(Physics2dPlane physics2dPlane, Body body);
    }
}
