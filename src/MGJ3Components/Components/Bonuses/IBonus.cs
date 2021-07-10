using System;
using System.Collections.Generic;

namespace MGJ3.Components
{
    public interface IBonus
    {
        int Score { get; }
        int Power { get; }
        int Lives { get; }
    }
}
