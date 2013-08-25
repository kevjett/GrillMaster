using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace GrillMaster
{
    public class PinButton
    {
        private AnalogInput _input;

        public PinButton(AnalogInput input)
        {
            _input = input;
        }

        public int Read()
        {
            return _input.ReadRaw();
        }
    }
}
