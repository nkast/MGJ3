using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.PageManager
{
    public partial class PageManager : IDrawable
    {
        /// <summary>
        /// Draw on RenderTargets, Compute shaders, etc.
        /// </summary>
        public virtual void PreDraw(GameTime gameTime)
        {
            this.GameTime = gameTime;
            
            // Find first visible page
            int firstPage = Math.Max(0, activePages.Count - 1);
            if (firstPage > 0 && (activePages[firstPage].IsPopUp || isTransitioning))
                firstPage--;
            while (firstPage > 0 && (activePages[firstPage].IsPopUp))
                firstPage--;

            //draw rendertargets for visible pages
            for (int i = firstPage; i < activePages.Count; i++)
            {
                IPage page = activePages[i];
                page.DrawRenderTargets(gameTime);
            }
        }

        #region IDrawable implementation
        public virtual void Draw(GameTime gameTime)
        {
            this.GameTime = gameTime;
            
            // Find first visible page
            int firstPage = Math.Max(0, activePages.Count - 1);
            if (firstPage > 0 && (activePages[firstPage].IsPopUp || isTransitioning))
                firstPage--;
            while (firstPage > 0 && (activePages[firstPage].IsPopUp))
                firstPage--;
            
            //draw visible pages from back to front
            for (int i = firstPage; i < activePages.Count; i++)
            {
                IPage page = activePages[i];
                if (page.TransitionState == Enums.EnumTransitionState.Loading) continue;
                page.Draw(gameTime);
            }
            
            return;
        }
        public int DrawOrder
        {
            get { return 0; }
        }
        public bool Visible
        {
            get { return true; }
        }
        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;
        #endregion
    }
}
