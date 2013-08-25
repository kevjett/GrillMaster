using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace GrillMaster
{
    public class Fan : IDisposable
    {
        public enum Speed : int
        {
            Maximum = 0,
            Fast = 0,
            Percent_100 = 0,
            Percent_95 = 5,
            Percent_90 = 10,
            Percent_85 = 15,
            Percent_80 = 20,
            Percent_75 = 25,
            Percent_70 = 30, 
            Percent_65 = 35,
            Percent_60 = 40,
            Percent_55 = 45,
            Half = 50,
            Percent_50 = 50,
            Percent_45 = 55,
            Percent_40 = 60,
            Percent_35 = 65,
            Percent_30 = 70,
            Percent_25 = 75,
            Percent_20 = 80,
            Percent_15 = 85,
            Percent_10 = 90,
            Percent_5 = 95,
            Stopped = 100,
            None = 100
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
            this.frequency = (double)Frequency.kHz_10; 
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
