using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MGJ3.Stages
{
    class Stage01 : Stage
    {
        public Stage01(Game game) : base(game, "Stage01.aether")
        {
            while(!LoadStage());
        }
    }
}
