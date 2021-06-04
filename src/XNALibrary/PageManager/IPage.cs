using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using tainicom.Devices;
using tainicom.PageManager.Enums;
using tainicom.PageManager.Events;


namespace tainicom.PageManager
{
    public interface IPage : IDrawable, IUpdateable
    {
        void HandleInput(InputState input);
        GestureType EnabledGestures { get; }

        EnumTransitionState TransitionState { get; }
        /// <summary>
        /// For pages that are not accuping the entire screen.
        /// When true, PageManager will also draw the page below before drawing this one.
        /// </summary>
        bool IsPopUp { get; }


        /// <summary>
        /// Draw any RenderTargets here.
        /// Called before draw(GameTime gameTime)
        /// </summary>
        /// <param name="gameTime"></param>
        void DrawRenderTargets(GameTime gameTime);
        
        #region removing

        /// <summary>
        /// Indicates whether this page can currenty be removed.
        /// </summary>
        /// <remarks>
        /// PageManager query this value before removing a page.
        /// Set this to true while playing an animation that shouldn't be interapted.
        /// This allows you to sequence pages with animations. 
        /// </remarks>
        /// <value>false</value>
        bool IsTransitionLocked { get; }

        /// <summary>
        /// Notify page that it needs to remove itself.
        /// </summary>
        /// <remarks>
        /// Base implementation will just rize BeginRemove and PageManager will acually remove the page.
        /// Override this method to avoid page removal. You must call base.OnBeginRemove() later to remove page from display stack.
        /// </remarks>
        void OnBeginRemove(EventArgs e);

        /// <summary>
        /// Will be rized by the base implementation of OnBeginRemove().
        /// </summary>
        /// <remarks>
        /// It signals the PageManager to schedule the page for transition-out 
        /// and removal from display stack.
        /// </remarks>
        event EventHandler BeginRemove;


        void OnRemoved(EventArgs e);
        event EventHandler Removed;


        #endregion


        #region child loading 

        /// <summary>
        /// Informs page that another page is about to be placed on top of this page.
        /// </summary>
        /// <remarks>
        /// Base implementation will just rize BeginChildAdd and PageManager will acually add the child page.
        /// Override this method to block the loading. Call base.OnBeginChildAdd() later to resume the child loading.
        /// </remarks>
        void OnBeginChildAdd(EventArgs e);

        /// <summary>
        /// Will be rized by the base implementation of OnBeginChildAdd().
        /// </summary>
        /// <remarks>
        /// It signals the PageManager to schedule the page for ChildTransition-in and activation of child page.
        /// </remarks>
        event EventHandler BeginChildAdd;

        #endregion


            

        #region loading Content

        /// <summary>
        /// load content here.
        /// You can load one asset each time and PageManager will call LoadContent again to support animated loading screens.
        /// </summary>
        /// <returns>True when all assets are loaded.</returns>
        bool LoadContent();
        
        /// <summary>
        /// Similar with LoadContent but used to sideload content while other screen is active.
        /// Please, break down loading to small steps and leave out any big items to avoid delays/frame drops.
        /// LoadContent(..) will be called when Page is activated.
        /// </summary>
        /// <returns>True when all assets are loaded.</returns>
        bool SideloadContent();

        /// <summary>
        /// Unload content to free memory.
        /// </summary>
        //void UnloadContent();

        #endregion

        /// <summary>
        /// Called by PageManager after LoadContent() and whenever screen Resolution or Oriantation change.
        /// </summary>
        void UpdateLayout();
        
        void GetTransitionInfo(IPage pageB, out TimeSpan durationA, out EnumTransitionSync syncA);
        void SetTransitionState(EnumTransitionState transitionState);
        void SetTransitionTime(TimeSpan duration, TimeSpan time);

        #region events
        event EventHandler<TransitionStateChangedEventArgs> TransitionStateChanged;
        #endregion

    }
}
