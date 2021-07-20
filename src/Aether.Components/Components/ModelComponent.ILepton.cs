using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Spatial;

namespace tainicom.Aether.Components
{
    public partial class ModelComponent : ILepton, IWorldTransform, IWorldTransformUpdateable,
        IPlasma<ISpatialNode>,
        ISpatialNode
    {
        #region  Implement ILepton
        LeptonImpl _leptonImpl = new LeptonImpl();

        #if WINDOWS
        [Category("ILepton")]
        #endif
        public Vector3 Position
        {
            get { return _leptonImpl.Position; }
            set
            {
                _leptonImpl.Position = value;
                UpdateChildrenTransform();
            }
        }

        #if WINDOWS
        [Category("ILepton")]
        #endif
        public Quaternion Rotation
        {
            get { return _leptonImpl.Rotation; }
            set
            {
                _leptonImpl.Rotation = value;
                UpdateChildrenTransform();
            }
        }

        #if WINDOWS
        [Category("ILepton")]
        #endif
        public Vector3 Scale
        {
            get { return _leptonImpl.Scale; }
            set
            {
                _leptonImpl.Scale = value;
                UpdateChildrenTransform();
            }
        }

        public Matrix LocalTransform { get { return _leptonImpl.LocalTransform; } }

        #endregion


        #region  Implement IWorldTransform & IWorldTransformUpdateable
        Matrix IWorldTransform.WorldTransform { get { return _leptonImpl.WorldTransform; } }

        void IWorldTransformUpdateable.UpdateWorldTransform(IWorldTransform parentWorldTransform)
        {
            _leptonImpl.UpdateWorldTransform(parentWorldTransform);            
            UpdateChildrenTransform();
        }
        #endregion

        protected virtual void UpdateChildrenTransform()
        {
            foreach (var child in _leptons)
            {
                var updatetable = child as IWorldTransformUpdateable;
                if (updatetable != null)
                    updatetable.UpdateWorldTransform(this);
            }
        }

        #region  Implement IPlasma<ISpatialNode>

        List<ISpatialNode> _leptons = new List<ISpatialNode>();

        IEnumerator<ISpatialNode> IEnumerable<ISpatialNode>.GetEnumerator()
        {
            return ((IEnumerable<ISpatialNode>)_leptons).GetEnumerator();
        }
        
        #endregion

        void UpdateLeptonChilden()
        {
            _leptons.Clear();
            _visualNodes.Clear();

            var rootIndex = _model.Root.Index;
            var bone = new ModelBoneComponent(this, rootIndex);

            _leptons.Add(bone);
            _visualNodes.Add(bone);
        }
    }
}
