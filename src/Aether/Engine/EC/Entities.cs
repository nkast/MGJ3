using System;
using System.Collections.Generic;
using tainicom.Aether.Core.ECS;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Serialization;

namespace tainicom.Aether.Engine.EC
{
    public class Entities : IAetherSerialization
    {
        private AetherEngine engine;

        public Entities(AetherEngine engine)
        {
            this.engine = engine;
        }

        public EntityComponents<T> GetEntityComponents<T>(Component component)
            where T : class, IAether
        {
            return new EntityComponents<T>(component);
        }

        void IAetherSerialization.Save(IAetherWriter writer)
        {
            throw new NotImplementedException();
        }

        void IAetherSerialization.Load(IAetherReader reader)
        {
            throw new NotImplementedException();
        }

    }
}
