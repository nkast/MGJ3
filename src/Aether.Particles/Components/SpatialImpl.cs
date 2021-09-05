using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Spatial;
using tainicom.Aether.Core.Spatial;

namespace tainicom.Aether.Particles
{
    class SpatialImpl : SpatialBase, IWorldTransform, IWorldTransformUpdateable, ILocalTransform, IPosition, ISpatialNode, IAether
    {
        #region Implement IWorldTransform, IWorldTransformUpdateable
        Matrix _parentWorldTransform = Matrix.Identity;
        Matrix _worldTransform = Matrix.Identity;

        public void UpdateWorldTransform(IWorldTransform parentWorldTransform)
        {
            _parentWorldTransform = parentWorldTransform.WorldTransform;
            UpdateWorldTransform();
        }

        public Matrix WorldTransform { get { return _worldTransform; } }
        #endregion

        #region Implement ISpatial Properties

        protected override void UpdateLocalTransform()
        {
            base.UpdateLocalTransform();

            UpdateWorldTransform();
        }

        private void UpdateWorldTransform()
        {
            _worldTransform = _localTransform * _parentWorldTransform;
        }
        #endregion
        
    }
}
