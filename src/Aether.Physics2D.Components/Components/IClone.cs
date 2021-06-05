using tainicom.Aether.Elementary;

namespace tainicom.Aether.Physics2D.Components
{
    // marks an element that is shared between two levels      
    // Each element has a reference to the other (see 'Clone' property).
    public interface IClone : IAether
    {
        bool IsClone {get;}
        IClone Clone { get; }
        IClone CreateClone();
        Physics2dPlane OtherPlane { get; set; }
    }
}

