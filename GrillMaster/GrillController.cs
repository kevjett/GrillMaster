using System;

namespace GrillMaster
{
    public class GrillController
    {
        private int _tempMeasurePeriod = 1000;
        private int _tempAvgCount = 8;
        private long _lastTempRead;
        private int _periodCounter;
        public TempProbe[] Probes { get; private set; }

        public GrillController(TempProbe[] probes)
        {
            _lastTempRead = 0;
            _periodCounter = 0;
            Probes = probes;
        }

        public bool DoWork()
        {
            var m = DateTime.Now.Ticks;
            var elapsed = m - _lastTempRead;
            if ((elapsed/TimeSpan.TicksPerMillisecond) < (_tempMeasurePeriod / _tempAvgCount))
                return false;
            _lastTempRead = m;

            for (var i=0; i<Probes.Length; i++)
                Probes[i].ReadTemp();
  
            ++_periodCounter;
            if (_periodCounter < _tempAvgCount)
                return false;

            for (var i = 0; i < Probes.Length; i++)
                Probes[i].CalculateTemp();

            _periodCounter = 0;
            return true;
        }
    }
}
