using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Peamel.BasicLogger;


namespace Peamel.SimpleFiniteStateMachine
{
    public class TriggerArgs<TTriggers>
         where TTriggers : struct, IComparable, IFormattable, IConvertible
    {
        public TriggerArgs()
        {
        }

        public TriggerArgs(TTriggers t, Object obj)
        {
            Trigger = t;
            Obj = obj;
        }

        public TTriggers Trigger { get; set; }
        public Object Obj;
    }

    public class FiniteStateMachineThreadSafe<TStates, TTriggers> : FiniteStateMachineBase<TStates, TTriggers>
        where TTriggers : struct, IComparable, IFormattable, IConvertible
        where TStates : struct, IComparable, IFormattable, IConvertible
    {
        // Create a thread safe queue
        BlockingCollection<TriggerArgs<TTriggers>> _triggerQueue = new BlockingCollection<TriggerArgs<TTriggers>>(new ConcurrentQueue<TriggerArgs<TTriggers>>(), 100);

        public FiniteStateMachineThreadSafe(TStates startupState)
            : base(startupState)
        {
            Task.Factory.StartNew(() => ProcessIncomingTriggers());
        }

        public FiniteStateMachineThreadSafe(TStates startupState, ILogger logger)
            : base(startupState, logger)
        {
            Task.Factory.StartNew(() => ProcessIncomingTriggers());
        }

        private void ProcessIncomingTriggers()
        {
            Boolean fireRc = false;
            TriggerArgs<TTriggers> triggerEvent;
            Boolean keepProcessing = true;

            while (keepProcessing)
            {
                while (_triggerQueue.TryTake(out triggerEvent, 10000))
                {
                    try
                    {
                        if (triggerEvent != null)
                        {
                            fireRc = FireInternal(triggerEvent.Trigger, triggerEvent.Obj);
                        }

                    }
                    catch (Exception ee)
                    {
                        _log.Error(LoggerName, "Exception on handling incoming trigger: " + ee.ToString());
                    }
                }
            }

            _log.Debug(LoggerName, "Exiting waiting on triggerEvent");
        }

        override public Boolean Fire(TTriggers trigger, Object obj = null)
        {
            TriggerArgs<TTriggers> triggerEvent = new TriggerArgs<TTriggers>(trigger, obj);
            _triggerQueue.Add(triggerEvent);
            return true;
        }


        private Boolean FireInternal(TTriggers trigger, Object obj = null)
        {
            _log.Debug(LoggerName, String.Format("Trigger Fired: State {0}, Trigger = {1}", _currentState, trigger));
            Boolean didTransition = TransitionStates(trigger, obj);
            if (didTransition)
            {
                _log.Debug(LoggerName, "Transition Completed");
                return true;
            }

            // If it didn't transition, it might be because it's an internal trigger event
            _log.Debug(LoggerName, "No transition, trying InternalTransition");
            Boolean internalTransition = InternalTransition(trigger, obj);
            if (internalTransition)
            {
                _log.Debug(LoggerName, "Internal Transition Completed");
            }

            return internalTransition;
        }

    }

}
