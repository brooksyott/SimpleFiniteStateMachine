using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Peamel.SimpleFiniteStateMachine
{
    public class State<TStates, TTriggers>
        where TTriggers : struct, IComparable, IFormattable, IConvertible
        where TStates : struct, IComparable, IFormattable, IConvertible
    {
        TStates _state;
        public TStates StateType
        {
            get { return _state; }
        }


        int _numberOfTriggers = 0;
        //Dictionary<Triggers, Func<Triggers, States>> _onEntryAction = new Dictionary<Triggers, Func<Triggers, States>>();
        //Dictionary<Triggers, Func<Triggers, States>> _onExitAction = new Dictionary<Triggers, Func<Triggers, States>>();
        List<Action<Object, TTriggers>> _onEntryAction = new List<Action<Object, TTriggers>>();
        List<Action<Object, TTriggers>> _onExitAction = new List<Action<Object, TTriggers>>();
        List<InternalTransition<TTriggers>> _onSelfTriggerAction = new List<InternalTransition<TTriggers>>();
        List<Transition<TStates, TTriggers>> _transitions = new List<Transition<TStates, TTriggers>>();

        private State()
        {
            _numberOfTriggers = Enum.GetNames(typeof(TTriggers)).Length;
        }

        /// <summary>
        /// Throws an exception if the state transition has not been defined
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        private TStates InvalidTrigger(TTriggers trigger)
        {
            throw new StateTransitionException("Invalid Trigger: " + trigger);
        }

        static public State<TStates, TTriggers> Configure(TStates state)
        {
            State<TStates, TTriggers> tempState = new State<TStates, TTriggers>();
            tempState._state = state;
            return new State<TStates, TTriggers>();
        }

        public State<TStates, TTriggers> Permit(TTriggers trigger, Action<Object, TTriggers> func)
        {
            PermitIf(trigger, func, EmptyGuard);
            return this;
        }

        /// <summary>
        /// Sets up an action based on a trigger
        /// Returns the pointer to the statemachine as per a Fluent Design (This may not be perfect)
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        //public State<TStates, TTriggers> Permit(TTriggers trigger, Func<Boolean> func)
        public State<TStates, TTriggers> PermitIf(TTriggers trigger, Action<Object, TTriggers> func, Func<Boolean> Guard)
        {
            InternalTransition<TTriggers> tTransition = new InternalTransition<TTriggers>();
            tTransition.Trigger = trigger;
            tTransition.Exec = func;
            tTransition.Guard = Guard;

            _onSelfTriggerAction.Add(tTransition);
            return this;
        }

        /// <summary>
        /// Sets up an action based on a trigger
        /// Returns the pointer to the statemachine as per a Fluent Design (This may not be perfect)
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public State<TStates, TTriggers> OnEntry(Action<Object, TTriggers> func)
        {
            _onEntryAction.Add(func);
            return this;
        }

        /// <summary>
        /// Sets up an action based on a trigger
        /// Returns the pointer to the statemachine as per a Fluent Design (This may not be perfect)
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public State<TStates, TTriggers> OnExit(Action<Object, TTriggers> func)
        {
            _onExitAction.Add(func);
            return this;
        }

        /// <summary>
        /// Initiates the action and returns the new state
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public void EnteringState(TTriggers trigger, Object obj)
        {
            foreach (Action<Object, TTriggers> func in _onEntryAction)
            {
                func.Invoke(obj, trigger);
            }
        }

        /// <summary>
        /// Initiates the action and returns the new state
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public void ExitingState(TTriggers trigger, Object obj)
        {
            foreach (Action<Object, TTriggers> func in _onExitAction)
            {
                func.Invoke(obj, trigger);
            }
        }

        public State<TStates, TTriggers> Permit(TTriggers trigger, TStates newState)
        {
            return PermitIf(trigger, newState, EmptyGuard);
        }

        public State<TStates, TTriggers> PermitIf(TTriggers trigger, TStates newState, Func<Boolean> guard)
        {
            Transition<TStates, TTriggers> tTransition = new Transition<TStates, TTriggers>();
            tTransition.Trigger = trigger;
            tTransition.State = newState;
            tTransition.Guard = guard;

            _transitions.Add(tTransition);
            return this;
        }

        public TStates? NextState(TTriggers trigger)
        {
            foreach(Transition<TStates, TTriggers> trans in _transitions)
            {
                int c = trans.Trigger.CompareTo(trigger);
                if (c == 0)
                {
                    // We have a valid, trigger, check the guard
                    if (trans.Guard != null)
                    {
                        Boolean guardPassed = trans.Guard.Invoke();
                        if (guardPassed)
                        {
                            return trans.State;
                        }
                    }
                }
            }

            return null;
        }

        public Boolean InternalTransition(TTriggers trigger, Object obj)
        {
            foreach (InternalTransition<TTriggers> trans in _onSelfTriggerAction)
            {
                int c = trans.Trigger.CompareTo(trigger);
                if (c == 0)
                {
                    // We have a valid, trigger, check the guard
                    if (trans.Exec != null)
                    {
                        Boolean guardPassed = trans.Guard.Invoke();
                        if (guardPassed)
                        {
                            trans.Exec.Invoke(obj, trigger);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private Boolean EmptyGuard()
        {
            return true;
        }
    }
}
