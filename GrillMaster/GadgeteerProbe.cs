using System;
using Microsoft.SPOT;
using Gadgeteer.Modules.GHIElectronics;

namespace GrillMaster
{
    public class GadgeteerProbe : Probe, IProbe
    {
        private Thermocouple _thermocouple;

        public GadgeteerProbe(string name, Config.ProbeType probeType, int startingTargetTemp, Thermocouple thermocouple)
            : base(name, probeType, startingTargetTemp)
        {
            _thermocouple = thermocouple;
        }

        public override double GetTemp()
        {
            return _thermocouple.GetExternalTemp_Fahrenheit();
        }
    }
}
