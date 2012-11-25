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
                    Menu.UpdateScreen();
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

        public static void UpdateLastActivity()
        {
            LastActivity = CurrentTime;
        }
    }
}
