using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peamel.SimpleFiniteStateMachine
{
    internal class StateTransitionException : Exception
    {
        internal StateTransitionException()
        {
        }

        internal StateTransitionException(string message)
            : base(message)
        {
        }

        internal StateTransitionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
