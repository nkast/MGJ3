using System;
using Microsoft.Xna.Framework;

namespace tainicom.PageManager
{
    public partial class PageManager : IUpdateable
    {     
   

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        public bool Enabled
        {
            get { return true; }
        }
        public int UpdateOrder
        {
            get { return 0; }
        }


    }
}