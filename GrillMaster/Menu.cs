using System;
using System.Threading;
using Microsoft.SPOT;

namespace GrillMaster
{
    public enum MenuState
    {
        None,
        Welcome,
        SetTemp_Pit,
        SetTemp_Food1,
        Food1,
        Pit,
        Reports,
        Report_Pit
    }

    public enum Button
    {
        None,
        Up,
        Down,
        Middle
    }

    public static class Menu
    {
        public static MenuPage _currentPage;
        public static Button _currentButton;

        public static void DoWork()
        {
            return; //notdone
            Button button = ReadButton();

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
            return Button.None;
        }

        public static MenuState GetState()
        {
            return _currentPage.State;
        }

        public static void SetState(MenuState state)
        {
            _currentPage = (MenuPage) Config.Menus[state];

            switch (state)
            {
                case MenuState.Welcome:
                    ShowWelcome();
                    break;
                case MenuState.Food1:
                    ShowFood(Config.ProbeType.Food1);
                    break;
                case MenuState.Pit:
                    ShowFood(Config.ProbeType.Pit);
                    break;
                case MenuState.None:
                default:
                    return;
            }


            Program.UpdateLastActivity();
        }

        private static void ShowWelcome()
        {
            Debug.Print("Welcome Kevin");
            //Config.Lcd.Write("Hello, world!");
            Config.Lcd.SetCursorPosition(0, 1);
            Config.Lcd.Write("Welcome Kevin");
        }

        private static void ShowFood(Config.ProbeType probeType)
        {
            Debug.Print("Show Food:"+probeType.ToString());
        }
    }
}
