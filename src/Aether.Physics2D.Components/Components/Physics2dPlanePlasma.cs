using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Temporal;
using tainicom.Aether.Core;

namespace tainicom.Aether.Physics2D.Components
{
    public class Physics2dPlanePlasma : BasePlasma<IPhysics2dNode>, IPhysics2dNode, ITickable
    {
        Physics2dEnabledList<IAether> _enabledParticles;

        public Physics2dPlanePlasma()
        {
            _enabledParticles = new Physics2dEnabledList<IAether>();
        }

        public void Tick(GameTime gameTime)
        {
            _enabledParticles.Process();
            foreach (IChronon item in _enabledParticles)
            {
                item.Tick(gameTime);
            }
            return;
        }

        protected override void InsertItem(int index, IPhysics2dNode item)
        {
            base.InsertItem(index, item);
            _enabledParticles.Add(item);
            return;
        }

        protected override void RemoveItem(int index)
        {
            IPhysics2dNode item = this[index];
            if (_enabledParticles.Contains(item)) _enabledParticles.Remove(item);
            base.RemoveItem(index);
        }

        public void Enable(IChronon item)
        {
            _enabledParticles.Enable(item);
        }

        public void Disable(IChronon item)
        {
            _enabledParticles.Disable(item);
        }


        class Physics2dEnabledList<T> : List<T>, IEnumerable<T>
        {
            List<T> enableList = new List<T>();
            List<T> disableList = new List<T>();

            public void Enable(T item)
            {
                if (enableList.Contains(item)) return;
                if (this.Contains(item))
                {
                    if (disableList.Contains(item))
                        disableList.Remove(item);
                    //else
                    return;
                }
                enableList.Add(item);
            }

            public void Disable(T item)
            {
                if (disableList.Contains(item)) return;
                if (!this.Contains(item))
                {
                    if (enableList.Contains(item))
                        enableList.Remove(item);
                    else return;
                }
                disableList.Add(item);
            }

            public void Process()
            {
                if (disableList.Count > 0) RemoveDisabled();
                if (enableList.Count > 0) AddEnabled();
            }

            public bool Contains(T item, bool includePending)
            {
                if (!includePending) return this.Contains(item);

                if (enableList.Contains(item)) return true;
                if (this.Contains(item) && !disableList.Contains(item)) return true;
                return false;
            }

            private void AddEnabled()
            {
                foreach (T item in enableList) this.Add(item);
                enableList.Clear();
            }
            private void RemoveDisabled()
            {
                foreach (T item in disableList) this.Remove(item);
                disableList.Clear();
            }
        }
    }
}
