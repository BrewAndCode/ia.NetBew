using System;
using System.Threading;
using GHIElectronics.NETMF.Hardware;
using IA.NetBrew.Helpers;
using IA.NetBrew.hardware;

namespace IA.NetBrew.Abstracts
{
    public abstract class RIMSKettleBase : IDisposable
    {
        public enum Stage
        {
            Idle = 0,
            Ramping,
            Maintain,
            Decline
        }

        protected double GasThresholdUp;
        protected double GasThresholdDown;

        protected NewHavenSerialLCD LCD;    
       
        protected PID Pid;
        protected double _kc = 600;
        protected double _tauR = 250;
        protected double _tauD = 1;
        protected bool _isHeating;
        protected double _tempSet;
        protected bool _alarm;
        public Stage CurrentStage = Stage.Idle;
        public GasSolenoid Gas;
        public BTPD BTPD;                //Brewtroller PID Display element
        protected PWM _rimsPwm;
        protected Thread _display;

        protected abstract double GetRIMSTemp();
        protected abstract double GetKettleTemp();
        protected abstract bool ProbeExist();

        public double Kc
        {
            get { return _kc; }
            set { 
                _kc = value; 
                Pid.SetTunings(_kc,_tauR,_tauD);
            }
        }
        public double TauR
        {
            get { return _tauR; }
            set { 
                _tauR = value;
                Pid.SetTunings(_kc,_tauR,_tauD);
            }
        }
        public double TauD
        {
            get { return _tauD; }
            set { 
                _tauD = value;
                Pid.SetTunings(_kc,_tauR,_tauD);
            }
        }

        public bool IsHeating {
            get { return _isHeating; } 
            set
            {
                _isHeating = value;
                if (value)
                    CurrentStage = TempActualRims < (TempSet-GasThresholdUp) ? Stage.Ramping : Stage.Maintain;
                else
                {
                    CurrentStage = Stage.Idle;
                }
            }
        }

        public double DutyCycle { get; protected set; }

        public double TempSet
        {
            get { return _tempSet; }
            set
            {
                _tempSet = value;
                Pid.mySetpoint = _tempSet;
            }
        }

        public double TempActualRims { get; protected set; }
        public double TempActualKettle { get; protected set; }

        protected void UpdateLCD()
        {
            if (LCD == null)
                return;
            var rimsTemp = StringHelper.PadLeft(TempActualRims.ToString("N1"), 4);
            var tunTemp = StringHelper.PadLeft(TempActualKettle.ToString("N1"), 4);
            LCD.SetPosition(0x45);
 
            LCD.Write(rimsTemp);
            LCD.SetPosition(0x4F);
            LCD.Write(tunTemp);

            LCD.SetPosition(0x18);
            var dutyString = StringHelper.PadLeft(DutyCycle.ToString("N1"), 4);
            LCD.Write(dutyString);

            var delta = TempActualRims - TempActualKettle;
            if (delta < 0)
                delta = delta*-1.0;
            var deltaString = StringHelper.PadLeft(delta.ToString("N1"), 4);
            LCD.SetPosition(0x5E);
            LCD.Write(deltaString);
   
            LCD.SetPosition(0x7);
            switch (CurrentStage)
            {
                case Stage.Idle:
                    LCD.Write("IDLE   ");
                    break;
                case Stage.Ramping:
                    LCD.Write("RAMPING");
                    break;
                case Stage.Maintain:
                    LCD.Write("MAINTNG");
                    break;
            }
        }

        /// <summary>
        /// Sets the tunings on the PID controller for the Mash Tun's Element
        /// </summary>
        /// <param name="kc">KC</param>
        /// <param name="tauR">tau R</param>
        /// <param name="tauD">tau D</param>
        public void SetTunings(double kc, double tauR, double tauD)
        {
            _kc = kc;
            _tauR = tauR;
            _tauD = tauD;
            if (Pid != null)
            {
                Pid.SetTunings(_kc,_tauR,_tauD);
            }
        }

        public void RaiseAlarm()
        {
            if (_alarm)
            {
                _alarm = false;
            }
        }

        protected void Calculate()
        {
            while(true)
            {
                //Get Actual and update Display
                TempActualRims = GetRIMSTemp();
                TempActualKettle = GetKettleTemp();
                BTPD.TempActual = TempActualRims;
                BTPD.TempSet = TempSet;
                BTPD.Update();
             
                if (!ProbeExist())
                {
                    BTPD.ShowErr("prbe");
                    _alarm = true;
                    RaiseAlarm();
                }
                else if (_isHeating && TempActualRims != 0.0)
                {
                    //Update PID
                    Pid.myInput = TempActualRims;
                    Pid.Compute();
                    DutyCycle = Pid.myOutput;

                    if (TempActualRims >= Mappings.RIMSShutoffTemp || TempActualRims < 0.00)  
                    {
                        //Shut off RIMS
                        _isHeating = false;
                        _rimsPwm.SetPulse(0,0);
                        CurrentStage=Stage.Idle;
                        _alarm = true;
                        RaiseAlarm();
                    }
                    else
                    {
                        var delta = (TempSet - TempActualRims);
                        if (CurrentStage==Stage.Ramping)
                        {
                            if(delta <= GasThresholdUp)
                                CurrentStage=Stage.Maintain;
                        }
                        if (CurrentStage==Stage.Maintain)
                            if (delta >= GasThresholdDown)
                                CurrentStage=Stage.Ramping;
                        if (Gas != null) Gas.State=(CurrentStage==Stage.Ramping);
                    }
                    //set duty Cycle on Element PWR
                    var cycleTime = (uint)(Mappings.RIMSMLTPulseLength*(DutyCycle/100));
                    _rimsPwm.SetPulse(Mappings.RIMSMLTPulseLength, cycleTime);
                }
                else
                {
                    //Shut down Element
                    _rimsPwm.SetPulse(0,0);
                    DutyCycle = 0;
                    if (Gas != null) Gas.Off();
                }
                if(LCD!=null)
                    UpdateLCD();
                Thread.Sleep(1000);
            }
        }

        public void Dispose()
        {
            if (_display != null && _display.ThreadState == ThreadState.Running)
                _display.Abort();
            if (_rimsPwm != null)
                _rimsPwm.Dispose();
        }
    }
}