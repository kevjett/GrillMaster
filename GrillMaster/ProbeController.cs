using System;
using Microsoft.SPOT;

namespace GrillMaster
{
    public class ProbeController
    {
        IProbe _probe;
        private Config.ProbeState _state;

        public string Name { get; private set; }
        public Config.ProbeType ProbeType { get; private set; }
        public int TargetTemp { get; set; }
        public Config.ProbeState State { get { return _state; } set { SetState(value); } }
        public long StateChangedTime { get; set; }
        public Config.ProbeState PreviousState { get; set; }
        public System.Collections.ArrayList TargetReachedTimes { get; set; }
        public System.Collections.Stack TargetReachedTimespans { get; set; }
        public int TargetPercentRemaining { get { return GetTargetPercentRemaining(); } }

        public ProbeController(IProbe probe, string name, Config.ProbeType probeType, int startingTargetTemp)
        {
            _probe = probe;
            Name = name;
            ProbeType = probeType;
            TargetTemp = startingTargetTemp;
            State = Config.ProbeState.Unavailable;
            TargetReachedTimes = new System.Collections.ArrayList();
            TargetReachedTimespans = new System.Collections.Stack();
        }

        private void SetState(Config.ProbeState state)
        {
            PreviousState = State;
            if (PreviousState != state || StateChangedTime <= 0)
                StateChangedTime = Program.CurrentTime;
            _state = state;
        }

        private Config.ProbeState GetState()
        {
            PreviousState = _state;

            if (!HasTemp())
            {
                _state = Config.ProbeState.Unavailable;
                return _state;
            }

            if (IsTargetReached())
            {
                if (_state != Config.ProbeState.TargetReached)
                    _state = Config.ProbeState.TargetReached;

                if (PreviousState != Config.ProbeState.TargetReached)
                {
                    TargetReachedTimes.Add(Program.CurrentTime);
                    TargetReachedTimespans.Push(0);
                }
                else
                {
                    var timespan = (int)TargetReachedTimespans.Pop();
                    timespan = (int)(Program.CurrentTime - StateChangedTime);
                    TargetReachedTimespans.Push(timespan);
                }
            }
            else
            {
                if (_state != Config.ProbeState.SeekingTarget)
                    _state = Config.ProbeState.SeekingTarget;
            }

            return _state;
        }

        public double CurrentTemp
        {
            get { return _probe.CurrentTemp; }
        }

        public bool HasTemp()
        {
            return CurrentTemp > 0;
        }

        public bool IsTargetReached()
        {
            return CurrentTemp >= TargetTemp;
        }

        private int GetTargetPercentRemaining()
        {
            return (int)((CurrentTemp / TargetTemp) * 100);
        }

        public double GetPercentUnderTarget()
        {
            return (((TargetTemp - CurrentTemp) / TargetTemp) * 100);
        }
    }
}
