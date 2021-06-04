using System;
using System.Collections.Generic;
using System.Text;
using tainicom.PageManager.Enums;

namespace tainicom.PageManager.Events
{
    /// <summary>
    /// Provides arguments for an event.
    /// </summary>
    
    public class TransitionStateChangedEventArgs : EventArgs
    {
        
        #region Public Properties
        public readonly EnumTransitionState NewTransitionState;
        public readonly EnumTransitionState PreviewsTransitionState;
        
        #endregion

        #region Private / Protected
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new instance of the <see cref="TransitionStateChangedEventArgs" /> class.
        /// </summary>

        public TransitionStateChangedEventArgs(EnumTransitionState newTransitionState, EnumTransitionState previewsTransitionState)
        {
            this.NewTransitionState = newTransitionState;
            this.PreviewsTransitionState = previewsTransitionState;
        }
        #endregion
    }
}
