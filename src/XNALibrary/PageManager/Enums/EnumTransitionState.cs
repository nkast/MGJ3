using System;
using System.Collections.Generic;
using System.Text;

namespace tainicom.PageManager.Enums
{    
    public enum EnumTransitionState
    {
        Loading,
        Loaded,
        TransitionIn,
        Active,
        ChildTransitionIn,
        ChildActive,
        ChildTransitionOut,
        TransitionOut
    }
}
