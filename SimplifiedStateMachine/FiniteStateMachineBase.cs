using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Peamel.BasicLogger;

namespace Peamel.SimpleFiniteStateMachine
{
    abstract public class FiniteStateMachineBase<TStates, TTriggers>
        where TTriggers : struct, IComparable, IFormattable, IConvertible
        where TStates : struct, IComparable, IFormattable, IConvertible
    {
        protected ILogger _log;
        protected BasicLoggerTag LoggerName = new BasicLoggerTag("FSM");

        protected Dictionary<TStates, State<TStates, TTriggers>> _states = new Dictionary<TStates, State<TStates, TTriggers>>();
        protected TStates _currentState;

        public TStates CurrentState
        {
            get { return _currentState; }
        }

        public FiniteStateMachineBase(TStates startupState) 
        {
            _log = BasicLoggerFactory.GetLogger();
            _currentState = startupState;
        }

        public FiniteStateMachineBase(TStates startupState, ILogger logger)
        {
            _currentState = startupState;
            _log = logger;
        }

        public void UpdateLogger(ILogger logger)
        {
            _log = logger;
        }

        /// <summary>
        /// Adds a state to the statemachine
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="state"></param>
        public State<TStates, TTriggers> Configure(TStates state)
        {
            _log.Debug(LoggerName, "FSM Configured: " + state);

            State<TStates, TTriggers> newState = State<TStates, TTriggers>.Configure(state);
            _states[state] = newState;
            return newState;
        }


        /// <summary>
        /// Fires the state, and sets a new state
        /// </summary>
        /// <param name="trigger"></param>
        virtual public Boolean Fire(TTriggers trigger, Object obj = null)
        {
            return false;
        }

        protected Boolean InternalTransition(TTriggers trigger, Object obj)
        {
            if (!_states.ContainsKey(_currentState))
            {
                _log.Error(LoggerName, String.Format("Invalid current state {0}", _currentState));

                return false; // No transition found
            }

            Boolean didInternalTransition = _states[_currentState].InternalTransition(trigger, obj);

            _log.Debug(LoggerName, String.Format("InternalTransition: State {0}, Trigger = {1}, Returned = {2}",
                _currentState, trigger, didInternalTransition));

            return didInternalTransition;
        }

        protected Boolean TransitionStates(TTriggers trigger, Object obj)
        {
            _log.Debug(LoggerName, String.Format("Transition from current state {0} due to trigger {1}",
                _currentState, trigger));
            if (!_states.ContainsKey(_currentState))
            {
                _log.Error(LoggerName, String.Format("Invalid current state {0}", _currentState));
                return false; // No transition found
            }

            TStates? nextState = _states[_currentState].NextState(trigger);
            if (nextState == null)
            {
                _log.Debug(LoggerName, String.Format("No transition found, current state: {0}, trigger {1}",
                    _currentState, trigger));
                return false; // No transition found
            }

            // Check if the new state has been defined
            if (!_states.ContainsKey(nextState.Value))
            {
                _log.Debug(LoggerName, String.Format("Next state does not exist, current state {0}, trigger {1}, next state {2}",
                   _currentState, trigger, nextState.Value));
                return false; // No transition found
            }

            // We have a valid transition, go to that state
            _states[_currentState].ExitingState(trigger, obj);

            // We should have now exited the last state, enter the new one
            _currentState = nextState.Value;

            _states[_currentState].EnteringState(trigger, obj);
            _log.Debug(LoggerName, String.Format("Transitioned to new state {0}", _currentState));

            return true;
        }
    }
}
