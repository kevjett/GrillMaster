using System;
using Microsoft.SPOT;
using Gadgeteer.Modules.GHIElectronics;
using MicroLiquidCrystal;
using Microsoft.SPOT.Hardware;
using GHI.OSHW.Hardware;

namespace GrillMaster
{
    public class PinLcd : ILcd
    {
        private RawLcd _display;

        public PinLcd(Cpu.Pin rs, Cpu.Pin enable, Cpu.Pin d4, Cpu.Pin d5, Cpu.Pin d6, Cpu.Pin d7)
        {
            var lcdProvider = new GpioLcdTransferProvider(
                FEZCerbuino.Pin.Digital.D7,  // RS
                FEZCerbuino.Pin.Digital.D8, // enable
                FEZCerbuino.Pin.Digital.D9,  //d4
                FEZCerbuino.Pin.Digital.D10,  //d5
                FEZCerbuino.Pin.Digital.D11,  //d6
                FEZCerbuino.Pin.Digital.D12); // d7

            _display = new RawLcd(lcdProvider);
            Initialize();
        }

        public bool Debug { get { return _display.DebugMode; } set { _display.DebugMode = value; } }

        public void Clear()
        {
            _display.Clear();
        }

        public void CursorHome()
        {
            _display.SetCursorPosition(0, 0);
        }

        public void Initialize()
        {
            _display.Begin(16, 2);
        }

        public void PrintString(string str)
        {
            _display.Write(str);
        }

        public void Putc(byte c)
        {
            _display.WriteByte(c);
        }

        public void SetBacklight(bool bOn)
        {
            _display.Backlight = bOn;
        }

        public void SetCursor(byte col, byte row)
        {
            _display.SetCursorPosition(col, row);
        }
    }
}
