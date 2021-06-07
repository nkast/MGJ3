﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Leptons;

namespace tainicom.ProtonType.ContentLib.Components
{
    partial class ModelBoneComponent : IEnumerable,
        ILocalTransform, 
        IPosition,
        IBoundingBox,
        IWorldTransform, IWorldTransformUpdateable,
        IPlasma<ILeptonNode>,
        ILeptonNode
    {
        private ModelComponent _modelComponent;
        private int _boneIndex;
        private int _meshIndex = -1;

        private ModelBone Bone { get { return _modelComponent.Model.Bones[_boneIndex]; } }
        private ModelMesh Mesh { get { return (_meshIndex==-1)? null : _modelComponent.Model.Meshes[_meshIndex]; } }
        
        
        public string Name 
        { 
            get { return Bone.Name; }
            set { }
        }

        public ModelBoneComponent(ModelComponent modelComponent, int boneIndex)
        {
            this._modelComponent = modelComponent;
            this._boneIndex = boneIndex;

            ((IWorldTransformUpdateable)this).UpdateWorldTransform((IWorldTransform)modelComponent);

            for(int m = 0; m<modelComponent.Model.Meshes.Count; m++)
            {
                var mesh = modelComponent.Model.Meshes[m];
                if (mesh.ParentBone.Index == boneIndex)
                {
                    _meshIndex = m;
                    break;
                }
            }

            for (int b = 0; b < Bone.Children.Count; b++)
            {
                var bone = new ModelBoneComponent(modelComponent, Bone.Children[b].Index);
                _leptons.Add(bone);
                _photons.Add(bone);
            }

            if (Mesh != null)
            {
                for(int meshPartIndex = 0; meshPartIndex < Mesh.MeshParts.Count; meshPartIndex++)
                {                    
                    var meshPart = new ModelMeshPartComponent(modelComponent, _meshIndex, meshPartIndex);
                    _leptons.Add(meshPart);
                    _photons.Add(meshPart);
                }
            }
            
            UpdateChildrenTransform();
        }

        public Matrix LocalTransform
        {
            get { return Bone.Transform; }
        }

        public Vector3 Position
        {
            get { return Bone.Transform.Translation; }
            set
            {
                var transform = Bone.Transform;
                transform.Translation = value;
                Bone.Transform = transform;

                UpdateWorldTransform();
                UpdateChildrenTransform();
            }
        }
        
        #region  Implement IWorldTransform & IWorldTransformUpdateable

        Matrix _parentWorldTransform = Matrix.Identity;
        Matrix _worldTransform = Matrix.Identity;
        Matrix IWorldTransform.WorldTransform { get { return _worldTransform; } }

        void IWorldTransformUpdateable.UpdateWorldTransform(IWorldTransform parentWorldTransform)
        {
            _parentWorldTransform = parentWorldTransform.WorldTransform;
            UpdateWorldTransform();
            UpdateChildrenTransform();
        }
        #endregion

        private void UpdateWorldTransform()
        {
            _worldTransform = LocalTransform * _parentWorldTransform;
        }
        
        protected virtual void UpdateChildrenTransform()
        {
            foreach (var child in _leptons)
            {
                var updatetable = child as IWorldTransformUpdateable;
                if (updatetable != null)
                    updatetable.UpdateWorldTransform(this);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }


        #region  Implement IPlasma<ILeptonNode>

        List<ILeptonNode> _leptons = new List<ILeptonNode>();

        IEnumerator<ILeptonNode> IEnumerable<ILeptonNode>.GetEnumerator()
        {
            return ((IEnumerable<ILeptonNode>)_leptons).GetEnumerator();
        }

        #endregion


        public BoundingBox GetBoundingBox()
        {
            if (Mesh != null)
                return BoundingBox.CreateFromSphere(this.Mesh.BoundingSphere);
            else
                return new BoundingBox();
        }
    }
}