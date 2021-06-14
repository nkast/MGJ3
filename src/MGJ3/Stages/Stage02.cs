using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MGJ3.Stages
{
    class Stage02 : Stage
    {
        public Stage02(Game game) : base(game, "Stage02.aether")
        {
            while(!LoadStage());
        }
    }
}
