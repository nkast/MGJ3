using System;

namespace tainicom.Aether.Physics2D.Components
{
    public interface IMaximumStep : IPhysics2dNode
    {
        float MaximumStep {get;}

        // replacing float with TimeSpan cause flippers to behave erratic. 
        //TimeSpan MaximumStep {get;}
    }
}
