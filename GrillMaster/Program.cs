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
            Config.Pins.OnboardLed.Write(false);
            Thread.Sleep(500);
            Config.Pins.OnboardLed.Write(true);

            MenuState state = Menu.GetState();
            if (state == MenuState.Welcome)
            {
                if (CurrentTime - StartTime < (Config.WelcomeWait * 1000))
                {
                    Debug.Print("Waiting for welcome");
                    Thread.Sleep((int) ((Config.WelcomeWait*1000) - (CurrentTime - StartTime)));
                }
                Config.Lcd.Clear();
                Menu.SetState(MenuState.ShowTemps);
                return;
            }

            if (state == MenuState.ShowTemps)
            {
                for (var i = 0; i < 2; i++)
                {
                    Config.Lcd.SetCursorPosition(0, i);
                    var probe = GrillController.Probes[i];
                    if (probe.HasTemperature)
                    {
                        var text = probe.Name + ":" + probe.TemperatureF.ToString("N0") + "F [N]";
                        Debug.Print(text);
                        Config.Lcd.WriteLine(text);
                    }
                    else
                    {
                        var text = probe.Name + ":NaN [N]";
                        Debug.Print(text);
                        Config.Lcd.WriteLine(text);
                    }
                }

                Config.Lcd.SetCursorPosition(15, 0);
                Config.Lcd.Write(GrillController.IsFanRunning ? "*" : " ");
                Config.Lcd.SetCursorPosition(1, 0);
            }
        }

        public static void UpdateLastActivity()
        {
            LastActivity = CurrentTime;
        }
    }
}
