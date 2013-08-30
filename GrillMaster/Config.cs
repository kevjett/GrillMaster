using System;
using System.Collections;
using System.Threading;
using GHI.OSHW.Hardware;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Gadgeteer.Modules.GHIElectronics;

namespace GrillMaster
{
    public static class Config
    {
        public const int WelcomeWait = 5; //secs to wait for welcome
        public const int DefaultPitTemp = 85; //default target temp for pit
        public const int DefaultFoodTemp = 160; //default target temp for food
        public const int LidOpenAutoResume = 30; //number of seconds to wait after temp has been reached and temp goes below target
        public const int LidOpenOffset = 6; //The percentage the temperature drops before automatic lidopen mode

        public enum ProbeType
        {
            Pit = 0,
            Food1 = 1
        }

        public enum ProbeState
        {
            Unavailable,
            SeekingTarget,
            TargetReached
        }

        private static class Pins
        {
            public static AnalogInput ProbePit;
            public static AnalogInput ProbeFood1;
            //public static OutputPort OnboardLed;
            public static AnalogInput Buttons;
        }

        public static ILcd Lcd;
        private static Thermocouple _thermocouple;
        public static Fan Fan;
        public static PinButton Buttons;

        public static Hashtable Probes;
        public static Hashtable Menus;

        public readonly static int TempMeasurePeriod = 1000; //wait time between temp updates
        public readonly static int TempAverageCount = 8; //number of temps to average

        public static void Initialize()
        {
            Pins.ProbePit = new AnalogInput(FEZCerbuino.Pin.AnalogIn.A0);
            Pins.ProbeFood1 = new AnalogInput(FEZCerbuino.Pin.AnalogIn.A1);
            Pins.Buttons = new AnalogInput(FEZCerbuino.Pin.AnalogIn.A3);
            //Pins.Fan = new OutputPort(FEZCerbuino.Pin.Digital.D6, false);
            //Pins.OnboardLed = new OutputPort(FEZCerbuino.Pin.Digital.LED1, false);
            //Pins.Fan = new PWM(, 50000, 0.0, false);
            //FEZCerbuino.Pin.PWM.D0
            Fan = new Fan(FEZCerbuino.Pin.PWM.A2);

            Probes = new Hashtable() {
                { ProbeType.Pit, new ProbeController(new GadgeteerProbe(_thermocouple), "Pit", ProbeType.Pit, DefaultPitTemp)},
                { ProbeType.Food1, new ProbeController(new PinProbe(Pins.ProbeFood1, PinProbe.ProbeModel.Maverick), "Food", ProbeType.Food1, DefaultFoodTemp) }
            };

            Buttons = new PinButton(Pins.Buttons);

            Menus = new Hashtable() {
                { MenuState.Welcome, new MenuPage(MenuState.Welcome) },
                { MenuState.SetTemp_Pit, new MenuPage(MenuState.Pit)
                        .AddBtn(Button.Left, MenuState.ShowTemps)
                        .AddBtn(Button.Right, MenuState.Food1)
                        .AddBtn(Button.Up, MenuState.SetTemp_Pit)
                        .AddBtn(Button.Down, MenuState.SetTemp_Pit) },
                { MenuState.SetTemp_Food1, new MenuPage(MenuState.Food1)
                        .AddBtn(Button.Left, MenuState.Pit)
                        .AddBtn(Button.Right, MenuState.Reports)
                        .AddBtn(Button.Up, MenuState.SetTemp_Food1)
                        .AddBtn(Button.Down, MenuState.SetTemp_Food1) },
                { MenuState.ShowTemps, new MenuPage(MenuState.ShowTemps)
                        .AddBtn(Button.Left, MenuState.Reports)
                        .AddBtn(Button.Right, MenuState.Pit) },
                { MenuState.Pit, new MenuPage(MenuState.Pit)
                        .AddBtn(Button.Left, MenuState.ShowTemps)
                        .AddBtn(Button.Right, MenuState.Food1)
                        .AddBtn(Button.Up, MenuState.SetTemp_Pit)
                        .AddBtn(Button.Down, MenuState.SetTemp_Pit) },
                { MenuState.Food1, new MenuPage(MenuState.Food1)
                        .AddBtn(Button.Left, MenuState.Pit)
                        .AddBtn(Button.Right, MenuState.Reports)
                        .AddBtn(Button.Up, MenuState.SetTemp_Food1)
                        .AddBtn(Button.Down, MenuState.SetTemp_Food1) },
                { MenuState.Reports, new MenuPage(MenuState.Reports)
                        .AddBtn(Button.Left, MenuState.Food1)
                        .AddBtn(Button.Right, MenuState.ShowTemps)
                        //.AddBtn(Button.Up, MenuState.Report_Pit)
                        //.AddBtn(Button.Down, MenuState.Report_Pit) 
                        },
                //{ MenuState.Report_Pit, new MenuPage(MenuState.Report_Pit)
                //        .AddBtn(Button.Left, MenuState.ShowTemps)
                //        .AddBtn(Button.Right, MenuState.ShowTemps)
                //        .AddBtn(Button.Up, MenuState.Reports)
                //        .AddBtn(Button.Down, MenuState.Reports) },
            };
        }

        public static void SetupLcd(Display_HD44780 display)
        {
            Lcd = new GadgeteerLcd(display);
        }

        internal static void SetupThemocouple(Thermocouple thermocouple)
        {
            _thermocouple = thermocouple;
        }
    }
}
