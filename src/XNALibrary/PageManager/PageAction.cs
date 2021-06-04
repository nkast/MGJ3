using System;
using System.Collections.Generic;
using System.Text;
using tainicom.PageManager.Enums;

namespace tainicom.PageManager
{
    internal class PageAction
    {
        public readonly IPage page;
        public readonly EnumPageAction action;


        public PageAction(IPage page, EnumPageAction action)
        {
            this.page = page;
            this.action = action;
        }
    }
}
