using System;
using Microsoft.SPOT;

namespace GrillMaster
{
    public interface ILcd
    {
        bool Debug { get; set; }
        void Clear();
        void CursorHome();
        void Initialize();
        void PrintString(string str);
        void Putc(byte c);
        void SetBacklight(bool bOn);
        void SetCursor(byte col, byte row);
    }
}
