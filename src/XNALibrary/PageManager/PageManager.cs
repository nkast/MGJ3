using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using tainicom.Devices;
using tainicom.PageManager.Enums;
//using tainicom.Helpers;

namespace tainicom.PageManager
{
    public partial class PageManager: IGameComponent
    {   
        public Game Game { get; private set; }
        public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        protected bool isInitialized { get; private set; }
        public GameTime GameTime;
        public bool IsInputEnabled { get; set; }

        //controls the transition between two pages
        TransitionController transitionController;
        protected bool isTransitioning;

        Stack<IPage> pageStack;
        Queue<PageAction> pageActionQueue;
        
        #if (WINDOWS || WP8_1 || W8_1 || W10)
        public InputState InputState { get {return  inputState;} } //TODO: expose it as an Event
        #endif

        /// <summary>
        /// Keep a temporary list of active pages that need input/update/draw.
        /// </summary>
        public List<IPage> activePages;
        public IPage ActivePage { get { return (activePages.Count == 0) ? null : activePages[activePages.Count - 1]; } }

        InputState inputState;
        
        public PageManager(Game game)
        {
            this.Game = game;
            this.GraphicsDeviceManager = Game.Services.GetService(typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager;
            isInitialized = false;

            this.inputState = new InputState();
            
            // set common settings
            IsInputEnabled = true;
            EnableSideloading = true;
            isTransitioning = false;
            pageStack = new Stack<IPage>(8);
            pageActionQueue = new Queue<PageAction>(8);
            pageStack.Push(this); //set this PageManager as the first page in the stack
            this.BeginChildAdd += new EventHandler(page_BeginChildAdd);
            activePages = new List<IPage>(5);
            
            return;
        }
        
        #region IGameComponent implementation
        public virtual void Initialize()
        {
            if (isInitialized) return;
            this.GraphicsDevice = Game.GraphicsDevice;
            this.SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            
            //Orientation
            #if (WP7)
            this.Game.Window.OrientationChanged += new EventHandler<EventArgs>(Window_OrientationChanged);
            #elif (WP8)
            this.Game.Window.OrientationChanged += new EventHandler<EventArgs>(Window_OrientationChanged);
            #elif (WP8_1 || W8_1)
            this.Game.Window.OrientationChanged +=Window_OrientationChanged;
            this.Game.Window.ClientSizeChanged += Window_ClientSizeChanged;
            #else
            this.Game.Window.ClientSizeChanged += Window_ClientSizeChanged;
            #endif

            isInitialized = true;
        }
        
        #endregion

        void Window_OrientationChanged(object sender, EventArgs e)
        {
            PagesUpdateLayout();
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            PagesUpdateLayout();
        }

        protected virtual void PagesUpdateLayout()
        {
            for (int i = 0; i < activePages.Count; i++)
            {
                IPage page = activePages[i];
                page.UpdateLayout();
            }
        }


        #region page handling/navigating
        /// <summary>
        /// Add a new page on the top of the active stack
        /// </summary>
        /// <param name="intro"></param>
        public void AddPage(IPage page)
        {
            if (page == null) throw new NullReferenceException();

            //add page to loading queue
            LoadQueue.Enqueue(page);

            // add a path action
            pageActionQueue.Enqueue(new PageAction(page, EnumPageAction.Add));
            
            return;
        }

        /// <summary>
        /// replace the page on top of the stack with this one
        /// </summary>
        /// <param name="page"></param>
        public void ReplacePage(IPage page)
        {
            if(page==null) throw new NullReferenceException();

            //add page to loading queue
            LoadQueue.Enqueue(page);

            // add a path action
            pageActionQueue.Enqueue(new PageAction(page, EnumPageAction.Replace));
        }
        
        /// <summary>
        /// Remove the page on top of the stack
        /// </summary>
        /// <param name="page"></param>
        public void RemovePage(IPage page)
        {
            if (page == null) throw new NullReferenceException();
            System.Diagnostics.Debug.Assert(pageStack.Contains(page),"PageManager.RemovePage(): Page not found in pageStack");

            // add a path action
            pageActionQueue.Enqueue(new PageAction(page, EnumPageAction.Remove));
        }
        
        #endregion 

        
        #region IUpdateable Members
        IPage[] activePagesTmp=new IPage[20];
        public virtual void Update(GameTime gameTime)
        {
            // keep local copy of last gametime
            this.GameTime = gameTime;
            
            // read state from input devices
            inputState.Update(Game.IsActive);

            // side-load content for preloaded pages.
            Sideload();
            
            // load content for any pages waiting to be transitioned in.
            bool loaded=false;
            if(!isTransitioning && LoadQueue.Count > 0)
            {
                IPage page = LoadQueue.Peek();
                loaded = page.LoadContent();
                // when page is loaded, hook some events and check if there is any available transition
                if (loaded)
                {
                    LoadQueue.Dequeue();
                    page.SetTransitionState(EnumTransitionState.Loaded);
                    page.UpdateLayout();
                    page.BeginRemove += new EventHandler(page_BeginRemove);
                    page.BeginChildAdd += new EventHandler(page_BeginChildAdd);
                }
            }

            if(!isTransitioning && pageActionQueue.Count>0)
            {
                NextTransition();
            }

            if (transitionController != null)
            {
                if (!transitionController.Finished)
                {
                    transitionController.Update(gameTime);
                }
                else
                {
                    //transition finished. 
                    if (transitionController.Action == EnumTransitionAction.AddChild)
                    {
                        // we don't need to change the activePages list.
                    }
                    if (transitionController.Action == EnumTransitionAction.Remove)
                    {
                        // remove the replaced page from the list
                        int pageIdx = activePages.LastIndexOf(transitionController.PageA);
                        activePages.RemoveAt(pageIdx);
                        transitionController.PageA.OnRemoved(EventArgs.Empty);
                    }
                    if (transitionController.Action == EnumTransitionAction.Replace)
                    {
                        // remove the replaced page from the list
                        int pageIdx = activePages.LastIndexOf(transitionController.PageA);
                        activePages.RemoveAt(pageIdx);
                        transitionController.PageA.OnRemoved(EventArgs.Empty);
                    }
                    //remove controller
                    transitionController.Close();
                    transitionController = null;
                    isTransitioning = false;
                    NextTransition();
                }
            }

            if (activePages.Count == 0) return;

            activePages.CopyTo(activePagesTmp);
            //IPage[] activePagesTmp = activePages.ToArray();

            //handle input
            IPage topPage = activePagesTmp[activePages.Count - 1];
            if(TouchPanel.EnabledGestures != topPage.EnabledGestures)
                TouchPanel.EnabledGestures = topPage.EnabledGestures;
            if (IsInputEnabled)
                topPage.HandleInput(inputState); //only top page has control
           
            //update child
            for (int i = activePages.Count - 1; i >= 0; i--)
            {
                IPage page = activePagesTmp[i];
                page.Update(gameTime); //update all pages
                if (!page.IsPopUp && 
                    (page.TransitionState == EnumTransitionState.Active ||
                    page.TransitionState == EnumTransitionState.ChildActive)) 
                    break;
            }
            
            return;
        }
        #endregion


        /// <summary>
        /// Check for pending transitions and if there are, switch to transitioning mode
        /// </summary>
        /// <remarks>Called by events of: loading page contents on pending pages,</remarks>
        private void NextTransition()
        {
            //don't do anything if allready in transition mode
            if (isTransitioning) return;

            if (pageActionQueue.Count == 0) return;
            PageAction action = pageActionQueue.Peek();
            IPage pageA = pageStack.Peek();
            IPage pageB = action.page;

            

            //abort transition if pages are not yet loaded
            if (pageA.TransitionState == EnumTransitionState.Loading || pageB.TransitionState == EnumTransitionState.Loading)
                return;

            if (pageA.IsTransitionLocked) return;

            //Abort transition mode if there are more pages waiting to be loaded/transitioning
            //if (LoadQueue.Count > 0) return;

            //initiate transition between pages
            switch (action.action)
            {
                case EnumPageAction.Add:
                    // inform pageA that we need to add a child page.
                    // The base implementation will immediately fire the BeginChildAdd event.
                    // we wait for the BeginChildAdd event to begin transition
                    pageA.OnBeginChildAdd(EventArgs.Empty);
                    break;                    
                case EnumPageAction.Replace:
                    // inform pageA that we need to remove it
                    pageA.OnBeginRemove(EventArgs.Empty);
                    break;
                case EnumPageAction.Remove:
                    // inform pageA that we need to remove it
                    pageA.OnBeginRemove(EventArgs.Empty);
                    break;
  /*                   
                case EnumPageAction.Remove:
                    pageB = pageStack.Pop();
                    pageA = pageStack.Peek();
                    transitionController = new TransitionController(pageA, pageB, EnumTransitionAction.Remove);
                    isTransitioning = true;
                    pageActionQueue.Dequeue();
                    break;
                     */
            }
            
            return;
        }
        
        void page_BeginChildAdd(object sender, EventArgs e)
        {
            IPage page = sender as IPage;
            if (isTransitioning) return;

            PageAction action = pageActionQueue.Peek();
            IPage pageA = pageStack.Peek();
            IPage pageB = action.page;

            transitionController = new TransitionController(pageA, pageB, EnumTransitionAction.AddChild);
            activePages.Add(pageB);
            isTransitioning = true;
            pageActionQueue.Dequeue();
            pageStack.Push(pageB);

            return;
        }

        void page_BeginRemove(object sender, EventArgs e)
        {
            IPage page = sender as IPage;
            if (isTransitioning) return;
            
            if (pageActionQueue.Count == 0) return;
            PageAction action = pageActionQueue.Peek();            

            switch (action.action)
            {
                case EnumPageAction.Replace:
                    {
                        IPage pageA = pageStack.Pop();
                        IPage pageAction = action.page;
                        transitionController = new TransitionController(pageA, pageAction, EnumTransitionAction.Replace);
                        activePages.Add(pageAction);
                        isTransitioning = true;
                        pageActionQueue.Dequeue();
                        pageStack.Push(pageAction);
                    }
                    break;
                case EnumPageAction.Remove:
                    {
                        IPage pageA = pageStack.Pop();
                        IPage pageB = pageStack.Peek();
                        IPage pageAction = action.page;
                        transitionController = new TransitionController(pageAction, pageB, EnumTransitionAction.Remove);
                        isTransitioning = true;
                        pageActionQueue.Dequeue();
                    }
                    break;
            }

            return;
        }

    }

}
