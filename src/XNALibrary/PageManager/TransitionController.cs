using System;
using tainicom.PageManager.Enums;

namespace tainicom.PageManager
{
    public class TransitionController
    {
        public readonly EnumTransitionSync Type;
        public readonly EnumTransitionAction Action;
        
        public readonly IPage PageA; // page out
        public readonly IPage PageB; // page in
        public TimeSpan Time;
        public readonly TimeSpan TotalDuration;
        public readonly TimeSpan DurationA, DurationB;
        public readonly TimeSpan DelayA,DelayB;
        
        /// <summary>
        /// takes values from 0f to 1f during the transition process
        /// </summary>
        public float Delta 
        {
            get { return Math.Min(0, Time.Ticks / TotalDuration.Ticks); }
        }
        public bool Finished
        {
            get { return (Time>=TotalDuration);}
        }
        
        public TransitionController(IPage pageA, IPage pageB, EnumTransitionAction action)
        {
            this.PageA = pageA;
            this.PageB = pageB;
            this.Action = action;

            this.Time = TimeSpan.Zero;

            EnumTransitionSync syncA, syncB;

            pageA.GetTransitionInfo(pageB, out DurationA, out syncA);
            pageB.GetTransitionInfo(pageA, out DurationB, out syncB);

            this.Type = syncA & syncB;
            if (this.Type == EnumTransitionSync.None) throw new InvalidOperationException();
            if ((this.Type & EnumTransitionSync.SyncBegin) != EnumTransitionSync.None) this.Type = EnumTransitionSync.SyncBegin;
            if ((this.Type & EnumTransitionSync.SyncMiddle) != EnumTransitionSync.None) this.Type = EnumTransitionSync.SyncMiddle;
            if ((this.Type & EnumTransitionSync.SyncFinish) != EnumTransitionSync.None) this.Type = EnumTransitionSync.SyncFinish;
            if ((this.Type & EnumTransitionSync.Exclusive) != EnumTransitionSync.None) this.Type = EnumTransitionSync.Exclusive;

            //calculate timing parameters (total duration/delays/etc)
            switch (Type)
            {
                case EnumTransitionSync.Exclusive:
                    TotalDuration = DurationA + DurationB;
                    DelayA = TimeSpan.Zero;
                    DelayB = DurationA;
                    break;
                case EnumTransitionSync.SyncBegin:
                    TotalDuration = new TimeSpan(Math.Max(DurationA.Ticks,DurationB.Ticks));
                    DelayA = DelayB = TimeSpan.Zero;
                    break;
                case EnumTransitionSync.SyncFinish:
                    TotalDuration = new TimeSpan(Math.Max(DurationA.Ticks,DurationB.Ticks));
                    TimeSpan diff = DurationB - DurationA;
                    DelayA = (diff > TimeSpan.Zero) ? diff : TimeSpan.Zero;
                    DelayB = (diff < TimeSpan.Zero) ? -diff : TimeSpan.Zero;
                    break;
                case EnumTransitionSync.SyncMiddle:
                    TotalDuration = new TimeSpan(Math.Max(DurationA.Ticks,DurationB.Ticks));
                    diff = DurationB - DurationA;
                    diff = new TimeSpan(diff.Ticks/2);
                    DelayA = (diff > TimeSpan.Zero) ? diff : TimeSpan.Zero;
                    DelayB = (diff < TimeSpan.Zero) ? -diff : TimeSpan.Zero;
                    break;
                default:
                    //throw new InvalidOperationException("Incompatible transition types");
                    break;
            }

            // set states based on action
            switch (Action)
            {
                case EnumTransitionAction.Replace:
                    //if (pageB.TransitionState != EnumTransitionState.Loaded) throw new InvalidOperationException();
                    //if (pageA.TransitionState != EnumTransitionState.Active) throw new InvalidOperationException();
                    pageB.SetTransitionState(EnumTransitionState.TransitionIn);
                    pageA.SetTransitionState(EnumTransitionState.TransitionOut);
                    break;
                case EnumTransitionAction.AddChild:
                    if (pageB.TransitionState != EnumTransitionState.Loaded) throw new InvalidOperationException();
                    //if (pageA.TransitionState != EnumTransitionState.Active) throw new InvalidOperationException();
                    pageB.SetTransitionState(EnumTransitionState.TransitionIn);
                    pageA.SetTransitionState(EnumTransitionState.ChildTransitionIn);
                    break;
                case EnumTransitionAction.Remove:
                    if (pageB.TransitionState != EnumTransitionState.ChildActive) throw new InvalidOperationException();
                    //if (pageA.TransitionState != EnumTransitionState.Active) throw new InvalidOperationException();
                    pageB.SetTransitionState(EnumTransitionState.ChildTransitionOut);
                    pageA.SetTransitionState(EnumTransitionState.TransitionOut);
                    break;
            }

            return;
        }

        internal void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {   
            if (Finished) throw new Exception("Transition allready finished.");

            this.Time += gameTime.ElapsedGameTime;
            TimeSpan timeA = TimeSpanClamp(Time-DelayA,TimeSpan.Zero,DurationA);
            TimeSpan timeB = TimeSpanClamp(Time-DelayB,TimeSpan.Zero,DurationB);
            
            PageA.SetTransitionTime(DurationA, DurationA - timeA);
            PageB.SetTransitionTime(DurationB, timeB);

            return;
        }

        private TimeSpan TimeSpanClamp(TimeSpan value, TimeSpan min, TimeSpan max)
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }


        internal void Close()
        {
            // set states based on action
            switch (Action)
            {
                case EnumTransitionAction.Replace:
                    PageB.SetTransitionState(EnumTransitionState.Active);
                    PageA.SetTransitionState(EnumTransitionState.Loaded);
                    break;
                case EnumTransitionAction.AddChild:
                    PageB.SetTransitionState(EnumTransitionState.Active);
                    PageA.SetTransitionState(EnumTransitionState.ChildActive);
                    break;
                case EnumTransitionAction.Remove:
                    PageB.SetTransitionState(EnumTransitionState.Active);
                    PageA.SetTransitionState(EnumTransitionState.Loaded);
                    break;
            }

            return;
        }
    }
}
