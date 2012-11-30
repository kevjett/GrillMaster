using System;
using System.Threading;
using Microsoft.SPOT;

namespace GrillMaster
{
    public enum MenuState
    {
        None,
        Welcome,
        Pit,
        SetTemp_Pit,
        Food1,
        SetTemp_Food1,
        ShowTemps,
        Reports,
        Report_Pit
    }

    public enum Button
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public static class Menu
    {
        public static MenuPage _currentPage;
        public static MenuState _currentState;
        public static Button _currentButton;

        public static void DoWork()
        {
            Button button = ReadButton();

            return; //notdone
            if (Program.Elapsed < 250)
                return;

            _currentButton = button;

            if (_currentButton == Button.None)
                return;

            Program.UpdateLastActivity();

            MenuState newState = _currentPage.GetNewState(button);
            if (newState == MenuState.None)
                return;

            SetState(newState);
        }

        private static Button ReadButton()
        {
            //see if the user has pushed a button
            var buttonRead = Config.Pins.Buttons.ReadRaw() >> 2;

            Debug.Print("Button Read:" + buttonRead);

            return Button.None;
        }

        public static MenuState GetState()
        {
            return _currentPage.State;
        }

        public static void SetState(MenuState state)
        {
            _currentPage = (MenuPage) Config.Menus[state];
            _currentState = state;

            Config.Lcd.Clear();
            UpdateScreen();

            Program.UpdateLastActivity();
        }

        public static void UpdateScreen()
        {
            Debug.Print("--Update Screen--");
            Config.Pins.OnboardLed.Write(false);
            Thread.Sleep(500);
            Config.Pins.OnboardLed.Write(true);

            switch (_currentState)
            {
                case MenuState.Welcome:
                    ShowWelcome();
                    break;
                case MenuState.ShowTemps:
                    ShowTemps();
                    break;
                case MenuState.None:
                default:
                    return;
            }
        }

        private static void ShowWelcome()
        {
            Debug.Print("Show Welcome");
            Config.Lcd.Clear();
            Config.Lcd.SetCursorPosition(0, 0);
            Config.Lcd.WriteLine("Welcome Kevin");
            Config.Lcd.SetCursorPosition(0, 1);
            Config.Lcd.WriteLine("Happy Grilling!");

            if (Program.CurrentTime - Program.StartTime < (Config.WelcomeWait * 1000))
            {
                Debug.Print("Waiting for welcome");
                Thread.Sleep((int)((Config.WelcomeWait * 1000) - (Program.CurrentTime - Program.StartTime)));
            }

            SetState(MenuState.ShowTemps);
        }

        private static void ShowTemps()
        {
            for (var i = 0; i < 2; i++)
            {
                var probe = GrillController.Probes[i];
                var text = "";
                if (!probe.HasTemperature)
                {
                    text = "- No " + probe.Name + " Probe -";
                }
                else if (i == (int)Config.ProbeType.Pit && GrillController.LidOpenResumeCountdown > 0)
                {
                    text = probe.Name + ":" + probe.TemperatureF.ToString("N0") + "F Lid:" + GrillController.LidOpenResumeCountdown;
                }
                else
                {
                    text = probe.Name + ":" + probe.TemperatureF.ToString("N0") + "F [" + probe.TargetTemp + "]";
                }
                Config.Lcd.SetCursorPosition(0, i);
                Debug.Print(text);
                Config.Lcd.WriteLine(text);
            }

            Config.Lcd.SetCursorPosition(15, 0);
            Config.Lcd.Write(GrillController.IsFanRunning ? "*" : " ");
            Config.Lcd.SetCursorPosition(0, 0);
        }
    }
}
