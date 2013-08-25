using System;
using Microsoft.SPOT;
using Gadgeteer.Modules.GHIElectronics;

namespace GrillMaster
{
    public class GadgeteerProbe : IProbe
    {
        private Thermocouple _thermocouple;

        public GadgeteerProbe(Thermocouple thermocouple)
        {
            _thermocouple = thermocouple;
        }

        public double CurrentTemp
        {
            get { return _thermocouple.GetExternalTemp_Fahrenheit(); }
        }
    }
}
