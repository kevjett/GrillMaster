using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace GrillMaster
{
    public partial class Program
    {
        public static long LastActivity { get; private set; }
        public static long StartTime { get; private set; }
        public static long CurrentTime { get { return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond); } }
        public static long Elapsed { get { return CurrentTime - LastActivity; } }
        public static long TotalElapsed { get { return CurrentTime - StartTime; } }

        private static bool _hasWelcomeShown = false;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/
            
            Initialize();

            GT.Timer timer = new GT.Timer(500);
            timer.Tick += new GT.Timer.TickEventHandler(timer_Tick);
            timer.Start();
        }

        bool speedtestdone = false;

        void timer_Tick(GT.Timer timer)
        {
            if (!_hasWelcomeShown)
            {
                _hasWelcomeShown = true;
                Menu.SetState(MenuState.Welcome);
            }

            //if (!speedtestdone)
            //{
            //    speedtestdone = true;
            //    Menu.SpeedTest();
            //    Menu.SetState(MenuState.ShowTemps);
            //}

            Menu.DoWork();
            if (GrillController.DoWork())
                Menu.UpdateScreen();
        }

        void Initialize()
        {
            Config.SetupLcd(display_HD44780);
            Config.SetupThemocouple(thermocouple);
            Config.Initialize();
            BlinkLed(); //boot completed

            StartTime = CurrentTime;
            GrillController.Initialize();

            BlinkLed(); //initialize completed
        }

        static void BlinkLed()
        {
            //Config.Pins.OnboardLed.Write(true);
            Thread.Sleep(350);
            //Config.Pins.OnboardLed.Write(false);
            Thread.Sleep(50);
        }

        public static void UpdateLastActivity()
        {
            LastActivity = CurrentTime;
        }

        public static string TimeText(long milliseconds)
        {
            TimeSpan t = TimeSpan.FromTicks(milliseconds * TimeSpan.TicksPerMillisecond);

            var secs = t.Seconds.ToString();
            if (t.Seconds < 10) secs = "0" + secs;

            string answer = t.Minutes + ":" + secs;

            if (t.Hours > 0)
                return t.Hours + ":" + answer;

            return answer;
        }
    }
}
