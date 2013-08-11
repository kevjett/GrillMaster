using System;
using Microsoft.SPOT;

namespace GrillMaster
{
    public abstract class Probe
    {
        public string Name { get; private set; }
        public Config.ProbeType ProbeType { get; private set; }
        public int TargetTemp { get; set; }
        private Config.ProbeState _state;
        public Config.ProbeState State { get { return _state; } set { SetState(value); } }
        public long StateChangedTime { get; set; }
        public Config.ProbeState PreviousState { get; set; }
        public System.Collections.ArrayList TargetReachedTimes { get; set; }
        public System.Collections.Stack TargetReachedTimespans { get; set; }
        public int TargetPercentRemaining { get { return GetTargetPercentRemaining(); } }

        public Probe(string name, Config.ProbeType probeType, int startingTargetTemp)
        {
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

        public bool IsTargetReached()
        {
            return GetTemp() >= TargetTemp;
        }

        public bool HasTemp()
        {
            return GetTemp() >= 0;
        }

        private int GetTargetPercentRemaining()
        {
            return (int)((GetTemp() / TargetTemp) * 100);
        }

        public double GetPercentUnderTarget()
        {
            return (((TargetTemp - GetTemp()) / TargetTemp) * 100);
        }

        public virtual void PreReadTemp() { }
        public abstract double GetTemp();
    }
}
