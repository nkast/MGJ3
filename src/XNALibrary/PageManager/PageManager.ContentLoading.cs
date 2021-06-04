using System;
using System.Collections.Generic;
using System.Text;

// content loading handles the smooth loading of content for each page.

namespace tainicom.PageManager
{
    public partial class PageManager
    {           
        Queue<IPage> LoadQueue = new Queue<IPage>(10);

        /// <summary>
        /// Enable/Disable sideloading.
        /// You might want to disable it during gameplay and enable again during pause,menus,etc.
        /// Sideloading is not active during transitions.
        /// </summary>
        public bool EnableSideloading { get; set; }
        // list of pages that are to be sidelaoded 
        private Queue<IPage> SideloadQueue = new Queue<IPage>(10);

        /// <summary>
        ///  add pages for sideloading.
        /// </summary>
        /// <param name="page"></param>
        public void SideloadPage(IPage page)
        {
            SideloadQueue.Enqueue(page);
        }

        /// <summary>
        /// Load content, step by step.
        /// </summary>
        private void Sideload()
        {
            if (isTransitioning) return;
            if(!EnableSideloading) return;
            //if (GameTime.IsRunningSlowly) return;

            sw.Reset();
            sw.Start();
            while (SideloadQueue.Count > 0)
            {
                IPage page = SideloadQueue.Peek();
                if (page.TransitionState != Enums.EnumTransitionState.Loading || page.SideloadContent())
                {
                    SideloadQueue.Dequeue();
                }
                if (sw.Elapsed >= sideloadHeadroom) break;
            }
            sw.Stop();
            
            return;
        }

        readonly System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        readonly TimeSpan sideloadHeadroom = TimeSpan.FromMilliseconds((1000f / 60f) * 0.5f);
    }
}
