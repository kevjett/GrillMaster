using System;
using System.Collections;
using System.Threading;
using GHI.OSHW.Hardware;
using MicroLiquidCrystal;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace GrillMaster
{
    public static class Config
    {
        public const int WelcomeWait = 10; //secs to wait for welcome
        public const int DefaultPitTemp = 300; //default target temp for pit
        public const int DefaultFoodTemp = 160; //default target temp for food
        public const int LidOpenAutoResume = 30; //number of seconds to wait after temp has been reached and temp goes below target
        public const int LidOpenOffset = 6; //The percentage the temperature drops before automatic lidopen mode

        public enum ProbeType
        {
            Pit = 0,
            Food1 = 1
        }

        public static class Pins
        {
            public static AnalogInput ProbePit;
            public static AnalogInput ProbeFood1;
            public static OutputPort OnboardLed;
            public static OutputPort Fan;
            public static AnalogInput Buttons;
        }

        public static Lcd Lcd;

        public static Hashtable Probes;
        public static Hashtable Menus;

        public readonly static int TempMeasurePeriod = 1000; //wait time between temp updates
        public readonly static int TempAverageCount = 8; //number of temps to average

        public static void Initialize()
        {
            Pins.ProbePit = new AnalogInput(FEZCerbuino.Pin.AnalogIn.A0);
            Pins.ProbeFood1 = new AnalogInput(FEZCerbuino.Pin.AnalogIn.A1);
            Pins.Buttons = new AnalogInput(FEZCerbuino.Pin.AnalogIn.A2);
            Pins.OnboardLed = new OutputPort(FEZCerbuino.Pin.Digital.LED1, false);
            Pins.Fan = new OutputPort(FEZCerbuino.Pin.Digital.D5, false);

            var lcdProvider = new GpioLcdTransferProvider(
                FEZCerbuino.Pin.Digital.D12,  // RS
                FEZCerbuino.Pin.Digital.D11, //Pins.GPIO_PIN_D11,  // enable
                FEZCerbuino.Pin.Digital.D5,  //d4
                FEZCerbuino.Pin.Digital.D4,  //d5
                FEZCerbuino.Pin.Digital.D3,  //d6
                FEZCerbuino.Pin.Digital.D2); // d7

            Lcd = new Lcd(lcdProvider);
            Lcd.Begin(16, 2);

            Probes = new Hashtable() {
                { ProbeType.Pit, new TempProbe("Pit", ProbeType.Pit, Pins.ProbePit, DefaultPitTemp)},
                { ProbeType.Food1, new TempProbe("Food", ProbeType.Food1, Pins.ProbeFood1, DefaultFoodTemp) }
            };

            Menus = new Hashtable() {
                { MenuState.Welcome, new MenuPage(MenuState.Welcome) },
                //{ MenuState.SetTemp_Pit, new MenuPage(MenuState.Pit)
                //        .AddBtn(Button.Left, MenuState.ShowTemps)
                //        .AddBtn(Button.Down, MenuState.Food1)
                //        .AddBtn(Button.Up, MenuState.SetTemp_Pit)
                //        .AddBtn(Button.Down, MenuState.SetTemp_Pit) },
                //{ MenuState.SetTemp_Food1, new MenuPage(MenuState.Food1)
                //        .AddBtn(Button.Left, MenuState.Pit)
                //        .AddBtn(Button.Down, MenuState.Reports)
                //        .AddBtn(Button.Up, MenuState.SetTemp_Food1)
                //        .AddBtn(Button.Down, MenuState.SetTemp_Food1) },
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
                        .AddBtn(Button.Up, MenuState.Report_Pit)
                        .AddBtn(Button.Down, MenuState.Report_Pit) },
                { MenuState.Report_Pit, new MenuPage(MenuState.Report_Pit)
                        .AddBtn(Button.Left, MenuState.ShowTemps)
                        .AddBtn(Button.Right, MenuState.ShowTemps)
                        .AddBtn(Button.Up, MenuState.Reports)
                        .AddBtn(Button.Down, MenuState.Reports) },
            };
        }
    }
}
