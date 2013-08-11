using System;
using Microsoft.SPOT;
using Gadgeteer.Modules.GHIElectronics;

namespace GrillMaster
{
    public class GadgeteerLcd : ILcd
    {
        private Display_HD44780 _display;

        public GadgeteerLcd(Display_HD44780 display)
        {
            _display = display;
        }

        public bool Debug { get { return _display.DebugPrintEnabled; } set { _display.DebugPrintEnabled = value; } }

        public void Clear()
        {
            _display.Clear();
        }

        public void CursorHome()
        {
            _display.CursorHome();
        }

        public void Initialize()
        {
            _display.Initialize();
        }

        public void PrintString(string str)
        {
            _display.PrintString(str);
        }

        public void Putc(byte c)
        {
            _display.Putc(c);
        }

        public void SetBacklight(bool bOn)
        {
            _display.SetBacklight(bOn);
        }

        public void SetCursor(byte col, byte row)
        {
            _display.SetCursor(row, col);
        }
    }
}
