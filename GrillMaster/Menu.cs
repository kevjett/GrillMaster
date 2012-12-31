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
        Right,
        RightDown,
        RightUp,
        LeftDown,
        LeftUp,
        LeftRight,
        UpDown
    }

    public static class Menu
    {
        public static MenuPage _currentPage;
        public static MenuPage _previousPage;
        public static MenuState _currentState;
        public static Button _currentButton;
        public static long _buttonDepressedTime;
        public static long _buttonDepressedElapsed;

        public static void DoWork()
        {
            Button button = ReadButton();

            //return; //notdone
            //if (Program.Elapsed < 25)
            //    return;

            var previousButton = _currentButton;

            _currentButton = button;

            if (_currentButton == Button.None)
                return;

            if (previousButton != _currentButton)
            {
                _buttonDepressedTime = Program.CurrentTime;
                _buttonDepressedElapsed = 0;
            }
            else
            {
                _buttonDepressedElapsed = Program.CurrentTime - _buttonDepressedTime;
            }

            Debug.Print("Button Pushed:" + button.ToString());

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

            if (buttonRead > 10)
                Debug.Print("Button Read:" + buttonRead);

            if (buttonRead >= 130 && buttonRead <= 150)
                return Button.Up;
            if (buttonRead >= 250 && buttonRead <= 280)
                return Button.Left;
            if (buttonRead >= 340 && buttonRead <= 350)
                return Button.LeftUp;
            if (buttonRead >= 450 && buttonRead <= 480)
                return Button.Down;
            if (buttonRead >= 520 && buttonRead <= 530)
                return Button.UpDown;
            if (buttonRead >= 580 && buttonRead <= 590)
                return Button.LeftDown;
            if (buttonRead >= 700 && buttonRead <= 780)
                return Button.Right;
            if (buttonRead >= 760 && buttonRead <= 770)
                return Button.RightUp;
            if (buttonRead >= 800 && buttonRead <= 810)
                return Button.LeftRight; 
            if (buttonRead >= 860 && buttonRead <= 870)
                return Button.RightDown;

            return Button.None;
        }

        public static MenuState GetState()
        {
            return _currentPage.State;
        }

        public static void SetState(MenuState state)
        {
            _previousPage = _currentPage;
            _currentPage = (MenuPage) Config.Menus[state];
            _currentState = state;

            switch (state)
            {
                case MenuState.SetTemp_Food1:
                    changeFood1();
                    break;
                case MenuState.SetTemp_Pit:
                    changePit();
                    break;
            }

            if (_currentPage != _previousPage)
            {
                Debug.Print("State change:" + state);
                Config.Lcd.Clear();
            }

            UpdateScreen();

            Program.UpdateLastActivity();
        }

        public static void changeFood1()
        {
            var probe = GrillController.Probes[(int)Config.ProbeType.Food1];
            changeTargetTemp(probe);
        }

        public static void changePit()
        {
            var probe = GrillController.Probes[(int)Config.ProbeType.Pit];
            changeTargetTemp(probe);
        }

        public static void changeTargetTemp(TempProbe probe)
        {
            var changeMultiplier = 1;
            if (_buttonDepressedElapsed > 3000)
            {
                changeMultiplier = 5;
            }
            if (_buttonDepressedElapsed > 6000)
            {
                changeMultiplier = 10;
            }
            if (_currentButton == Button.Up)
            {
                if (probe.TargetTemp < 700)
                {
                    probe.TargetTemp += 1 * changeMultiplier;
                    Debug.Print(probe.Name + ": Up target temp to " + probe.TargetTemp);
                }
                else
                {
                    Debug.Print(probe.Name + ": Max target reached");
                }
            }
            else if (_currentButton == Button.Down)
            {
                if (probe.TargetTemp > 80)
                {
                    probe.TargetTemp -= 1 * changeMultiplier;
                    Debug.Print(probe.Name + ": Down target temp to " + probe.TargetTemp);
                }
                else
                {
                    Debug.Print(probe.Name + ": Min target reached");
                }
            }
        }

        public static void UpdateScreen()
        {
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
                case MenuState.Pit:
                case MenuState.SetTemp_Pit:
                    ShowPit();
                    break;
                case MenuState.Food1:
                case MenuState.SetTemp_Food1:
                    ShowFood();
                    break;
                case MenuState.Reports:
                    ShowReports();
                    break;
                case MenuState.None:
                default:
                    return;
            }
        }

        private static void ShowWelcome()
        {
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
                
                Config.Lcd.SetCursorPosition(0, i);
                Config.Lcd.WriteLine(GetProbeTempText(probe));
            }

            Config.Lcd.SetCursorPosition(15, 0);
            Config.Lcd.Write(GrillController.IsFanRunning ? "*" : " ");
            Config.Lcd.SetCursorPosition(0, 0);
        }

        private static string GetProbeTempText(TempProbe probe)
        {
            if (!probe.HasTemperature)
            {
                return "-No " + probe.Name + " Probe";
            }
            else if (probe.ProbeType == Config.ProbeType.Pit && GrillController.LidOpenResumeCountdown > 0)
            {
                return probe.Name + ":" + probe.TemperatureF.ToString("N0") + "F Lid:" + GrillController.LidOpenResumeCountdown;
            }
            else
            {
                return probe.Name + ":" + probe.TemperatureF.ToString("N0") + "F [" + probe.TargetTemp + "]";
            }
        }

        private static void ShowFood()
        {
            var probe = GrillController.Probes[(int)Config.ProbeType.Food1];

            Config.Lcd.SetCursorPosition(0, 0);
            Config.Lcd.WriteLine(GetProbeTempText(probe));

            if (probe.HasTemperature && _currentState == MenuState.SetTemp_Food1)
            {
                Config.Lcd.SetCursorPosition(15, 0);
                if (_currentButton == Button.Up)
                    Config.Lcd.Write("^");
                else if (_currentButton == Button.Down)
                    Config.Lcd.Write("V");
            }

            var text = "";
            if (!probe.TargetReached && probe.HasTemperature)
            {
                text = "G-" + probe.TargetPercentRemaining + "% T-" + Program.TimeText(Program.CurrentTime-probe.StateChangedTime);
            }
            else if (probe.TargetReached && probe.HasTemperature)
            {
                //text = "Goal-" + Program.TimeText((int)probe.TargetReachedTimespans.Peek());
            }

            Config.Lcd.SetCursorPosition(0, 1);
            Config.Lcd.WriteLine(text);
        }

        private static void ShowPit()
        {
            var probe = GrillController.Probes[(int)Config.ProbeType.Pit];

            Config.Lcd.SetCursorPosition(0, 0);
            Config.Lcd.WriteLine(GetProbeTempText(probe));

            if (probe.HasTemperature && _currentState == MenuState.SetTemp_Pit)
            {
                Config.Lcd.SetCursorPosition(15, 0);
                if (_currentButton == Button.Up)
                    Config.Lcd.Write("^");
                else if (_currentButton == Button.Down)
                    Config.Lcd.Write("V");
            }

            var text = "";
            if (!probe.TargetReached && probe.HasTemperature)
            {
                text = "G-" + probe.TargetPercentRemaining + "% T-" + Program.TimeText(Program.CurrentTime - probe.StateChangedTime);
            }
            else if (probe.TargetReached && probe.HasTemperature)
            {
                //text = "Goal-" + Program.TimeText((int)probe.TargetReachedTimespans.Peek());
            }

            Config.Lcd.SetCursorPosition(0, 1);
            Config.Lcd.WriteLine(text);
        }

        private static void ShowReports()
        {
            Config.Lcd.SetCursorPosition(0, 0);
            Config.Lcd.WriteLine("See Reports");
        }
    }
}
