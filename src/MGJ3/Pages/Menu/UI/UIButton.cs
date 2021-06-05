using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MGJ3.Pages.Menu.UI
{
    class UIButton
    {
        public Vector2 Position { get; private set; }
        public Texture2D texture { get; private set; }
        public float Scale { get; set; }
        public float Depth { get; set; }
        public Vector2 Origin { get; private set; }

        public UIButton(Vector2 vector2, float depth, Texture2D texture)
        {
            this.Position = vector2;
            this.Depth = depth;
            this.texture = texture;
            this.Scale = 1f;
            this.Origin = new Vector2(texture.Width, texture.Height) / 2f;
        }
    }
}
