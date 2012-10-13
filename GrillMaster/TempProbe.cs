using Microsoft.SPOT.Hardware;

namespace GrillMaster
{
    public class TempProbe
    {
        private readonly AnalogInput _aInput;
        private int _accumulator;
        private int _accumulatedCount;
        private double[] _steinhart;
        public double TemperatureF { get; private set; }
        public double TemperatureR { get; private set; }
        public double TemperatureC { get; private set; }
        public double TemperatureFAvg { get; private set; }
        public bool HasTemperature { get { return TemperatureF >= 0; } }
        public string Name { get; private set; }

        public TempProbe(string name, Cpu.AnalogChannel pin)
        {
            Name = name;
            _aInput = new AnalogInput(pin);
            _accumulator = 0;
            _accumulatedCount = 0;
            TemperatureF = -1;
            TemperatureR = -1;
            TemperatureC = -1;
            TemperatureFAvg = -1;
            _steinhart = new double[4] { 2.3067434e-4, 2.3696596e-4, 1.2636414e-7, 1.0e+4 };
        }

        public void ReadTemp()
        {
            var totalAdc = 0;
            var sampleCount = 100;
            for (int i = 0; i < sampleCount; i++)
            {
                var adc = _aInput.ReadRaw();
                if (adc == 0 || adc >= 4095)
                {
                    addAdcValue(0);
                    return;
                }
                totalAdc += adc;
            }
            //totalAdc = totalAdc >> 2;
            addAdcValue(totalAdc/sampleCount);
        }

        public void CalculateTemp()
        {
            double AdcMax = (1 << (10 + 2)) - 1;
            if (_accumulatedCount != 0)
            {
                int ADCval = _accumulator / _accumulatedCount;
                _accumulatedCount = 0;

                if (ADCval != 0)  // Vout >= MAX is reduced in readTemp()
                {
                    double R;
                    double T;
                    // If you put the fixed resistor on the Vcc side of the thermistor, use the following
                    R = _steinhart[3] / ((AdcMax / (float)ADCval) - 1.0);
                    // If you put the thermistor on the Vcc side of the fixed resistor use the following
                    //R = _steinhart[3] * AdcMax / (float)ADCval - _steinhart[3];

                    TemperatureR = R;

                    // Compute degrees K
                    R = System.Math.Log(R);
                    T = 1.0f / ((_steinhart[2] * R * R + _steinhart[1]) * R + _steinhart[0]);

                    SetTemperature(T - 273.15);
                } /* if ADCval */
                else
                {
                    TemperatureF = -1;
                    TemperatureC = -1;
                    TemperatureR = -1;
                }
            }
            if (this.HasTemperature)
            {
                //Temperature += Offset;
                CalcExpMovingAverage((1.0f / 20.0f), TemperatureFAvg, TemperatureF);
                //Alarms.updateStatus(TemperatureF);
            }
            //else
                //Alarms.silenceAll();
        }

        private void CalcExpMovingAverage(float smoothing, double currentAvg, double newTemp)
        {
            if (currentAvg < 0)
                currentAvg = newTemp;
            else
            {
                newTemp = newTemp - currentAvg;
                currentAvg = currentAvg + (smoothing * newTemp);
            }
        }

        private void addAdcValue(int analogTemp)
        {
            if (analogTemp == 0) // >= MAX is reduced in readTemp()
                _accumulator = 0;
            else if (_accumulatedCount == 0)
                _accumulator = analogTemp;
            else if (_accumulator != 0)
                _accumulator += analogTemp;
            ++_accumulatedCount;
        }

        private void SetTemperature(double T)
        {
            // Sanity - anything less than -20C (-4F) or greater than 500C (932F) is rejected
            if (T <= -20.0f || T > 500.0f)
            {
                TemperatureF = -2;
                TemperatureC = -2;
            }
            TemperatureF = (T * (9.0f / 5.0f)) + 32.0f;
            TemperatureC = T;
        }
    }
}
