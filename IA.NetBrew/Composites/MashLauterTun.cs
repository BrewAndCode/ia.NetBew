using System;
using System.Threading;
using GHIElectronics.NETMF.Hardware;
using IA.NetBrew.Abstracts;
using IA.NetBrew.hardware;

namespace IA.NetBrew.Composites
{
    /// <summary>
    ///  Singleton Class, Handles Processing and state tracking of the Mash Lauter Tun
    /// </summary>
    public class MashLauterTun : RIMSKettleBase
    {
        //Single Instancing
        private static MashLauterTun _instance;
        private static readonly object SyncRoot = new Object();

        /// <summary>
        /// Constructor, Initialize Variables and Spawn Thread
        /// </summary>
        public MashLauterTun()
        {
            //Setup Initial settings
            BTPD = new BTPD(Mappings.BTPDMashTun);
            BTPD.ShowInit(); 
            _isHeating = false;
            _tempSet = 0.0;
            GasThresholdUp = 0.50;
            GasThresholdDown = 3.00;

            //Setup LCD
            LCD = new NewHavenSerialLCD(40, 100) {Underline = false};
            LCD.Clear();
            LCD.ShowFirmware();
            LCD.Underline = true;
            LCD.Display = true;
            LCD.SetContrast(0x18);
            LCD.SetBacklight(0x08);
           
            //LCD.ShowFirmware();
            
            LCD.WriteLine(NewHavenSerialLCD.Line.Line1, "STAGE: IDLE         ");
            LCD.WriteLine(NewHavenSerialLCD.Line.Line2, "RIMS:999.9 TUN:999.9");
            LCD.WriteLine(NewHavenSerialLCD.Line.Line3, "PID: 100  TIME: 999m");
            LCD.WriteLine(NewHavenSerialLCD.Line.Line4, "RIM/TUN D: 999.9    ");

            //Config PID
            Pid = new PID(TempActualRims,DutyCycle,_tempSet,_kc,_tauR,_tauD);
            Pid.Reset();
            Pid.SetSampleTime(2000);
            Pid.SetMode(1);
            //Config PWM
            _rimsPwm = new PWM(Mappings.PinRIMSMLT);
            //Spawn Thread             
            _display = new Thread(Calculate) {Priority = ThreadPriority.AboveNormal};
            _display.Start();
        }

        protected override double GetRIMSTemp()
        {
            return OneWireTempController.Instance.TempMLTRIMS;
        }
        protected override double GetKettleTemp()
        {
            return OneWireTempController.Instance.TempMLTKettle;
        }
        protected override bool ProbeExist()
        {
            return OneWireTempController.Instance.ProbeExists(Mappings.RomRIMSMashTun);
        }
        public static MashLauterTun Instance
        {
            get
            {
               if (_instance==null)
                lock (SyncRoot)
                {
                    if(_instance==null)
                       _instance = new MashLauterTun();
                }
               return _instance;
            }
        }
    }
}
