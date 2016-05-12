using System;
using System.Threading;
using HZ.OneWireBus;
using HZ.OneWireBus.Devices;

namespace IA.NetBrew.hardware
{
    /// <summary>
    /// Singleton Class, Handled the One-Wire DS18B20 interface.
    /// 
    /// Ideally the interface will poll all registered onewire DS18B20 probes
    /// On an interval and store the results
    /// </summary>
    public sealed class OneWireTempController : IDisposable
    {
        private static OneWireTempController _instance;
        private static readonly object SyncRoot = new Object();
        private readonly OneWireBus _oneWireBus;

        //Temperature Probes
        private readonly TemperatureDevice _tempProbleMLTRIMS;
        private readonly TemperatureDevice _tempProbleMLTKettle;
        private readonly TemperatureDevice _tempProbeHLTRims;

        //Private temp holding vars

        //Temp Probe holding fields
        public double TempMLTRIMS { get; private set; }
        public double TempMLTKettle { get; private set; }
        public double TempHLTRIMS { get; private set; }

        

        //Thread to fire off for monitoring
        private readonly Thread _tempReader;

        public int TempReadInterval { get; set; }

        private OneWireTempController()
        {
            _oneWireBus = new OneWireBus(Mappings.PinOW1);
            _tempProbleMLTKettle = _oneWireBus.GetDevice(new DeviceAddress(Mappings.RomMashTun)) as TemperatureDevice;
            _tempProbleMLTRIMS = _oneWireBus.GetDevice(new DeviceAddress(Mappings.RomRIMSMashTun)) as TemperatureDevice;
            _tempProbeHLTRims =_oneWireBus.GetDevice(new DeviceAddress(Mappings.RomRIMSHotLiqourTun)) as TemperatureDevice;
            TempReadInterval = 1000;

            //Regardless of whether we are actively brewing or not we're going to constantly monitor 
            //Temps to prove that we actually have a live system :P
            //So let's fire up the temp retrieval thread.  Since Maintaining temp this thread, along with the PWM PID threads
            //will be the highest priority
            _tempReader = new Thread(ReadTemperatures) {Priority = ThreadPriority.AboveNormal};
            _tempReader.Start();
        }

        private static double CelsiusToFarenheit(double celcius)
        {
            return (9.0/5.0)*celcius + 32.0;
        }

        private void ReadTemperatures()
        {
            //Read them, Pause Thread
            while (true)
            {

                TempMLTRIMS = (_tempProbleMLTRIMS != null) ? CelsiusToFarenheit(_tempProbleMLTRIMS.GetTemperature()) : 0.0;
                TempMLTKettle = (_tempProbleMLTKettle != null) ? CelsiusToFarenheit(_tempProbleMLTKettle.GetTemperature()) : 0.0;
                TempHLTRIMS = (_tempProbeHLTRims != null) ? CelsiusToFarenheit(_tempProbeHLTRims.GetTemperature()) : 0.0;

                //Sleep For Probe Interval
                Thread.Sleep(TempReadInterval);
                if (Thread.CurrentThread.ThreadState == ThreadState.AbortRequested)
                    break;
            }
        }


        public bool ProbeExists(byte[] Address)
        {
            return _oneWireBus.DeviceIsKnown(new DeviceAddress(Address));
        }

        public static OneWireTempController Instance
        {
            get
            {
                lock(SyncRoot)
                {
                    return _instance ?? (_instance = new OneWireTempController());
                }
            }
        }


        public void Dispose()
        {
            if (_tempReader != null && _tempReader.ThreadState == ThreadState.Running)
                _tempReader.Abort();


            if (_oneWireBus != null)
                _oneWireBus.Dispose();
        }
    }
}
