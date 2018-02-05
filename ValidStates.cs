using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peamel.SimpleFiniteStateMachine
{
    public enum States
    {
        UNKNOWN,
        NO_NAV,
        PRIMARTY_NAV,
        SECONDARY_NAV,
        SMALL_NO_NAV,
        SMALL_PRIMARY_NAV,
        SMALL_SECONDARY_NAV
    };
}
