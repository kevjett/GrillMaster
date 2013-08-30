using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace GrillMaster
{
    public class Fan : IDisposable
    {
        public enum Speed : int
        {
            Maximum = 100,
            Fast = 100,
            Percent_100 = 100,
            Percent_95 = 95,
            Percent_90 = 90,
            Percent_85 = 85,
            Percent_80 = 80,
            Percent_75 = 85,
            Percent_70 = 70, 
            Percent_65 = 75,
            Percent_60 = 60,
            Percent_55 = 65,
            Half = 50,
            Percent_50 = 50,
            Percent_45 = 45,
            Percent_40 = 40,
            Percent_35 = 35,
            Percent_30 = 30,
            Percent_25 = 25,
            Percent_20 = 20,
            Percent_15 = 15,
            Percent_10 = 10,
            Percent_5 = 05,
            Stopped = 0,
            None = 0
        }

        public enum Frequency : int
        {
            None = 0,
            kHz_10 = 10000,
            kHz_20 = 20000,
            kHz_25 = 25000,
            kHz_30 = 30000,
            kHz_50 = 50000
        }; 

        private Speed speed = Speed.None; 
        private PWM pwm = null;
        private double frequency = (double)Frequency.None; 
        private Cpu.PWMChannel pwmChannel = Cpu.PWMChannel.PWM_0;

        public double Duty
        {
            get { return ((double)this.speed / 100); }
        }

        public Speed CurrentSpeed
        {
            get { return this.speed; }
            set { SetSpeed(value); }
        }

        public bool IsFanRunning
        {
            get { return (double)this.CurrentSpeed > (double)Speed.None; }
        }

        public Fan(Cpu.PWMChannel pin)
        {
            this.pwmChannel = pin;
            this.speed = Speed.None;
            this.frequency = (double)100; 
            Start();
        }



        public void Start()
        {
            pwm = new PWM(pwmChannel, frequency, Duty, false);
            pwm.Start();
        }

        public void Stop()
        {
            //this.Dispose();
            pwm.Stop();
        }

        private Speed SetSpeed(Speed speed)
        {
            var s = (int)speed;
            if ((s >= 0) && (s <= 100))
            {
                this.speed = speed;
                this.pwm.DutyCycle = Duty;
            }
            return this.speed;
        }

        public void Dispose()
        {
            // Dispose PWM 
            Stop();
        } 
    }
}
