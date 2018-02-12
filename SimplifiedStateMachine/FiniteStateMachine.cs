using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Peamel.BasicLogger;

namespace Peamel.SimpleFiniteStateMachine
{

    public class FiniteStateMachine<TStates, TTriggers>
        where TTriggers : struct, IComparable, IFormattable, IConvertible
        where TStates : struct, IComparable, IFormattable, IConvertible
    {
        private ILogger _log;
        private const String LoggerName = "FSM";

        Dictionary<TStates, State<TStates, TTriggers>> _states = new Dictionary<TStates, State<TStates, TTriggers>>();
        TStates _currentState;

        public TStates CurrentState
        {
            get { return _currentState; }
        }

        public FiniteStateMachine(TStates startupState)
        {
            _log = BasicLoggerFactory.GetLogger();
            _currentState = startupState;
        }

        public FiniteStateMachine(TStates startupState, ILogger logger)
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
            _log.Debug("FSM Configured: " + state, LoggerName);

            State<TStates, TTriggers> newState = State<TStates, TTriggers>.Configure(state);
            _states[state] = newState;
            return newState;
        }


        /// <summary>
        /// Fires the state, and sets a new state
        /// </summary>
        /// <param name="trigger"></param>
        public Boolean Fire(TTriggers trigger, Object obj = null)
        {
            _log.Debug(String.Format("Trigger Fired: State {0}, Trigger = {1}", _currentState, trigger), LoggerName);
            Boolean didTransition = TransitionStates(trigger, obj);
            if (didTransition)
            {
                _log.Debug("Transition Completed", LoggerName);
                return true;
            }

            // If it didn't transition, it might be because it's an internal trigger event
            _log.Debug("No transition, trying InternalTransition", LoggerName);
            Boolean internalTransition = InternalTransition(trigger, obj);
            if (internalTransition)
            {
                _log.Debug("Internal Transition Completed", LoggerName);
            }

            return internalTransition;
        }

        private Boolean InternalTransition(TTriggers trigger, Object obj)
        {
            if (!_states.ContainsKey(_currentState))
            {
                _log.Error(String.Format("Invalid current state {0}", _currentState), LoggerName);

                return false; // No transition found
            }

            Boolean didInternalTransition = _states[_currentState].InternalTransition(trigger, obj);

            _log.Debug(String.Format("InternalTransition: State {0}, Trigger = {1}, Returned = {2}",
                _currentState, trigger, didInternalTransition), LoggerName);

            return didInternalTransition;
        }

        private Boolean TransitionStates(TTriggers trigger, Object obj)
        {
            _log.Debug(String.Format("Transition from current state {0} due to trigger {1}",
                _currentState, trigger), LoggerName);
            if (!_states.ContainsKey(_currentState))
            {
                _log.Error(String.Format("Invalid current state {0}", _currentState), LoggerName);
                return false; // No transition found
            }

            TStates? nextState = _states[_currentState].NextState(trigger);
            if (nextState == null)
            {
                _log.Debug(String.Format("No transition found, current state: {0}, trigger {1}",
                    _currentState, trigger), LoggerName);
                return false; // No transition found
            }

            // Check if the new state has been defined
            if (!_states.ContainsKey(nextState.Value))
            {
                _log.Debug(String.Format("Next state does not exist, current state {0}, trigger {1}, next state {2}",
                   _currentState, trigger, nextState.Value), LoggerName);
                return false; // No transition found
            }

            // We have a valid transition, go to that state
            _states[_currentState].ExitingState(trigger, obj);

            // We should have now exited the last state, enter the new one
            _currentState = nextState.Value;

            _states[_currentState].EnteringState(trigger, obj);
            _log.Debug(String.Format("Transitioned to new state {0}", _currentState), LoggerName);

            return true;
        }
    }
}
