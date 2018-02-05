using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peamel.SimpleFiniteStateMachine
{
    public class EnhancedNavigationFSM<TStates, TTriggers>
        where TTriggers : struct, IComparable, IFormattable, IConvertible
        where TStates : struct, IComparable, IFormattable, IConvertible
    {
        Dictionary<TStates, State<TStates, TTriggers>> _states = new Dictionary<TStates, State<TStates, TTriggers>>();
        TStates _currentState;
        public TStates CurrentState
        {
            get { return _currentState; }
        }

        public EnhancedNavigationFSM(TStates startupState)
        {
            _currentState = startupState;
        }

        /// <summary>
        /// Adds a state to the statemachine
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="state"></param>
        public State<TStates, TTriggers> Configure(TStates state)
        {
            State<TStates, TTriggers> newState = State<TStates, TTriggers>.Configure(state);
            _states[state] = newState;
            return newState;
        }


        /// <summary>
        /// Fires the state, and sets a new state
        /// </summary>
        /// <param name="trigger"></param>
        public Boolean Fire(TTriggers trigger)
        {
            Debug.WriteLine("Fire Start: State {0}, Trigger = {1}", _currentState, trigger);
            Boolean didTransition = TransitionStates(trigger);
            if (didTransition) return true;

            // If it didn't transition, it might be because it's an internal trigger event
            Boolean internalTransition = InternalTransition(trigger);
            return internalTransition;
        }

        private Boolean InternalTransition(TTriggers trigger)
        {
            if (!_states.ContainsKey(_currentState))
            {
                return false; // No transition found
            }

            Boolean didInternalTransition = _states[_currentState].InternalTransition(trigger);
            Debug.WriteLine("InternalTransition: State {0}, Trigger = {1}, Found = {2}", 
                _currentState, trigger, didInternalTransition);
            return true;
        }

        private Boolean TransitionStates(TTriggers trigger)
        {
            if (!_states.ContainsKey(_currentState))
            {
                return false; // No transition found
            }

            TStates? nextState = _states[_currentState].NextState(trigger);
            if (nextState == null)
            {
                return false; // No transition found
            }

            // Check if the new state has been defined
            if (!_states.ContainsKey(nextState.Value))
            {
                return false; // No transition found
            }
            // We have a valid transition, go to that state
            _states[_currentState].ExitingState(trigger);

            // We should have now exited the last state, enter the new one
            _currentState = nextState.Value;

            _states[_currentState].EnteringState(trigger);
            Debug.WriteLine("Fire End: State {0}, Trigger = {1}", _currentState, trigger);
            return true;
        }
    }
}
