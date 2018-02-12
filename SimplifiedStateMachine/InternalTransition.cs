using System;
using System.Collections.Generic;
using System.Text;

namespace Peamel.SimpleFiniteStateMachine
{
    internal class InternalTransition<TTriggers>
         where TTriggers : struct, IComparable, IFormattable, IConvertible
    {
        public TTriggers Trigger { get; set; }
        public Action<Object, TTriggers> Exec;
        public Func<Boolean> Guard;
    }
}
