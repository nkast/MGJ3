using System;
using System.Collections.Generic;
using System.Text;

namespace tainicom.PageManager.Enums
{
    public enum EnumTransitionAction
    {
        /// <summary>
        /// Page B replace Page A
        /// </summary>
        Replace,
        /// <summary>
        /// Page B is loaded as child of Page A
        /// </summary>
        AddChild,
        /// <summary>
        /// Page B is unloaded from parent Page A
        /// </summary>
        Remove
    }
}
