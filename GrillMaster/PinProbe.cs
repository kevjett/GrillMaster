using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace GrillMaster
{
    public class PinProbe : IProbe
    {
        public static class ProbeModel
        {
            public static double[] Maverick = new double[4] { 2.3067434e-4, 2.3696596e-4, 1.2636414e-7, 1.0e+4 };
        }

        private readonly AnalogInput _input;
        private int _accumulator;
        private int _accumulatedCount;
        private double[] _steinhart;
        private long _lastTempRead = 0;
        private double TemperatureF = -1;
        private double TemperatureR = -1;
        private double TemperatureC = -1;
        private double TemperatureFAvg = -1;
        private double _currentTemp = -1;

        public PinProbe(AnalogInput input, double[] steinhartValues)
        {
            _input = input;
            _accumulator = 0;
            _accumulatedCount = 0;
            TemperatureF = -1;
            TemperatureR = -1;
            TemperatureC = -1;
            TemperatureFAvg = -1;
            _steinhart = steinhartValues;
        }

        public double CurrentTemp
        {
            get { return GetTemp(); }
        }

        private double GetTemp()
        {
            ReadTemp();
            CalculateTemp();
            //CalcTemp2();
            return TemperatureF;
        }

        private void ReadTemp()
        {
            var totalAdc = 0;
            var sampleCount = 100;
            for (int i = 0; i < sampleCount; i++)
            {
                var adc = _input.ReadRaw();
                Debug.Print("Probe Read:" + adc.ToString());
                if (adc == 0 || adc >= 4095)
                {
                    addAdcValue(0);
                    return;
                }
                totalAdc += adc;
            }
            //totalAdc = totalAdc >> 2;
            addAdcValue(totalAdc/sampleCount);
            _lastTempRead = DateTime.Now.Ticks;
        }

        private void CalculateTemp()
        {
            double AdcMax = (1 << (10 + 2)) - 1;
            if (_accumulatedCount != 0)
            {
                int ADCval = _accumulator / _accumulatedCount;
                _accumulatedCount = 0;

                if (ADCval != 0 && ADCval < 4020)  // Vout >= MAX is reduced in readTemp()
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
            if (TemperatureF >= 0)
            {
                CalcExpMovingAverage((1.0f / 20.0f), TemperatureFAvg, TemperatureF);
            }
        }

        private void CalcTemp2()
        {
            TemperatureF = thermister_temp(_input.ReadRaw());
        }

        private int thermister_temp(int aval)
        {
            double R, T;

            Debug.Print("Probe Read:" + aval.ToString());
            // These were calculated from the thermister data sheet
            //	A = 2.3067434E-4;
            //	B = 2.3696596E-4;
            //	C = 1.2636414E-7;
            //
            // This is the value of the other half of the voltage divider
            //	Rknown = 22200;

            // Do the log once so as not to do it 4 times in the equation
            //	R = log(((1024/(double)aval)-1)*(double)22200);
            R = System.Math.Log((1 / ((4096 / (double)aval) - 1)) * (double)15000);
            //lcd.print("A="); lcd.print(aval); lcd.print(" R="); lcd.print(R);
            // Compute degrees C
            T = (1 / ((_steinhart[0]) + (_steinhart[1]) * R + (_steinhart[2]) * R * R * R)) - 273.25;
            // return degrees F
            return ((int)((T * 9.0) / 5.0 + 32.0));
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
