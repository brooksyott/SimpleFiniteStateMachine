using System;
using System.Collections.Generic;
using System.Text;

using Peamel.BasicLogger;

namespace Peamel.SimpleFiniteStateMachine
{
    public class FiniteStateMachine<TStates, TTriggers> : FiniteStateMachineBase<TStates, TTriggers>
        where TTriggers : struct, IComparable, IFormattable, IConvertible
        where TStates : struct, IComparable, IFormattable, IConvertible
    {
        public FiniteStateMachine(TStates startupState)
            : base(startupState)
        {

        }

        public FiniteStateMachine(TStates startupState, ILogger logger)
            : base(startupState, logger)
        {

        }

        override public Boolean Fire(TTriggers trigger, Object obj = null)
        {
            _log.Debug(_tag, String.Format("Trigger Fired: State {0}, Trigger = {1}", _currentState, trigger));
            Boolean didTransition = TransitionStates(trigger, obj);
            if (didTransition)
            {
                _log.Debug(_tag, "Transition Completed");
                return true;
            }

            // If it didn't transition, it might be because it's an internal trigger event
            _log.Debug(_tag, "No transition, trying InternalTransition");
            Boolean internalTransition = InternalTransition(trigger, obj);
            if (internalTransition)
            {
                _log.Debug(_tag, "Internal Transition Completed");
            }

            return internalTransition;
        }

    }

}
