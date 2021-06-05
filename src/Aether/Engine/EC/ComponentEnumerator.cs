using System;
using System.Collections.Generic;


namespace tainicom.Aether.Core.ECS
{
    public struct EntityComponents<T>
        where T : class
    {
        private readonly Component component;

        internal EntityComponents(Component component)
        {
            this.component = component;
        }

        public ComponentEnumerator<T> GetEnumerator()
        {
            return new ComponentEnumerator<T>(this.component);
        }
    }

    public struct ComponentEnumerator<T>
        where T : class
    {
        private readonly Component headComponent;
        private Component currentComponent;

        internal ComponentEnumerator(Component component)
        {
            this.headComponent = component;
            this.currentComponent = null;
        }

        public T Current
        {
            get
            {
                if (currentComponent is ComponentProxy)
                    return ((ComponentProxy)currentComponent).Value as T;
                else
                    return currentComponent as T;
            }
        }

        public bool MoveNext()
        {
            while (true)
            {
                if (currentComponent == null)
                {
                    currentComponent = headComponent;
                }
                else
                {
                    currentComponent = currentComponent._nextComponent;
                    if (currentComponent == headComponent)
                    {
                        return false;
                    }
                }

                if (currentComponent is T)
                    return true;
            }

            return false;
        }

        public void Reset()
        {
            this.currentComponent = null;
        }
    }
}
