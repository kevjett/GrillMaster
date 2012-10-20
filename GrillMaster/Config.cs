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
            Pins.OnboardLed = new OutputPort(FEZCerbuino.Pin.Digital.LED1, false);

            var lcdProvider = new GpioLcdTransferProvider(
                FEZCerbuino.Pin.Digital.D12,  // RS
                FEZCerbuino.Pin.Digital.D11, //Pins.GPIO_PIN_D11,  // enable
                FEZCerbuino.Pin.Digital.D2,  //d4
                FEZCerbuino.Pin.Digital.D3,  //d5
                FEZCerbuino.Pin.Digital.D4,  //d6
                FEZCerbuino.Pin.Digital.D5); // d7

            Lcd = new Lcd(lcdProvider);
            Lcd.Begin(16, 2);

            Probes = new Hashtable() {
                { ProbeType.Pit, new TempProbe("Pit", ProbeType.Pit, Pins.ProbePit)},
                { ProbeType.Food1, new TempProbe("Food 1", ProbeType.Food1, Pins.ProbeFood1) }
            };

            Menus = new Hashtable() {
                { MenuState.Welcome, new MenuPage(MenuState.Welcome) },
                { MenuState.SetTemp_Pit, new MenuPage(MenuState.SetTemp_Pit)
                        .AddBtn(Button.Up, MenuState.SetTemp_Pit)
                        .AddBtn(Button.Down, MenuState.SetTemp_Pit)
                        .AddBtn(Button.Middle, MenuState.SetTemp_Pit) },
                { MenuState.SetTemp_Food1, new MenuPage(MenuState.SetTemp_Food1)
                        .AddBtn(Button.Up, MenuState.SetTemp_Food1)
                        .AddBtn(Button.Down, MenuState.SetTemp_Food1)
                        .AddBtn(Button.Middle, MenuState.SetTemp_Food1) },
                { MenuState.Food1, new MenuPage(MenuState.Food1)
                        .AddBtn(Button.Up, MenuState.Reports)
                        .AddBtn(Button.Down, MenuState.Pit)
                        .AddBtn(Button.Middle, MenuState.SetTemp_Food1) },
                { MenuState.Pit, new MenuPage(MenuState.Pit)
                        .AddBtn(Button.Up, MenuState.Food1)
                        .AddBtn(Button.Down, MenuState.Reports)
                        .AddBtn(Button.Middle, MenuState.SetTemp_Pit) },
                { MenuState.Reports, new MenuPage(MenuState.Reports)
                        .AddBtn(Button.Up, MenuState.Pit)
                        .AddBtn(Button.Down, MenuState.Food1)
                        .AddBtn(Button.Middle, MenuState.Report_Pit) },
                { MenuState.Report_Pit, new MenuPage(MenuState.Report_Pit)
                        .AddBtn(Button.Up, MenuState.Food1)
                        .AddBtn(Button.Down, MenuState.Food1)
                        .AddBtn(Button.Middle, MenuState.Food1) },
            };
        }
    }
}
