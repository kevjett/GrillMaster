using System;

namespace GrillMaster
{
    public static class GrillController
    {
        private static long _lastTempRead = 0;
        private static int _periodCounter = 0;
        private static int _lidOpenDuration = Config.LidOpenAutoResume;
        private static bool _pitTempReached = false;

        public static TempProbe[] Probes { get; set; }
        public static int LidOpenResumeCountdown { get; set; }

        public static bool IsFanRunning
        {
            get { return Config.Pins.Fan.Read(); }
        }

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

            ControlFan();

            _periodCounter = 0;
            return true;
        }

        public static void ControlFan()
        {
            var pit = Probes[(int) Config.ProbeType.Pit];

            if (!pit.HasTemperature)
            {
                Config.Pins.Fan.Write(false);
                return;
            }

            if (pit.TargetReached && (_lidOpenDuration-LidOpenResumeCountdown) >= Config.LidOpenAutoResume)
            {
                if (!_pitTempReached)
                {
                    _pitTempReached = true;
                }
                LidOpenResumeCountdown = 0;
            } 
            else if (LidOpenResumeCountdown != 0)
            {
                LidOpenResumeCountdown = LidOpenResumeCountdown - (Config.TempMeasurePeriod/1000);
            }
            else if (_pitTempReached && (((pit.TargetTemp - pit.TemperatureF) / pit.TargetTemp) * 100) >= Config.LidOpenOffset)
            {
                ResetLidOpenResumeCountdown();
            }

            Config.Pins.Fan.Write(!_pitTempReached);
        }

        private static void ResetLidOpenResumeCountdown()
        {
            LidOpenResumeCountdown = _lidOpenDuration;
            _pitTempReached = false;
        }
    }
}
