using System;
using System.Collections.Generic;
using System.Text;

namespace tainicom.PageManager.Enums
{   
    [Flags]
    public enum EnumTransitionSync
    {
        None=0,
        /// <summary>
        /// transition in begins after transition out finished.
        /// </summary>
        Exclusive=1, 
        /// <summary>
        /// both pages start transition at the same time. Might not finish at the same time.
        /// </summary>
        SyncBegin=2, 
        /// <summary>
        /// one transition might delay so that both pages finish at the same time.
        /// </summary>
        SyncFinish=4, 
        /// <summary>
        /// one transition might delay so that both transitions sync halfway (0.5) in their trasitions. Might not finish at the same time.
        /// </summary>
        SyncMiddle=8        
    }
}
