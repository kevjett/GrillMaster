using System;

namespace GrillMaster
{
    public static class GrillController
    {
        private static long _lastTempRead = 0;
        private static int _periodCounter = 0;
        public static TempProbe[] Probes;

        public static void Initialize()
        {
            Probes = new TempProbe[Config.Probes.Count];
            foreach (Config.ProbeType key in Config.Probes.Keys)
            {
                var probe = (TempProbe) Config.Probes[key];
                Probes[(int) probe.ProbeType] = probe;
            }
        }

        public static bool DoWork()
        {
            var m = DateTime.Now.Ticks;
            var elapsed = m - _lastTempRead;
            if ((elapsed/TimeSpan.TicksPerMillisecond) < (Config.TempMeasurePeriod / Config.TempAverageCount))
                return false;
            _lastTempRead = m;

            for (var i=0; i<Probes.Length; i++)
                Probes[i].ReadTemp();
  
            ++_periodCounter;
            if (_periodCounter < Config.TempAverageCount)
                return false;

            for (var i = 0; i < Probes.Length; i++)
                Probes[i].CalculateTemp();

            _periodCounter = 0;
            return true;
        }
    }
}
