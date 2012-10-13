using System;
using System.Threading;
using GHI.OSHW.Hardware;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace GrillMaster
{
    public class Program
    {
        private static GrillController grill;
        private static readonly OutputPort _onboardLed = new OutputPort((Cpu.Pin)FEZCerbuino.Pin.Digital.LED1, true);

        public static void Main()
        {
            var probes = new TempProbe[1];
            probes[0] = new TempProbe("Food 1", FEZCerbuino.Pin.AnalogIn.A0);

            grill = new GrillController(probes);
            var timer = new Timer(LedBlink, null, 0, 1000);
            while (true)
            {
                if (grill.DoWork())
                    NewTempAvailable();
            }
        }

        static void LedBlink(object o)
        {
            _onboardLed.Write(!_onboardLed.Read());
        }

        private static void NewTempAvailable()
        {
            UpdateDisplay();
        }

        private static void UpdateDisplay()
        {
            for (int i = 0; i < grill.Probes.Length; i++)
            {
                Debug.Print(grill.Probes[i].Name + ": " + grill.Probes[i].TemperatureF.ToString("N0"));
            }
        }
    }
}
