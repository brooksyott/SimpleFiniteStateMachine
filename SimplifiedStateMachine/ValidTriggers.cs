using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peamel.SimpleFiniteStateMachine
{
    public enum Triggers
    {
        UNKNOWN,
        VISUAL_STATE_SMALL,
        VISUAL_STATE_MEDIUM,
        VISUAL_STATE_LARGE,
        PRIMARY_NAV_ENABLED,
        PRIMARY_NAV_DISABLED,
        SECONDARY_NAV_ENABLED,
        SECONDARY_NAV_DISABLED
    };
}
