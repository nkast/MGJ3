using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aether.Elementary.Leptons;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using FarseerPhysics.Dynamics;
using Aether.Elementary.Chronons;
using FarseerPhysics.Controllers;
using Aether.Elementary;
using Aether.Engine;
using TableEngine;
using System.Collections.ObjectModel;
using Aether.Elementary.Serialization;
using Aether.Core;

namespace tainicom.Aether.Physics2D.Components
{
    public class VirtualPhysics2dPlane : Physics2dPlane
    {
        public VirtualPhysics2dPlane()
        {
            Index = 0;
            Height = 0;

            scale = Vector3.One;
            rotation = Quaternion.Identity;
            UpdateLocalTransform();
        }
        
    }
}
