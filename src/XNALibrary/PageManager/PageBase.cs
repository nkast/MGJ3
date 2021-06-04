using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using tainicom.Devices;
using tainicom.PageManager;
using tainicom.PageManager.Enums;
using tainicom.PageManager.Events;

namespace tainicom.PageManager
{

    public abstract class PageBase : GameComponent, IPage
    {
        protected PageManager pageManager { get; private set; }
        public bool IsPopUp { get; set; }
        public bool IsTransitionLocked { get; protected set; }
        protected EnumTransitionState _transitionState;
        public    EnumTransitionState TransitionState { get { return _transitionState; } }
        public virtual GestureType EnabledGestures { get { return GestureType.Tap; } }

        public PageBase(PageManager pageManager):base(pageManager.Game)
        {
            this.pageManager = pageManager;
            this._transitionState = EnumTransitionState.Loading; // page initially need to be loading assets
            this.IsPopUp = false;
            this.IsTransitionLocked = false;
        }

        ~PageBase()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnloadContent();
            }
            base.Dispose(disposing);
        }
        
        /// <summary>
        /// load content here. 
        /// You can load one asset each time and PageManager will call LoadContent again to support animated loading screens
        /// </summary>
        /// <returns>True when all assets are loaded.</returns>
        public virtual bool LoadContent()
        {
            return true;
        }

        /// <summary>
        /// Similar with LoadContent but used to sideload content while other screen is active.
        /// Break down loading to small steps and leave out any big items to avoid delays. 
        /// LoadContent(..) will be called when Page is activated.
        /// </summary>
        /// <returns>True when all assets are loaded.</returns>
        public virtual bool SideloadContent()
        {
            return true;
        }

        /// <summary>
        /// unload content to free memory.
        /// </summary>
        protected virtual void UnloadContent()
        {
            return;
        }
        

        public virtual void UpdateLayout() { }
        

        public virtual void HandleInput(InputState input)
        {
            
        }

        /// <summary>
        /// overide this method to draw any render targets.
        /// </summary>
        public virtual void DrawRenderTargets(GameTime gameTime)
        {
        }
        
        #region IDrawable Members
        public virtual void Draw(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
        public int DrawOrder
        {
            get { throw new NotImplementedException(); }
        }
        public bool Visible
        {
            get { throw new NotImplementedException(); }
        }
        public event EventHandler<EventArgs> DrawOrderChanged=null;
        public event EventHandler<EventArgs> VisibleChanged = null;
        #endregion

        #region exiting page

        public event EventHandler BeginRemove;
        public virtual void OnBeginRemove(EventArgs e)
        {
            var handler = BeginRemove;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler BeginChildAdd;
        public void OnBeginChildAdd(EventArgs e)
        {
            var handler = BeginChildAdd;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler Removed;
        public virtual void OnRemoved(EventArgs e)
        {
            var handler = Removed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        
        #endregion
        

        public void SetTransitionState(EnumTransitionState transitionState)
        {
            //check for invalid transitionState changes
            if (_transitionState == EnumTransitionState.Loading && transitionState != EnumTransitionState.Loaded) 
                throw new InvalidOperationException();

            TransitionStateChangedEventArgs eventArgs = new TransitionStateChangedEventArgs(transitionState, _transitionState);            
            _transitionState = transitionState;
            OnTransitionStateChanged(eventArgs);

            return;
        }

        public virtual void GetTransitionInfo(IPage pageB, out TimeSpan durationA, out EnumTransitionSync syncA)
        {
            durationA = TimeSpan.Zero;
            syncA = EnumTransitionSync.Exclusive;
            return;
        }

        protected TimeSpan TransitionDuration, TransitionTime;
        protected float TransitionDelta;
        public virtual void SetTransitionTime(TimeSpan duration, TimeSpan time)
        {
            this.TransitionDuration = duration;
            this.TransitionTime = time;
            if (duration.Ticks == time.Ticks) { this.TransitionDelta = 1; return; }
            this.TransitionDelta = time.Ticks / (float)duration.Ticks;
        }


        #region events

        /// <summary>
        /// Event raised after the <see cref="TransitionState" /> property value has changed.
        /// </summary>
        public event EventHandler<TransitionStateChangedEventArgs> TransitionStateChanged;
        /// <summary>
        /// Raises the <see cref="TransitionStateChanged" /> event.
        /// </summary>
        /// <param name="e"><see cref="TransitionStateChangedEventArgs" /> object that provides the arguments for the event.</param>
        protected virtual void OnTransitionStateChanged(TransitionStateChangedEventArgs e)
        {
            var handler = TransitionStateChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        
    }
}
