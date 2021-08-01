using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Spatial;

namespace tainicom.Aether.Components
{
    public partial class ModelComponent : ISpatial, IWorldTransform, IWorldTransformUpdateable,
        IPlasma<ISpatialNode>,
        ISpatialNode
    {
        #region  Implement ISpatial
        SpatialImpl _spatialImpl = new SpatialImpl();

        #if WINDOWS
        [Category("ISpatial")]
        #endif
        public Vector3 Position
        {
            get { return _spatialImpl.Position; }
            set
            {
                _spatialImpl.Position = value;
                UpdateChildrenTransform();
            }
        }

        #if WINDOWS
        [Category("ISpatial")]
        #endif
        public Quaternion Rotation
        {
            get { return _spatialImpl.Rotation; }
            set
            {
                _spatialImpl.Rotation = value;
                UpdateChildrenTransform();
            }
        }

        #if WINDOWS
        [Category("ISpatial")]
        #endif
        public Vector3 Scale
        {
            get { return _spatialImpl.Scale; }
            set
            {
                _spatialImpl.Scale = value;
                UpdateChildrenTransform();
            }
        }

        public Matrix LocalTransform { get { return _spatialImpl.LocalTransform; } }

        #endregion


        #region  Implement IWorldTransform & IWorldTransformUpdateable
        Matrix IWorldTransform.WorldTransform { get { return _spatialImpl.WorldTransform; } }

        void IWorldTransformUpdateable.UpdateWorldTransform(IWorldTransform parentWorldTransform)
        {
            _spatialImpl.UpdateWorldTransform(parentWorldTransform);            
            UpdateChildrenTransform();
        }
        #endregion

        protected virtual void UpdateChildrenTransform()
        {
            foreach (var child in _spatialNodes)
            {
                var updatetable = child as IWorldTransformUpdateable;
                if (updatetable != null)
                    updatetable.UpdateWorldTransform(this);
            }
        }

        #region  Implement IPlasma<ISpatialNode>

        List<ISpatialNode> _spatialNodes = new List<ISpatialNode>();

        IEnumerator<ISpatialNode> IEnumerable<ISpatialNode>.GetEnumerator()
        {
            return ((IEnumerable<ISpatialNode>)_spatialNodes).GetEnumerator();
        }
        
        #endregion

        void UpdateSpatialChilden()
        {
            _spatialNodes.Clear();
            _visualNodes.Clear();

            var rootIndex = _model.Root.Index;
            var bone = new ModelBoneComponent(this, rootIndex);

            _spatialNodes.Add(bone);
            _visualNodes.Add(bone);
        }
    }
}
