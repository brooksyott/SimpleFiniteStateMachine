using System;
using System.Collections.Generic;
using System.Text;

namespace Peamel.SimpleFiniteStateMachine
{
    internal class Transition<TStates, TTriggers>
        where TTriggers : struct, IComparable, IFormattable, IConvertible
        where TStates : struct, IComparable, IFormattable, IConvertible
    {
        public TTriggers Trigger { get; set; }
        public TStates State { get; set; }
        public Func<Boolean> Guard;
    }
}
