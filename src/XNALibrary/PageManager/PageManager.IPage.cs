using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using tainicom.Devices;
using tainicom.PageManager.Enums;

namespace tainicom.PageManager
{
    // pagemanager acts as a dummy IPage root node
    public partial class PageManager:IPage
    {
        public bool LoadContent()
        {
            throw new NotImplementedException();
        }
        public bool SideloadContent()
        {
            throw new NotImplementedException();
        }
        public void UnloadContent()
        {
            throw new NotImplementedException();
        }
        
        public virtual void UpdateLayout() { }

        public void HandleInput(InputState input)
        {
            #if WINDOWS || WP7 || WP8 || W8 || ANDROID
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Game.Exit();
            #endif
        }

        public void DrawRenderTargets(GameTime gameTime)
        {
            // root Page doesn't draw anything
        }

        public event EventHandler BeginRemove;
        public void OnBeginRemove(EventArgs e)
        {
            // root page should never be removed
            throw new NotImplementedException();
        }
        public event EventHandler Removed;
        public void OnRemoved(EventArgs e)
        {
            // root page should never be removed
            throw new NotImplementedException();
        }
        

        public event EventHandler BeginChildAdd;
        public void OnBeginChildAdd(EventArgs e)
        {
            var handler = BeginChildAdd;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }



        public bool IsPopUp { get{return false;} }
        public bool IsTransitionLocked { get { return false; } protected set {} }
        protected EnumTransitionState _transitionState = EnumTransitionState.Active;
        public    EnumTransitionState TransitionState { get { return _transitionState; } }
        public void SetTransitionState(EnumTransitionState transitionState)
        {
            //check for invalid transitionState changes
            if (_transitionState == EnumTransitionState.Loading && transitionState != EnumTransitionState.Loaded) throw new InvalidOperationException();

            _transitionState = transitionState;
            return;
        }

        public void GetTransitionInfo(IPage pageB, out TimeSpan durationA, out EnumTransitionSync syncA)
        {
            durationA = TimeSpan.Zero;
            syncA = EnumTransitionSync.Exclusive | EnumTransitionSync.SyncBegin | EnumTransitionSync.SyncFinish | EnumTransitionSync.SyncMiddle;
        }

        public void SetTransitionTime(TimeSpan duration, TimeSpan time)
        {
        }

        public GestureType EnabledGestures { get { return GestureType.None; } }



        #region events       

        event EventHandler<Events.TransitionStateChangedEventArgs> IPage.TransitionStateChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        #endregion


    }
}
