﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Peamel.SimpleFiniteStateMachine;
using Peamel.BasicLogger;

namespace PhoneCallTest
{
    public class PhoneCall
    {
        enum Triggers
        {
            CallDialed,
            TakeOffHook,
            CallConnected,
            LeftMessage,
            PlacedOnHold,
            TakenOffHold,
            Hangup,
            PhoneHurledAgainstWall,
            MuteMicrophone,
            UnmuteMicrophone,
            SetVolume
        }

        enum States
        {
            OnHook,
            OffHook,
            Ringing,
            Connected,
            OnHold,
            PhoneDestroyed
        }

        string _caller;
        string _callee;
        States _state = States.OnHook;
        FiniteStateMachine<States, Triggers> _machine;
        ILogger _log;

        public PhoneCall(string caller)
        {
            _log = BasicLoggerFactory.CreateLogger("c:\\temp\\phone.log");
            _log.SetLogLevel(BASICLOGGERLEVELS.INFO);

            _caller = caller;

            _machine = new FiniteStateMachine<States, Triggers>(_state, _log);

            _machine.Configure(States.OnHook)
                .Permit(Triggers.TakeOffHook, States.OffHook);

            _machine.Configure(States.OffHook)
                .Permit(Triggers.CallDialed, States.Ringing)
                .Permit(Triggers.Hangup, States.OnHook);

            _machine.Configure(States.Ringing)
                .OnEntry((callee, trigger) => OnDialed(callee, trigger))
                .Permit(Triggers.CallConnected, States.Connected)
                .Permit(Triggers.Hangup, States.OnHook);

            _machine.Configure(States.Connected)
                .OnEntry((t, k) => StartCallTimer())
                .OnExit((t, k) => StopCallTimer())
               .Permit(Triggers.SetVolume, (volume, trigger) => OnSetVolume((int)volume))
               .PermitIf(Triggers.MuteMicrophone, (t, k) => OnMute(), () => NotMuted())
               .Permit(Triggers.UnmuteMicrophone, (t, k) => OnUnmute())
               .Permit(Triggers.Hangup, States.OnHook);
        }

        int _volume = 5;

        Boolean OnSetVolume(int volume)
        {
            _volume = volume;
            _log.Info(String.Format("Volume set to " + volume + "!"));
            return true;
        }

        Boolean _isMuted = false;
        Boolean NotMuted()
        {
            _log.Info(String.Format("Microphone is muted = " + _isMuted));
            return !_isMuted;
        }

        Boolean OnUnmute()
        {
            _log.Info(String.Format("Microphone unmuted!"));
            _isMuted = false;
            return true;
        }

        Boolean OnMute()
        {
            _log.Info(String.Format("Microphone muted!"));
            _isMuted = true;
            return true;
        }

        Boolean OnDialed(Object callee, Object trigger = null)
        {
            _callee = (string)callee;
            _log.Info(String.Format("[Phone Call] placed from {0} to {1}", _caller, _callee));
            return true;
        }

        Boolean StartCallTimer()
        {
            _log.Info(String.Format("[Timer:] Call started at {0}", DateTime.Now));
            return true;
        }

        Boolean StopCallTimer()
        {
            _log.Info(String.Format("[Timer:] Call ended at {0}", DateTime.Now));
            return true;
        }

        public void Mute()
        {
            _log.Info("Firing Triggers.MuteMicrophone ");
            _machine.Fire(Triggers.MuteMicrophone);
        }

        public void Unmute()
        {
            _log.Info("Firing Triggers.UnmuteMicrophone ");
            _machine.Fire(Triggers.UnmuteMicrophone);
        }

        public void SetVolume(int volume)
        {
            _log.Info("Firing Triggers.SetVolume " + volume);
            _machine.Fire(Triggers.SetVolume, volume);
        }

        public void Print()
        {
            Console.WriteLine("[{1}] placed call and [Status:] {0}", _machine, _caller);
        }

        public void TakeOffHook()
        {
            _log.Info("Firing Triggers.TakeOffHook ");
            _machine.Fire(Triggers.TakeOffHook);
        }

        public void HangUp()
        {
            _log.Info("Firing Triggers.HangUp ");
            _machine.Fire(Triggers.Hangup);
        }


        public void Dialed(string callee)
        {
            _log.Info("Firing Triggers.CallDialed to " + callee);
            _machine.Fire(Triggers.CallDialed, callee);
        }

        public void Connected()
        {
            _log.Info("Firing Triggers.CallConnected ");
            _machine.Fire(Triggers.CallConnected);
        }

        public void Hold()
        {
            _machine.Fire(Triggers.PlacedOnHold);
        }

        public void Resume()
        {
            _machine.Fire(Triggers.TakenOffHold);
        }
    }
}
