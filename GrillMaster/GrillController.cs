using System;

namespace GrillMaster
{
    public static class GrillController
    {
        private static long _lastTempRead = 0;
        private static int _periodCounter = 0;
        private static int _lidOpenDuration = Config.LidOpenAutoResume;
        private static bool _pitTempReached = false;

        public static ProbeController[] Probes { get; set; }
        public static int LidOpenResumeCountdown { get; set; }

        public static void Initialize()
        {
            Probes = new ProbeController[Config.Probes.Count];
            foreach (Config.ProbeType key in Config.Probes.Keys)
            {
                var probe = (ProbeController)Config.Probes[key];
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

            ++_periodCounter;
            if (_periodCounter < Config.TempAverageCount)
                return false;

            ControlFan();

            _periodCounter = 0;
            return true;
        }

        public static void ControlFan()
        {
            var pit = Probes[(int) Config.ProbeType.Pit];

            if (!pit.HasTemp())
            {
                Config.Fan.CurrentSpeed = Fan.Speed.None;
                return;
            }

            if (pit.State == Config.ProbeState.TargetReached && (_lidOpenDuration-LidOpenResumeCountdown) >= Config.LidOpenAutoResume)
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
            else if (_pitTempReached && pit.GetPercentUnderTarget() >= Config.LidOpenOffset)
            {
                ResetLidOpenResumeCountdown();
            }

            if (_pitTempReached)
                Config.Fan.CurrentSpeed = Fan.Speed.None;
            else
            {
                Config.Fan.CurrentSpeed = Fan.Speed.Percent_100;
            }
        }

        private static void ResetLidOpenResumeCountdown()
        {
            LidOpenResumeCountdown = _lidOpenDuration;
            _pitTempReached = false;
        }
    }
}
