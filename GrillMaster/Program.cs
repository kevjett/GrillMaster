using System;
using System.Threading;
using GHI.OSHW.Hardware;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace GrillMaster 
{
    public class Program
    {
        public static long LastActivity { get; private set; }
        public static long StartTime { get; private set; }
        public static long CurrentTime { get { return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond); } }
        public static long Elapsed { get { return CurrentTime - LastActivity; } }
        public static void Main()
        {
            Initialize();

            while (true)
            {
                Menu.DoWork();
                if (GrillController.DoWork())
                    NewTempAvailable();
            }
        }

        static void Initialize()
        {
            Config.Initialize();
            BlinkLed(); //boot completed

            StartTime = CurrentTime;
            GrillController.Initialize();
            Menu.SetState(MenuState.Welcome);

            BlinkLed(); //initialize completed
        }

        static void BlinkLed()
        {
            Config.Pins.OnboardLed.Write(true);
            Thread.Sleep(350);
            Config.Pins.OnboardLed.Write(false);
            Thread.Sleep(50);
        }

        private static void NewTempAvailable()
        {
            Debug.Print("--NewTempAvailable");
            UpdateDisplay();
        }

        private static void UpdateDisplay()
        {
            Debug.Print("--Update Display");
            MenuState state = Menu.GetState();
            if (state == MenuState.Welcome)
            {
                if (CurrentTime-StartTime<5000)
                    Thread.Sleep((int)(5000 - (CurrentTime - StartTime)));
                //Menu.SetState(MenuState.Pit);
                //return;
            }

            for (int i = 0; i < GrillController.Probes.Length; i++)
            {
                if (GrillController.Probes[i].HasTemperature)
                    Debug.Print(GrillController.Probes[i].Name + ": " + GrillController.Probes[i].TemperatureF.ToString("N0"));
                else
                    Debug.Print(GrillController.Probes[i].Name + ": Not plugged in");
            }
        }

        public static void UpdateLastActivity()
        {
            LastActivity = CurrentTime;
        }
    }
}
