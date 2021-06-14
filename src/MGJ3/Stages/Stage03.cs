using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MGJ3.Stages
{
    class Stage03 : Stage
    {
        public Stage03(Game game) : base(game, "Stage03.aether")
        {
            while(!LoadStage());
        }
    }
}
