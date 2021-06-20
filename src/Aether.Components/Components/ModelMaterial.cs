#region License
//   Copyright 2015 Kastellanos Nikolaos
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
#endregion

#if (WINDOWS)
using tainicom.Aether.Design.Converters;
#endif
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using tainicom.Aether.Core.Materials.Data;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Core.Materials;

namespace tainicom.Aether.Components.Components
{
    public class ModelMaterial : MaterialBase
    {
        //TODO: we must hide Effect class completly.
        public Effect Effect { get { return (Effect)_effect; } }

        public ModelMaterial():base()
        {

        }

        public ModelMaterial(Effect effect):base()
        {
            this._effect = effect;
        }
      
        protected override void CreateEffect()
        {
            //this._effect = new BasicEffect(this.GraphicsDevice);
        }

        public override void Apply()
        {            
            base.Apply();
        }
        
        #region Aether.Elementary.Serialization.IAetherSerialization Members

        public override void Save(IAetherWriter writer)
        {
#if(WINDOWS)
            base.Save(writer);
#endif
        }

        public override void Load(IAetherReader reader)
        {
            base.Load(reader);
            Vector3 vctr3; float flt; bool bl;
        }

        #endregion

    }
}
